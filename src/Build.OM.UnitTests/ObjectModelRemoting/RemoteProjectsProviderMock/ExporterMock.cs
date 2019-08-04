namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using Microsoft.Build.BackEnd;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Evaluation.Context;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Shared;
    using Microsoft.Build.ObjectModelRemoting;
    using Shouldly;
    using Xunit;

    using ExportedLinksMap = LinkedObjectsMap<object>;
    using ImportedLinksMap = LinkedObjectsMap<System.UInt32>;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    // Typical flow for "linked object" of type "Foo"
    // [ ---  Client Process                                    ]                           [ Server process (can be different) ] 
    // (Foo) localView <=> (FooLink) link <=> FooLinkRemoter (Proxy) <=~connection mechanism~=> FooLinkRemoter(stub) <=> (Real object)
    //
    // FooLinkRemoter would be whatever ExternalProviders see useful to provide FooLink implementation, it might be completely different interface.
    // (note some link types would be inconvenient /impossible to serialize for example and pass cross process).
    // For the purpose of unit tests we know Client and Server are on the same process, so we can cheat and combine the proxy/connection/stub to a single object.
    // we'll call FooRemoter. We want to make sure FooRemoter only use a simple type (generally only serializable types) to ensure the
    // actual implementation is possible.

    internal interface IRemoteLinkId
    {
        UInt32 HostCollectionId { get; }
        UInt32 LocalObjectId { get; }
    }

    internal class LinkedObjectsWithId<T, R> : ExportedLinksMap.LinkedObject<T>, IRemoteLinkId
    {
        public UInt32 HostCollectionId { get; }
        public UInt32 LocalObjectId { get; }

        public R Remoter { get; set; }
    }

    internal class LinkRemoterMock : IRemoteLinkId
    {
        public UInt32 HostCollectionId { get; }
        public UInt32 LocalObjectId { get; }
    }

    internal abstract class LinkRemoterMock<T, L, LMock> : ExportedLinksMap.LinkedObject<T>, IRemoteLinkId // LinkRemoterMock
        where T : class
    {
        public abstract L CreateLink();

        public ProjectCollectionLinker OwningCollection { get; private set; }

        public override void Initialize(object key, T source, object context)
        {
            base.Initialize(key, source, context);
            this.OwningCollection = (ProjectCollectionLinker)context;
        }

        public UInt32 HostCollectionId => this.OwningCollection.CollectionId;
        public UInt32 LocalObjectId => this.LocalId;

        public static void Export<RMock>(ProjectCollectionLinker linker, T t, out RMock remoter)
            where RMock : LinkRemoterMock<T, L, LMock>, new()
        {
            linker.Export<T, L, RMock, LMock>(t, out remoter);
        }
    }

    internal class LinkMock
    {
    }

    internal class LinkMock<T, L, RMock> : LinkMock
    {
    }

    internal class LinkProxy<T, L, RMock> : ImportedLinksMap.LinkedObject<RMock>
    {
        public override void Initialize(uint key, RMock source, object context)
        {
            base.Initialize(key, source, context);

            var pcl = (Func<RMock, T>)context;
            this.Remoter = source;
            this.Linked = pcl(source);
        }

        public T Linked { get; protected set; }
        public RMock Remoter {get; protected set; }
    }


    internal class ProjectLinkRemoter : LinkRemoterMock<Project, ProjectLink, ProjectLinkMock>
    {
        public override ProjectLink CreateLink()
        {
            throw new NotImplementedException();
        }
    }


    internal interface ILinkMock
    {
        object Remoter { get; }
    }

    internal class PrjLinkMock : ProjectLink , ILinkMock
    {
        public PrjLinkMock(ProjectLinkRemoter proxy)
        {
            this.Proxy = proxy;
        }

        public ProjectLinkRemoter Proxy { get; }
        object ILinkMock.Remoter => this.Proxy;

    }



    internal class ProjectLinkMock : LinkMock<Project, ProjectLink, ProjectLinkRemoter>
    {

    }


    class ProjectCollectionLinker
    {
        public UInt32 CollectionId { get; }
        public ProjectCollection Collection { get; }
        public LinkedObjectsFactory LinkFactory { get; }

        private ExportedLinksMap exported = ExportedLinksMap.Create();
        private Dictionary<UInt32, ImportedLinksMap> imported = new Dictionary<UInt32, ImportedLinksMap>();

        public void ConnectTo (ProjectCollectionLinker other)
        {
            if (other.CollectionId == this.CollectionId)
            {
                throw new Exception("Can not connect to self");
            }

            lock (imported)
            {
                if (imported.ContainsKey(other.CollectionId))
                {
                    return;
                }

                // clone for so we are atomic.
                // we don't have to be efficient here on "Connect" there very few calls.
                // compared to potentially 1000's of accesses (so it is better to copy that to lock access)
                Dictionary<UInt32, ImportedLinksMap> newMap = new Dictionary<uint, ImportedLinksMap>(imported);
                newMap.Add(other.CollectionId, ImportedLinksMap.Create());
                imported = newMap;
            }
        }

        public T Import<T, L, RMock, LMock>(RMock remoter)
            where T : class
            where RMock : LinkRemoterMock<T, L, LMock>, new()
        {
            if (remoter == null)
            {
                return null;
            }

            if (remoter.HostCollectionId == this.CollectionId)
            {
                this.exported.GetActive(remoter.LocalId, out T result);
                return result;
            }

            if (!imported.TryGetValue(remoter.HostCollectionId, out var perRemoteCollection))
            {
                throw new Exception("Not connected!");
            }

            LinkProxy<T, L, RMock> proxy;
            perRemoteCollection.GetOrCreate(remoter.LocalId, remoter, (r)=>   out proxy, slow : true);

            return proxy.Linked;
        }

        public void Export<T, L, RMock, LMock>(T obj, out RMock remoter)
            where T : class
            where RMock : LinkRemoterMock<T, L, LMock>, new()
        {
            if (obj == null)
            {
                remoter = null;
                return;
            }

            var external = this.LinkFactory.GetLink(obj);

            if (external != null)
            {
                var proxy = (ILinkMock)external;

                remoter = (RMock) proxy.Remoter;
                return;
            }

            exported.GetOrCreate(obj, obj, this, out remoter);
        }
    }

    public static class CompileTest
    {
        public static void T()
        {
            ProjectCollectionLinker pcl = new ProjectCollectionLinker();
            Project p = null;
            // pcl.Export(p, out ProjectLinkRemoter remoter);
            ProjectLinkRemoter remoter;
            ProjectLinkRemoter.Export(pcl, p, out remoter);
            pcl.Export<Project, ProjectLink, ProjectLinkRemoter, ProjectLinkMock>(p, out remoter);

        }
    }


}

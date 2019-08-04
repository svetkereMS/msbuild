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
    using Microsoft.Build.Framework;
    using Microsoft.Build.Logging;

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

    internal abstract class LinkRemoterMock<T, L> : ExportedLinksMap.LinkedObject<T>, IRemoteLinkId // LinkRemoterMock
        where T : class
        where L : class
    {
        public abstract T CreateLinkedObject(ProjectCollectionLinker remote);

        public ProjectCollectionLinker OwningCollection { get; private set; }

        public override void Initialize(object key, T source, object context)
        {
            base.Initialize(key, source, context);
            this.OwningCollection = (ProjectCollectionLinker)context;
        }

        public UInt32 HostCollectionId => this.OwningCollection.CollectionId;
        public UInt32 LocalObjectId => this.LocalId;

        public static void Export<RMock>(ProjectCollectionLinker linker, T t, out RMock remoter)
            where RMock : LinkRemoterMock<T, L>, new()
        {
            linker.Export<T, L, RMock>(t, out remoter);
        }
    }

    internal class LinkProxy<T, L, RMock> : ImportedLinksMap.LinkedObject<RMock>
        where T : class
        where L : class
        where RMock : LinkRemoterMock<T, L>
    {
        public override void Initialize(uint key, RMock source, object context)
        {
            base.Initialize(key, source, context);

            this.Remoter = source;
            this.Linker = (ProjectCollectionLinker) context;
            this.Linked = source.CreateLinkedObject(this.Linker);
        }

        public ProjectCollectionLinker Linker { get; private set; }

        public T Linked { get; protected set; }
        public RMock Remoter {get; protected set; }
    }


    internal class ProjectLinkRemoter : LinkRemoterMock<Project, ProjectLink>
    {
        public override Project CreateLinkedObject(ProjectCollectionLinker remote)
        {
            var link = new PrjLinkMock(this);
            return remote.LinkFactory.Create(link);
        }
    }


    internal interface ILinkMock<T, L>
    {
        object Remoter { get; }
    }

    internal class PrjLinkMock : ProjectLink , ILinkMock<Project, ProjectLink>
    {
        public PrjLinkMock(ProjectLinkRemoter proxy)
        {
            this.Proxy = proxy;
        }

        public ProjectLinkRemoter Proxy { get; }

        #region ProjectLink
        #region NotImpl
        public override ProjectRootElement Xml => throw new NotImplementedException();

        public override bool ThrowInsteadOfSplittingItemElement { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override bool IsDirty => throw new NotImplementedException();

        public override IDictionary<string, string> GlobalProperties => throw new NotImplementedException();

        public override ICollection<string> ItemTypes => throw new NotImplementedException();

        public override ICollection<ProjectProperty> Properties => throw new NotImplementedException();

        public override IDictionary<string, List<string>> ConditionedProperties => throw new NotImplementedException();

        public override IDictionary<string, ProjectItemDefinition> ItemDefinitions => throw new NotImplementedException();

        public override ICollection<ProjectItem> Items => throw new NotImplementedException();

        public override ICollection<ProjectItem> ItemsIgnoringCondition => throw new NotImplementedException();

        public override IList<ResolvedImport> Imports => throw new NotImplementedException();

        public override IList<ResolvedImport> ImportsIncludingDuplicates => throw new NotImplementedException();

        public override IDictionary<string, ProjectTargetInstance> Targets => throw new NotImplementedException();

        public override ICollection<ProjectProperty> AllEvaluatedProperties => throw new NotImplementedException();

        public override ICollection<ProjectMetadata> AllEvaluatedItemDefinitionMetadata => throw new NotImplementedException();

        public override ICollection<ProjectItem> AllEvaluatedItems => throw new NotImplementedException();

        public override string ToolsVersion => throw new NotImplementedException();

        public override string SubToolsetVersion => throw new NotImplementedException();

        public override bool SkipEvaluation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool DisableMarkDirty { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool IsBuildEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override int LastEvaluationId => throw new NotImplementedException();

        object ILinkMock<Project, ProjectLink>.Remoter => this.Proxy;

        public override IList<ProjectItem> AddItem(string itemType, string unevaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata)
        {
            throw new NotImplementedException();
        }

        public override IList<ProjectItem> AddItemFast(string itemType, string unevaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata)
        {
            throw new NotImplementedException();
        }

        public override bool Build(string[] targets, IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers, EvaluationContext evaluationContext)
        {
            throw new NotImplementedException();
        }

        public override ProjectInstance CreateProjectInstance(ProjectInstanceSettings settings, EvaluationContext evaluationContext)
        {
            throw new NotImplementedException();
        }

        public override string ExpandString(string unexpandedValue)
        {
            throw new NotImplementedException();
        }

        public override List<GlobResult> GetAllGlobs(EvaluationContext evaluationContext)
        {
            throw new NotImplementedException();
        }

        public override List<GlobResult> GetAllGlobs(string itemType, EvaluationContext evaluationContext)
        {
            throw new NotImplementedException();
        }

        public override List<ProvenanceResult> GetItemProvenance(string itemToMatch, EvaluationContext evaluationContext)
        {
            throw new NotImplementedException();
        }

        public override List<ProvenanceResult> GetItemProvenance(string itemToMatch, string itemType, EvaluationContext evaluationContext)
        {
            throw new NotImplementedException();
        }

        public override List<ProvenanceResult> GetItemProvenance(ProjectItem item, EvaluationContext evaluationContext)
        {
            throw new NotImplementedException();
        }

        public override ICollection<ProjectItem> GetItems(string itemType)
        {
            throw new NotImplementedException();
        }

        public override ICollection<ProjectItem> GetItemsByEvaluatedInclude(string evaluatedInclude)
        {
            throw new NotImplementedException();
        }

        public override ICollection<ProjectItem> GetItemsIgnoringCondition(string itemType)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<ProjectElement> GetLogicalProject()
        {
            throw new NotImplementedException();
        }

        public override ProjectProperty GetProperty(string name)
        {
            throw new NotImplementedException();
        }

        public override string GetPropertyValue(string name)
        {
            throw new NotImplementedException();
        }

        public override void MarkDirty()
        {
            throw new NotImplementedException();
        }

        public override void ReevaluateIfNecessary(EvaluationContext evaluationContext)
        {
            throw new NotImplementedException();
        }

        public override bool RemoveGlobalProperty(string name)
        {
            throw new NotImplementedException();
        }

        public override bool RemoveItem(ProjectItem item)
        {
            throw new NotImplementedException();
        }

        public override void RemoveItems(IEnumerable<ProjectItem> items)
        {
            throw new NotImplementedException();
        }

        public override bool RemoveProperty(ProjectProperty property)
        {
            throw new NotImplementedException();
        }

        public override void SaveLogicalProject(TextWriter writer)
        {
            throw new NotImplementedException();
        }

        public override bool SetGlobalProperty(string name, string escapedValue)
        {
            throw new NotImplementedException();
        }

        public override ProjectProperty SetProperty(string name, string unevaluatedValue)
        {
            throw new NotImplementedException();
        }

        public override void Unload()
        {
            throw new NotImplementedException();
        }
        #endregion
        #endregion
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

        public T Import<T, L, RMock>(RMock remoter)
            where T : class
            where L : class
            where RMock : LinkRemoterMock<T, L>, new()
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
            perRemoteCollection.GetOrCreate(remoter.LocalId, remoter, this, out proxy, slow : true);

            return proxy.Linked;
        }

        public void Export<T, L, RMock>(T obj, out RMock remoter)
            where T : class
            where L : class
            where RMock : LinkRemoterMock<T, L>, new()
        {
            if (obj == null)
            {
                remoter = null;
                return;
            }

            var external = this.LinkFactory.GetLink(obj);

            if (external != null)
            {
                var proxy = (ILinkMock<T,L>)external;

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
            pcl.Export<Project, ProjectLink, ProjectLinkRemoter>(p, out remoter);

        }
    }


}

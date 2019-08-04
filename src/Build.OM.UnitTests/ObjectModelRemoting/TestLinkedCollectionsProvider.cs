// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{
    using Microsoft.Build.Evaluation;
    using System.Text.RegularExpressions;

    public static class CompileTest
    {
        public static void T()
        {
            var group1 = ProjectCollectionLinker.CreateGroup();

            var pcLocal = group1.AddNew();
            var pcRemote = group1.AddNew();
            pcLocal.Importing = true;

            var local = pcLocal.Collection.LoadedProjects;


            Project p = null;
            // pcl.Export(p, out ProjectLinkRemoter remoter);
            pcLocal.Export(p, out MockProjectLinkRemoter remoter);
            pcLocal.Export<Project, MockProjectLinkRemoter>(p, out remoter);
        }
    }


#if false
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



    // Typical flow for "linked object" of type "Foo"
    // [ ---  Client Process                                    ]                           [ Server process (can be different) ] 
    // (Foo) localView <=> (FooLink) link <=> FooLinkRemoter (Proxy) <=~connection mechanism~=> FooLinkRemoter(stub) <=> (Real object)
    //
    // FooLinkRemoter would be whatever ExternalProviders see useful to provide FooLink implementation, it might be completely different interface.
    // (note some link types would be inconvenient /impossible to serialize for example and pass cross process).
    // For the purpose of unit tests we know Client and Server are on the same process, so we can cheat and combine the proxy/connection/stub to a single object.
    // we'll call FooRemoter. We want to make sure FooRemoter only use a simple type (generally only serializable types) to ensure the
    // actual implementation is possible.

    using TestProjectLinkRemoterProxy = TestProjectLinkRemoter;
    using TestProjectLinkRemoterStub = TestProjectLinkRemoter;

    using ProjectRootElementRemoterProxy = ProjectRootElementRemoter;
    using ProjectRootElementRemoterStub = ProjectRootElementRemoter;
    using System.Runtime.Remoting.Messaging;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Logging;

    internal interface ILinkOrigin
    {
        UInt32 HostCollectionId { get; }
    }

    internal class LinkRemoter
    {
        public UInt32 HostCollectionId { get; }
        public UInt32 LocalObjectId { get; }
    }

    internal class ProjectRootElementLinkRemoter : LinkRemoter
    {
        public static ProjectRootElementLinkRemoter Create()
        {
            return null;
        }

    }

    internal class ProjectLinkRemoter : LinkRemoter
    {
        public ProjectRootElementLinkRemoter Xml { get; }
    }

    internal class MockProjectLink : ProjectLink
    {
        public static MockProjectLink Create (ProjectCollectionLinker linker, ProjectLinkRemoter proxy)
        {
            return new MockProjectLink(linker, proxy);
        }

        public MockProjectLink(ProjectCollectionLinker linker, ProjectLinkRemoter proxy)
        {
            this.Linker = linker;
            this.Proxy = proxy;
        }

        public ProjectCollectionLinker Linker { get; }

        private ProjectLinkRemoter Proxy { get; }


        public override ProjectRootElement Xml => Linker.
    #region NotImpl
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
    }



    internal class ProjectLinkImplement : ProjectLink, ILinkOrigin
    {
        public ProjectImporter Proxy { get; }
        uint ILinkOrigin.HostCollectionId => Proxy.HostCollectionId;


        public override ProjectRootElement Xml => throw new NotImplementedException();

        public override bool IsDirty => throw new NotImplementedException();

        public override IDictionary<string, string> GlobalProperties => throw new NotImplementedException();

        public override ICollection<string> ItemTypes => throw new NotImplementedException();

        public override ICollection<ProjectProperty> Properties => throw new NotImplementedException();

        public override IDictionary<string, ProjectItemDefinition> ItemDefinitions => throw new NotImplementedException();

        public override ICollection<ProjectItem> Items => throw new NotImplementedException();

        public override IList<ResolvedImport> Imports => throw new NotImplementedException();

        public override IDictionary<string, ProjectTargetInstance> Targets => throw new NotImplementedException();

        public override ICollection<ProjectProperty> AllEvaluatedProperties => throw new NotImplementedException();

        public override ICollection<ProjectMetadata> AllEvaluatedItemDefinitionMetadata => throw new NotImplementedException();

        public override ICollection<ProjectItem> AllEvaluatedItems => throw new NotImplementedException();

        public override string ToolsVersion => throw new NotImplementedException();

        public override string SubToolsetVersion => throw new NotImplementedException();

        public override int LastEvaluationId => throw new NotImplementedException();

        public override Guid HostCollectionId => throw new NotImplementedException();

        public override string ExpandString(string unexpandedValue)
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

        public override bool RemoveProperty(ProjectProperty property)
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
    }

    interface IProjectLinkRemoting
    {
    }

    internal class ProjectExporter : ExportedLinksMap.LinkedObject<Project>, IProjectLinkRemoting
    {
        public UInt32 HostCollectionId { get; }

        public Project Object { get; }
    }

    internal class ProjectImporter : ImportedLinksMap.LinkedObject<ProjectExporter>
    {
        public ProjectExporter Stub { get; }

        public uint HostCollectionId => Stub.HostCollectionId;

        public override bool IsNull => Key == 0;

        public Project Object { get; }
    }


    internal class ProjectProxy : ImportedLinksMap.LinkedObject<Project>
    {
        public uint HostCollectionId => throw new NotImplementedException();

        public override bool IsNull => Key == 0;
    }

    internal class RemoteObject<T>
    {

    }


    class ProjectLinkProxy : ImportedLinksMap.LinkedObject<Project>
    {
    }


    class ProjectLinkStub : ExportedLinksMap.LinkedObject<Project>
    {
    }

    internal abstract class RemoteFactory<T, R, L>
        where L : class, new()
    {
        public L CreateLink(ProjectCollectionLinker linker, R remoter)
        {
            var link = new L();
            return link;
        }

        public R CreateRemoter(ProjectCollectionLinker linker, T obj)
        {
            linker.LinkFactory.
        }

    }

    class ProjectCollectionsLinker
    {


        ProjectCollectionLinker AddNew();
        public void Remove(ProjectCollectionLinker collection);
    }

    class ProjectCollectionLinker : IDisposable
    {
        public ProjectCollectionsLinker Linker { get; private set; }
        public ProjectCollection Collection { get; }

        public LinkedObjectsFactory LinkFactory { get; }

        public UInt32 CollectionId { get; }

        private ExportedLinksMap exported = ExportedLinksMap.Create();
        private ImportedLinksMap imported = ImportedLinksMap.Create();

        public void Dispose()
        {
            Linker?.Remove(this);
            this.Linker = null;
            imported.Dispose();
            exported.Dispose();
        }

        public  R Export<R, T, L> (T obj, Func<T, R> factory)
        {
            var external = this.LinkFactory.GetLink(obj);
            if (external != null)
            {
                return (L)external.GetRemoter();
            }

            exported.GetOrCreate(R, T out R);
        }

        public Project Import(ProjectExporter exporter)
        {
            if (exporter == null)
            {
                return null;
            }

            if (exporter.HostCollectionId == CollectionId)
            {
                return exporter.Object;
            }

            ProjectImporter remote;
            imported.GetOrCreate(exporter.LocalId, exporter, out remote);
            return remote.Object;
        }

        public ProjectExporter Export(Project project)
        {
            if (project == null)
            {
                return null;
            }

            
            var linked = ProjectLink.GetLink(project);
            if (linked == null)
            {
                ProjectExporter result;
                exported.GetOrCreate(/*key*/project, /*strong ref*/ project, out result);
                return result;
            }
            else
            {
                return ((ProjectLinkImplement)linked).Proxy.Stub;
            }
        }
    }





internal class ProjectRootElementRemoter
    {
    }

    internal class ProjectPropertyRemoter
    {
    }

    internal class TestProjectLinkRemoter
    {
        private TestLinkedCollectionsExporeter exporter;
        private Project project;

        public virtual ProjectRootElementRemoter Xml => this.exporter.GetOrCreateRemoter<ProjectRootElementRemoter>(project.Xml);

        public virtual bool ThrowInsteadOfSplittingItemElement => project.ThrowInsteadOfSplittingItemElement;
        public virtual bool IsDirty => project.IsDirty;
        public virtual IReadOnlyCollection<KeyValuePair<string, string>> GlobalProperties => (IReadOnlyCollection<KeyValuePair<string, string>>)this.project.GlobalProperties;
        public virtual IReadOnlyCollection<string> ItemTypes => (IReadOnlyCollection<string>)this.project.ItemTypes;

        public virtual IReadOnlyCollection<ProjectPropertyRemoter> Properties
        {
            get
            {
                var localProps = project.Properties;
                var result = new List<ProjectPropertyRemoter>(localProps.Count);
                foreach (var prop in project.Properties)
                {
                    result.Add(this.exporter.GetOrCreateRemoter<ProjectPropertyRemoter>(prop));
                }
                return result;
            }
        }

        public virtual IReadOnlyCollection<KeyValuePair<string, IReadOnlyCollection<string>>> ConditionedProperties => (IReadOnlyCollection<KeyValuePair<string, IReadOnlyCollection<string>>>)this.project.ConditionedProperties;
        public virtual IReadOnlyCollection<ProjectItemDefinitionRemoter> ItemDefinitions { get; }

        public abstract ICollection<ProjectItem> Items { get; }
        public virtual ICollection<ProjectItem> ItemsIgnoringCondition { get { throw new NotImplementedException(); } }
        public abstract IList<ResolvedImport> Imports { get; }
        public virtual IList<ResolvedImport> ImportsIncludingDuplicates { get { return Imports; } }
        public abstract IDictionary<string, ProjectTargetInstance> Targets { get; }
        public abstract ICollection<ProjectProperty> AllEvaluatedProperties { get; }
        public abstract ICollection<ProjectMetadata> AllEvaluatedItemDefinitionMetadata { get; }
        public abstract ICollection<ProjectItem> AllEvaluatedItems { get; }
        public abstract string ToolsVersion { get; }
        public abstract string SubToolsetVersion { get; }
        public virtual bool SkipEvaluation { get; set; }
        public virtual bool DisableMarkDirty { get; set; }
        public virtual bool IsBuildEnabled { get; set; } = true;
        public abstract int LastEvaluationId { get; }

        public virtual List<GlobResult> GetAllGlobs(EvaluationContext evaluationContext) { throw new NotImplementedException(); }
        public virtual List<GlobResult> GetAllGlobs(string itemType, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

        public virtual List<ProvenanceResult> GetItemProvenance(string itemToMatch, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

        public virtual List<ProvenanceResult> GetItemProvenance(string itemToMatch, string itemType, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

        public virtual List<ProvenanceResult> GetItemProvenance(ProjectItem item, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

        public virtual IEnumerable<ProjectElement> GetLogicalProject() { throw new NotImplementedException(); }

        public abstract ProjectProperty GetProperty(string name);

        public abstract string GetPropertyValue(string name);

        public abstract ProjectProperty SetProperty(string name, string unevaluatedValue);

        public abstract bool SetGlobalProperty(string name, string escapedValue);

        public virtual IList<ProjectItem> AddItem(string itemType, string unevaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata) { throw new NotImplementedException(); }

        public virtual IList<ProjectItem> AddItemFast(string itemType, string unevaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata) { return AddItem(itemType, unevaluatedInclude, metadata); }

        public abstract ICollection<ProjectItem> GetItems(string itemType);
        public virtual ICollection<ProjectItem> GetItemsIgnoringCondition(string itemType) { throw new NotImplementedException(); }

        public abstract ICollection<ProjectItem> GetItemsByEvaluatedInclude(string evaluatedInclude);

        public abstract bool RemoveProperty(ProjectProperty property);
        public abstract bool RemoveGlobalProperty(string name);

        public virtual bool RemoveItem(ProjectItem item) { throw new NotImplementedException(); }
        public virtual void RemoveItems(IEnumerable<ProjectItem> items) { throw new NotImplementedException(); }
        public abstract string ExpandString(string unexpandedValue);
        public virtual ProjectInstance CreateProjectInstance(ProjectInstanceSettings settings, EvaluationContext evaluationContext) { throw new NotImplementedException(); }
        public abstract void MarkDirty();
        public abstract void ReevaluateIfNecessary(EvaluationContext evaluationContext);
        // public virtual void SaveLogicalProject(TextWriter writer) { throw new NotImplementedException(); }
        // public virtual bool Build(string[] targets, IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers, EvaluationContext evaluationContext) { throw new NotImplementedException(); }
        public override void Unload() { } // do nothing.
    }

    internal class TestProjectLinkChannel
    {
        private TestLinkedCollectionsProvider linkProvider;
        private Project _project;

        public ProjectRootElementLink XmlLink { get; }

    }


    class LinkedObject<T, P>
    {
        public T Object;
        public P Proxy; 
    }

    class ObjectWithId
    {
        public int Id { get; set; }
        public WeakDictionary Tracker { get; set; }

        ~ObjectWithId()
        {
            Tracker?.Remove(Id);
        }
    }

    abstract class RemotedObject
    {
        public int Id { get; set; }
        public abstract object Key { get; }
    }

    class RemotedObject<T> : RemotedObject
    {
        public RemotedObject(T obj)
        {
            this.Holder = obj;
        }

        public override object Key => this.Holder;
        public T Holder;
    }


    public class ProxyFactory<T>
    {
        public static Func<int, T> Factory { get; set; } = (i) => throw new NotImplementedException();
    }



    abstract class ProxiedObject<T>
    {
        public abstract int Id { get; set; }
        public abstract T Holder { get; }
    }

    class ActiveStubs
    {
        private int nextId = 0;
        private object Lock { get; } = new object();
        Dictionary<int, RemotedObject> ActiveKeys = new Dictionary<int, RemotedObject>();
        Dictionary<object, RemotedObject> activeRemotedObjects = new Dictionary<object, RemotedObject>();


        private int Remote(object key, Func<RemotedObject> factory)
        {
            lock (this.Lock)
            {
                if (!activeRemotedObjects.TryGetValue(key, out var existing))
                {
                    existing = factory();
                    while (nextId == 0 || ActiveKeys.ContainsKey(nextId))
                    {
                        nextId++;
                    }
                    existing.Id = nextId;
                    ActiveKeys.Add(nextId, existing);
                    activeRemotedObjects.Add(existing.Key, existing);
                }

                return existing.Id;
            }
        }


        private object Get(int id)
        {
            lock (this.Lock)
            {
                if (ActiveKeys.TryGetValue(id, out var ret))
                {
                    return ret.Key;
                }

                return null;
            }
        }


        public void Release(IEnumerable<int> ids)
        {
            lock (this.Lock)
            {
                foreach (var id in ids)
                {
                    if (ActiveKeys.TryGetValue(id, out var existing))
                    {
                        ActiveKeys.Remove(id);
                        activeRemotedObjects.Remove(existing.Key);
                    }
                }
            }
        }

        public T Get<T>(int id) where T : class
        {
            return Get(id) as T;
        }

        public int Add<T>(T obj) where T : class
        {
            return Remote(obj, () => new RemotedObject<T>(obj));
        }
    }


    class ActiveProxies
    {
        static ActiveProxies()
        {
            ProxyFactory<Project>.Factory = 
        }

        private object Lock { get; } = new object();
        Dictionary<int, object> proxies = new Dictionary<int, object>();


        private int Remote(object key, Func<RemotedObject> factory)
        {
            lock (this.Lock)
            {
                if (!activeRemotedObjects.TryGetValue(key, out var existing))
                {
                    existing = factory();
                    while (nextId == 0 || ActiveKeys.ContainsKey(nextId))
                    {
                        nextId++;
                    }
                    existing.Id = nextId;
                    ActiveKeys.Add(nextId, existing);
                    activeRemotedObjects.Add(existing.Key, existing);
                }

                return existing.Id;
            }
        }


        private object Get(int id)
        {
            lock (this.Lock)
            {
                if (proxies.TryGetValue(id, out var ret))
                {
                    return ret;
                }

                return null;
            }
        }


        public void Release(IEnumerable<int> ids)
        {
            lock (this.Lock)
            {
                foreach (var id in ids)
                {
                    if (ActiveKeys.TryGetValue(id, out var existing))
                    {
                        ActiveKeys.Remove(id);
                        activeRemotedObjects.Remove(existing.Key);
                    }
                }
            }
        }

        public T Get<T>(int id, Func<int, T> factory) where T : class
        {
            if (id == 0) return null;
            var retO = Get(id);
            if (retO == null)
            {
                var proxied = factory(id);
                ... ADD...
            }

            return (T)retO;
        }
    }

    internal abstract class LinkedObjects
    {
        public abstract object LocalKey { get; }
        public abstract object LinkKey { get; }
    }

    internal class LinkedObjects<T, L> : LinkedObjects
    {
        public override object LocalKey => this.LocalObject;

        public override object LinkKey => this.RemoteLink;

        public T LocalObject { get; }
        public L RemoteLink { get; }
    }
     


    /// <summary>
    /// Maintain a "active" linked object collection.
    /// </summary>
    class LinkedObjectCollection
    {
        Dictionary<object, WeakReference<LinkedObjects>> byLink;
    }



    class WeakDictionary
    {
        private int nextId = 0;
        private object Lock { get; } = new object();

        Dictionary<int, WeakReference<ObjectWithId>> possiblyActive;

        public void Remove(int id)
        {
            TryGet(id, out var ignored);
        }


        public bool Add(ObjectWithId item)
        {
            if (item.Tracker != null) return false;
            item.Tracker = this;
            lock (Lock)
            {
                while (nextId == 0 || possiblyActive.ContainsKey(nextId))
                {
                    nextId++;
                }

                item.Id = nextId;
                possiblyActive.Add(nextId, new WeakReference<ObjectWithId>(item));
                return true;
            }
        }

        public bool TryGet(int i, out ObjectWithId obj)
        {
            lock (Lock)
            {
                if (!possiblyActive.TryGetValue(i, out var ret))
                {
                    obj = null;
                    return false;
                }

                if (!ret.TryGetTarget(out obj))
                {
                    possiblyActive.Remove(i);
                    return false;
                }
            }

            return true;
        }
    }


    internal class Remoter<T>
    {
        public void Init( T remoted)
        {
            Object = remoted;
        }

        public T Object { get; private set; }

        ~Remoter()
        {
            TestLinkedCollectionsExporeter.Instance.ReleaseRemoter(Object);
        }
    }


    internal class TestLinkedCollectionsExporeter
    {
        ActiveStubs remotedObjects = new ActiveStubs();

        public static TestLinkedCollectionsExporeter Instance { get; } = new TestLinkedCollectionsExporeter(); 

        public ProjectCollection LocalCollection { get; } = new ProjectCollection();

        public void ReleaseRemoter(object key)
        {
        }


        public R GetOrCreateRemoter<R, T>(T obj) where R : Remoter<T>, new()
        {
            if (obj == null) return null;
            var ret = new R();
            ret.Init(obj);
            return ret;
        }


        public IReadOnlyCollection<int> GetLoadedProjects(string filePath)
        {
            List<int> links = new List<int>();
            var toRemote = filePath == null ? LocalCollection.LoadedProjects : LocalCollection.GetLoadedProjects(filePath);

            foreach (var remoteProject in toRemote)
            {
                links.Add(remotedObjects.Add(remoteProject));
            }

            return links;
       }

        public void Collect(IReadOnlyCollection<int> unusedObjects)
        {
            remotedObjects.Release(unusedObjects);
        }
    }


    internal class TestLinkedCollectionsImporter : ExternalProjectsProvider
    {
        ActiveProxies proxies = new ActiveProxies();


        public ProjectCollection LocalCollection { get; }
        public TestLinkedCollectionsExporeter RemoteCollection { get; }

        public TestLinkedCollectionsImporter()
        {
            this.LocalCollection = new ProjectCollection();
        }


        private ProjectRootElement CreateProjectRootElement(int id)
        {
        }

        private Project CreateProject(ProjectLink link)
        {
            ExternalProjectsProvider.CreateLinkedProject(this.LocalCollection, link);
        }

        public override ICollection<Project> GetLoadedProjects(string filePath)
        {
            List<Project> links = new List<Project>();
            var projectIds = RemoteCollection.GetLoadedProjects(filePath);

            foreach (var id in projectIds)
            {
                links.Add(proxies.Get<Project>(id, CreateProject));
            }

            return links;
        }


        private ProjectRootElement GetProjectXml(int projectId, ref ProjectRootElement cache)
        {
            if (cache == null)
            {
                var xmlId = RemoteCollection.GetProjectXml(projectId);
                var xml = proxies.Get<ProjectRootElement>(xmlId, CreateProjectRootElement);
                cache = xml;
            }

            return cache;
        }


        internal class TestRemoteCollectionService
        {
            public TestLinkedCollectionsImporter importer { get; }

            public ProjectRootElement GetProjectXml(int projId);

            public bool GetThrowInsteadOfSplittingItemElement(int projId);

            public bool GetIsDirty(int projId)

            public IReadOnlyCollection<KeyValuePair<string, string>> GetGlobalProperties(int projId);

            public IReadOnlyCollection<string> GetItemTypes(int projId)
            public IReadOnlyCollection<ProjectProperty> GetProperties(int projId);

            public IReadOnlyCollection<KeyValuePair<string, IReadOnlyCollection<string>>> GetConditionedProperties(int projId);

            public IReadOnlyCollection<ProjectItemDefinition> GetItemDefinitions(int projId);
            public IReadOnlyCollection<ProjectItem> GetItems(int projId);
            public IReadOnlyCollection<ProjectItem> GetItemsIgnoringCondition(int projId);
            public IReadOnlyCollection<ResolvedImport> GetImports(int projId);

            public IReadOnlyCollection<ResolvedImport> GetImportsIncludingDuplicates(int projId);

            public IReadOnlyCollection<ProjectTargetInstance> GetTargets(int projId);

            public IReadOnlyCollection<ProjectProperty> GetAllEvaluatedProperties(int projId);
            public IReadOnlyCollection<ProjectMetadata> GetAllEvaluatedItemDefinitionMetadata(int projId);
            public IReadOnlyCollection<ProjectItem> GetAllEvaluatedItems(int projId);

            public string GetToolsVersion(int projId);
            public string GetSubToolsetVersion(int projId);

            public virtual bool SkipEvaluation(int projId);
            public virtual bool DisableMarkDirty(int projId);
            public virtual bool IsBuildEnabled(int projId);
            public virtual int LastEvaluationId(int projId);

            /*
            public virtual List<GlobResult> GetAllGlobs(EvaluationContext evaluationContext) { throw new NotImplementedException(); }
            public virtual List<GlobResult> GetAllGlobs(string itemType, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

            public virtual List<ProvenanceResult> GetItemProvenance(string itemToMatch, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

            public virtual List<ProvenanceResult> GetItemProvenance(string itemToMatch, string itemType, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

            public virtual List<ProvenanceResult> GetItemProvenance(ProjectItem item, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

            public virtual IEnumerable<ProjectElement> GetLogicalProject() { throw new NotImplementedException(); }
            */


            public ProjectProperty GetProperty(int projId, string name);

            public string GetPropertyValue(int projId, string name);

            public ProjectProperty SetProperty(int projId, string name, string unevaluatedValue);

            public bool SetGlobalProperty(int projId, string name, string escapedValue);

            public IReadOnlyCollection<ProjectItem> AddItem(int projId, string itemType, string unevaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata)
            public IReadOnlyCollection<ProjectItem> AddItemFast(int projId, string itemType, string unevaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata)

            public IReadOnlyCollection<ProjectItem> GetItems(int projId, string itemType);
            public IReadOnlyCollection<ProjectItem> GetItemsIgnoringCondition(int projId, string itemType) { throw new NotImplementedException(); }

            public IReadOnlyCollection<ProjectItem> GetItemsByEvaluatedInclude(int projId, string evaluatedInclude);

            public bool RemoveProperty(int projId, ProjectProperty property);
            public bool RemoveGlobalProperty(int projId, string name);

            public bool RemoveItem(int projId, ProjectItem item);
            public void RemoveItems(int projId, IEnumerable<ProjectItem> items);
            public string ExpandString(int projId, string unexpandedValue);

            // public virtual ProjectInstance CreateProjectInstance(ProjectInstanceSettings settings, EvaluationContext evaluationContext) { throw new NotImplementedException(); }
            public void MarkDirty(int projId);

            public void ReevaluateIfNecessary(int projId, EvaluationContext evaluationContext);

            // public virtual void SaveLogicalProject(TextWriter writer) { throw new NotImplementedException(); }
            // public virtual bool Build(string[] targets, IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

            public override void Unload() { } // do nothing.
        }



        internal class TestProjectLink : ProjectLink
        {
            public TestProjectLink(TestProjectLinkRemoterProxy x)
            {
                this.Proxy = x;
            }

            public TestLinkedCollectionsImporter importer { get; }
            public TestProjectLinkRemoterProxy Proxy { get; }


            public int ProjectId { get; }

            private ProjectRootElement xmlCached;
            public override ProjectRootElement Xml => this.importer.GetProjectXml(this.ProjectId, ref this.xmlCached);


            public virtual bool ThrowInsteadOfSplittingItemElement { get; set; }
            public abstract bool IsDirty { get; }
            public abstract IDictionary<string, string> GlobalProperties { get; }
            public abstract ICollection<string> ItemTypes { get; }
            public abstract ICollection<ProjectProperty> Properties { get; }
            public virtual IDictionary<string, List<string>> ConditionedProperties { get { throw new NotImplementedException(); } }
            public abstract IDictionary<string, ProjectItemDefinition> ItemDefinitions { get; }
            public abstract ICollection<ProjectItem> Items { get; }
            public virtual ICollection<ProjectItem> ItemsIgnoringCondition { get { throw new NotImplementedException(); } }
            public abstract IList<ResolvedImport> Imports { get; }
            public virtual IList<ResolvedImport> ImportsIncludingDuplicates { get { return Imports; } }
            public abstract IDictionary<string, ProjectTargetInstance> Targets { get; }
            public abstract ICollection<ProjectProperty> AllEvaluatedProperties { get; }
            public abstract ICollection<ProjectMetadata> AllEvaluatedItemDefinitionMetadata { get; }
            public abstract ICollection<ProjectItem> AllEvaluatedItems { get; }
            public override string ToolsVersion => this.Proxy.ToolsVersion;
            public override string SubToolsetVersion => this.Proxy.SubToolsetVersion;
            public override bool SkipEvaluation
            {
                get => this.Proxy.SkipEvaluation;
                set => this.Proxy.SkipEvaluation = value;
            }

            public override bool DisableMarkDirty
            {
                get => this.Proxy.DisableMarkDirty;
                set => this.Proxy.DisableMarkDirty = value;
            }

            // public virtual bool IsBuildEnabled { get; set; } = true;
            public override int LastEvaluationId => this.Proxy.LastEvaluationId;

            public virtual List<GlobResult> GetAllGlobs(EvaluationContext evaluationContext) { throw new NotImplementedException(); }
            public virtual List<GlobResult> GetAllGlobs(string itemType, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

            public virtual List<ProvenanceResult> GetItemProvenance(string itemToMatch, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

            public virtual List<ProvenanceResult> GetItemProvenance(string itemToMatch, string itemType, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

            public virtual List<ProvenanceResult> GetItemProvenance(ProjectItem item, EvaluationContext evaluationContext) { throw new NotImplementedException(); }

            public virtual IEnumerable<ProjectElement> GetLogicalProject() { throw new NotImplementedException(); }

            public override ProjectProperty GetProperty(string name)
            {
                return this.Proxy.GetProperty(name);
            }

            public abstract string GetPropertyValue(string name);

            public abstract ProjectProperty SetProperty(string name, string unevaluatedValue);

            public abstract bool SetGlobalProperty(string name, string escapedValue);

            public virtual IList<ProjectItem> AddItem(string itemType, string unevaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata) { throw new NotImplementedException(); }

            public virtual IList<ProjectItem> AddItemFast(string itemType, string unevaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata) { return AddItem(itemType, unevaluatedInclude, metadata); }

            public abstract ICollection<ProjectItem> GetItems(string itemType);
            public virtual ICollection<ProjectItem> GetItemsIgnoringCondition(string itemType) { throw new NotImplementedException(); }

            public abstract ICollection<ProjectItem> GetItemsByEvaluatedInclude(string evaluatedInclude);

            public abstract bool RemoveProperty(ProjectProperty property);
            public abstract bool RemoveGlobalProperty(string name);

            public virtual bool RemoveItem(ProjectItem item) { throw new NotImplementedException(); }
            public virtual void RemoveItems(IEnumerable<ProjectItem> items) { throw new NotImplementedException(); }
            public abstract string ExpandString(string unexpandedValue);
            public virtual ProjectInstance CreateProjectInstance(ProjectInstanceSettings settings, EvaluationContext evaluationContext) { throw new NotImplementedException(); }
            public abstract void MarkDirty();
            public abstract void ReevaluateIfNecessary(EvaluationContext evaluationContext);
            // public virtual void SaveLogicalProject(TextWriter writer) { throw new NotImplementedException(); }
            // public virtual bool Build(string[] targets, IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers, EvaluationContext evaluationContext) { throw new NotImplementedException(); }
            public override void Unload() { } // do nothing.
        }
    }


#endif

}

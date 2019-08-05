// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Evaluation.Context;
    using Microsoft.Build.Execution;
    using Microsoft.Build.ObjectModelRemoting;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Logging;

    internal class MockProjectLinkRemoter : MockLinkRemoter<Project>
    {
        public MockProjectLinkRemoter()
        {
        }

        public override Project CreateLinkedObject(ProjectCollectionLinker remote)
        {
            var link = new MockProjectLink(this);
            return remote.LinkFactory.Create(link);
        }
    }

    internal class MockProjectLink : ProjectLink, ILinkMock
    {
        public MockProjectLink(MockProjectLinkRemoter proxy)
        {
            this.Proxy = proxy;
        }

        public MockProjectLinkRemoter Proxy { get; }
        object ILinkMock.Remoter => this.Proxy;

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
}

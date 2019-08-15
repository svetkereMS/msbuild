// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Build.Construction;
    using Microsoft.Build.ObjectModelRemoting;
    using Microsoft.Build.Evaluation;
    using Xunit;
    using System.Runtime.ExceptionServices;
    using System.Xml.Schema;
    using System.Collections;
    using Microsoft.Build.Framework;

    internal enum ProjectType
    {
        Real = 1,
        View = 2
    }

    internal class ProjectPair
    {
        public ProjectPair(Project view, Project real)
        {
            ViewValidation.VerifyLinkedNotNull(view);
            ViewValidation.VerifyNotLinkedNotNull(real);
            this.View = view;
            this.Real = real;
        }

        public Project GetProject(ProjectType type) => type == ProjectType.Real ? this.Real : this.View;
        public Project View { get; }
        public Project Real { get; }

        public void ValidatePropertyValue(string name, string value)
        {
            Assert.Equal(value, this.View.GetPropertyValue(name));
            Assert.Equal(value, this.Real.GetPropertyValue(name));
        }

        private ProjectItem VerifyAfterAddSingleItem(ProjectType where, ICollection<ProjectItem> added, IEnumerable<KeyValuePair<string, string>> metadata)
        {
            Assert.NotNull(added);
            Assert.Equal(1, added.Count);
            var result = added.First();
            Assert.NotNull(result);

            // validate there is exactly 1 item with this include in both view and real and it is the exact same object.
            Assert.Same(result, this.GetSingleItemWithVerify(where, result.EvaluatedInclude));

            if (metadata != null)
            {
                foreach (var m in metadata)
                {
                    Assert.True(result.HasMetadata(m.Key));
                    var md = result.GetMetadata(m.Key);
                    Assert.NotNull(md);
                    Assert.Equal(m.Value, md.UnevaluatedValue);
                }
            }

            return result;
        }

        public ProjectItem AddSingleItemWithVerify(ProjectType where, string itemType, string unevaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata = null)
        {
            var toAdd = this.GetProject(where);
            var added = (metadata == null) ? toAdd.AddItem(itemType, unevaluatedInclude) : toAdd.AddItem(itemType, unevaluatedInclude, metadata);
            return VerifyAfterAddSingleItem(where, added, metadata);
        }

        public ProjectItem AddSingleItemFastWithVerify(ProjectType where, string itemType, string unevaluatedInclude, IEnumerable<KeyValuePair<string, string>> metadata = null)
        {
            var toAdd = this.GetProject(where);
            var added = (metadata == null) ? toAdd.AddItemFast(itemType, unevaluatedInclude) : toAdd.AddItemFast(itemType, unevaluatedInclude, metadata);
            return VerifyAfterAddSingleItem(where, added, metadata);
        }

        public ProjectItem GetSingleItemWithVerify(ProjectType which, string evaluatedInclude)
        {
            var realItems = this.Real.GetItemsByEvaluatedInclude(evaluatedInclude);
            var viewItems = this.View.GetItemsByEvaluatedInclude(evaluatedInclude);

            ViewValidation.Verify(viewItems, realItems, ViewValidation.Verify, this);
            if (viewItems == null || viewItems.Count == 0) return null;
            Assert.Equal(1, viewItems.Count);
            return which == ProjectType.View ? viewItems.First() : realItems.First();
        }
    }

    internal static partial class ViewValidation
    {
        public static void Verify(ProjectProperty view, ProjectProperty real, ProjectPair pair = null)
        {
            if (view == null && real == null) return;
            VerifyLinkedNotNull(view);
            VerifyNotLinkedNotNull(real);

            Assert.Equal(real.Name, view.Name);
            Assert.Equal(real.EvaluatedValue, view.EvaluatedValue);
            Assert.Equal(real.UnevaluatedValue, view.UnevaluatedValue);
            Assert.Equal(real.IsEnvironmentProperty, view.IsEnvironmentProperty);
            Assert.Equal(real.IsGlobalProperty, view.IsGlobalProperty);
            Assert.Equal(real.IsReservedProperty, view.IsReservedProperty);
            Assert.Equal(real.IsImported, view.IsImported);

            Verify(view.Xml, real.Xml);

            VerifyLinkedNotNull(view.Project);
            VerifyNotLinkedNotNull(real.Project);
            if (pair != null)
            {
                Assert.Same(pair.View, view.Project);
                Assert.Same(pair.Real, real.Project);
            }

            Verify(view.Predecessor, real.Predecessor, pair);
        }

        public static void Verify(ProjectMetadata view, ProjectMetadata real, ProjectPair pair = null)
        {
            if (view == null && real == null) return;
            VerifyLinkedNotNull(view);
            VerifyNotLinkedNotNull(real);

            Assert.Equal(real.Name, view.Name);
            Assert.Equal(real.EvaluatedValue, view.EvaluatedValue);
            Assert.Equal(real.UnevaluatedValue, view.UnevaluatedValue);
            Assert.Equal(real.ItemType, view.ItemType);
            Assert.Equal(real.IsImported, view.IsImported);

            VerifySameLocation(real.Location, view.Location);
            VerifySameLocation(real.ConditionLocation, view.ConditionLocation);

            Verify(view.Xml, real.Xml);

            VerifyLinkedNotNull(view.Project);
            VerifyNotLinkedNotNull(real.Project);
            if (pair != null)
            {
                Assert.Same(pair.View, view.Project);
                Assert.Same(pair.Real, real.Project);
            }

            Verify(view.Predecessor, real.Predecessor, pair);
        }

        public static void Verify(ProjectItemDefinition view, ProjectItemDefinition real, ProjectPair pair = null)
        {
            if (view == null && real == null) return;
            VerifyLinkedNotNull(view);
            VerifyNotLinkedNotNull(real);

            // note ItemDefinition does not have a XML element
            // this is since it is [or can be] a aggregation of multiple ProjectItemDefinitionElement's.
            // This is somewhat of deficiency of MSBuild API.
            // (for example SetMetadata will always create a new ItemDefinitionElement because of that, for new metadata).

            Assert.Equal(real.ItemType, view.ItemType);
            Assert.Equal(real.MetadataCount, view.MetadataCount);

            Verify(view.Metadata, real.Metadata, Verify, pair);

            foreach (var rm in real.Metadata)
            {
                var rv = real.GetMetadataValue(rm.Name);
                var vv = view.GetMetadataValue(rm.Name);
                Assert.Equal(rv, vv);

                var grm = real.GetMetadata(rm.Name);
                var gvm = view.GetMetadata(rm.Name);

                Verify(gvm, grm, pair);
            }

            VerifyLinkedNotNull(view.Project);
            VerifyNotLinkedNotNull(real.Project);
            if (pair != null)
            {
                Assert.Same(pair.View, view.Project);
                Assert.Same(pair.Real, real.Project);
            }
        }

        public static void Verify(ProjectItem view, ProjectItem real, ProjectPair pair = null)
        {
            if (view == null && real == null) return;
            VerifyLinkedNotNull(view);
            VerifyNotLinkedNotNull(real);

            Verify(view.Xml, real.Xml);

            Assert.Equal(real.ItemType, view.ItemType);
            Assert.Equal(real.UnevaluatedInclude, view.UnevaluatedInclude);
            Assert.Equal(real.EvaluatedInclude, view.EvaluatedInclude);
            Assert.Equal(real.IsImported, view.IsImported);

            Assert.Equal(real.DirectMetadataCount, view.DirectMetadataCount);
            Verify(view.DirectMetadata, real.DirectMetadata, Verify, pair);

            Assert.Equal(real.MetadataCount, view.MetadataCount);
            Verify(view.Metadata, real.Metadata, Verify, pair);

            foreach (var rm in real.Metadata)
            {
                var rv = real.GetMetadataValue(rm.Name);
                var vv = view.GetMetadataValue(rm.Name);
                Assert.Equal(rv, vv);

                var grm = real.GetMetadata(rm.Name);
                var gvm = view.GetMetadata(rm.Name);

                Verify(gvm, grm, pair);

                Assert.Equal(real.HasMetadata(rm.Name), view.HasMetadata(rm.Name));
            }

            Assert.Equal(real.HasMetadata("random non existent"), view.HasMetadata("random non existent"));
            Assert.Equal(real.GetMetadataValue("random non existent"), view.GetMetadataValue("random non existent"));

            VerifyLinkedNotNull(view.Project);
            VerifyNotLinkedNotNull(real.Project);
            if (pair != null)
            {
                Assert.Same(pair.View, view.Project);
                Assert.Same(pair.Real, real.Project);
            }
        }


        private static void Verify(SdkReference view, SdkReference real, ProjectPair pair = null)
        {
            if (view == null && real == null) return;
            Assert.NotNull(view);
            Assert.NotNull(real);
            
            Assert.Equal(real.Name, view.Name);
            Assert.Equal(real.Version, view.Version);
            Assert.Equal(real.MinimumVersion, view.MinimumVersion);

        }

        private static void Verify(SdkResult view, SdkResult real, ProjectPair pair = null)
        {
            if (view == null && real == null) return;
            Assert.NotNull(view);
            Assert.NotNull(real);
            Assert.Equal(real.Success, view.Success);
            Assert.Equal(real.Path, view.Path);
            Verify(view.SdkReference, real.SdkReference, pair);
        }

        private static void Verify(ResolvedImport view, ResolvedImport real, ProjectPair pair = null)
        {
            Assert.Equal(real.IsImported, view.IsImported);
            Verify(view.ImportingElement, real.ImportingElement);
            Verify(view.ImportedProject, real.ImportedProject);
            Verify(view.SdkResult, real.SdkResult, pair);
        }

        private static void Verify(List<string> viewProps, List<string> realProps, ProjectPair pair = null)
        {
            if (viewProps == null && realProps == null) return;
            Assert.NotNull(viewProps);
            Assert.NotNull(realProps);
            Assert.Equal(realProps.Count, viewProps.Count);

            for (int i = 0; i< realProps.Count; i++)
            {
                Assert.Equal(realProps[i], viewProps[i]);
            }
        }

        public static void Verify(Project view, Project real)
        {
            if (view == null && real == null) return;
            var pair = new ProjectPair(view, real);
            Verify(pair);
        }


        public static void Verify(ProjectPair pair)
        {
            if (pair == null) return;
            var real = pair.Real;
            var view = pair.View;

            Verify(view.Xml, real.Xml);

            Verify(view.ItemsIgnoringCondition, real.ItemsIgnoringCondition, Verify, pair);
            Verify(view.Items, real.Items, Verify, pair);
            Verify(view.ItemDefinitions, real.ItemDefinitions, Verify, pair);
            Verify(view.ConditionedProperties, real.ConditionedProperties, Verify, pair);
            Verify(view.Properties, real.Properties, Verify, pair);
            Verify(view.GlobalProperties, real.GlobalProperties, (a, b, p) => Assert.Equal(b, a), pair);
            Verify(view.Imports, real.Imports, Verify, pair);
            Verify(view.ItemTypes, real.ItemTypes, (a, b, p) => Assert.Equal(b, a), pair);

            // this can only be used if project is loaded with ProjectLoadSettings.RecordDuplicateButNotCircularImports
            // or it throws otherwise. Slightly odd and inconvenient API design, but thats how it is.
            bool isImportsIncludingDuplicatesAvailable = false;
            try
            {
                var testLoadSettings = real.ImportsIncludingDuplicates;
                isImportsIncludingDuplicatesAvailable = true;
            }
            catch { }

            if (isImportsIncludingDuplicatesAvailable)
            {
                Verify(view.ImportsIncludingDuplicates, real.ImportsIncludingDuplicates, Verify, pair);
            }

            
            Verify(view.AllEvaluatedProperties, real.AllEvaluatedProperties, Verify, pair);
            Verify(view.AllEvaluatedItemDefinitionMetadata, real.AllEvaluatedItemDefinitionMetadata, Verify, pair);
            Verify(view.AllEvaluatedItems, real.AllEvaluatedItems, Verify, pair);

            Assert.NotSame(view.ProjectCollection, real.ProjectCollection);
            Assert.Equal(real.ToolsVersion, view.ToolsVersion);
            Assert.Equal(real.SubToolsetVersion, view.SubToolsetVersion);
            Assert.Equal(real.DirectoryPath, view.DirectoryPath);
            Assert.Equal(real.FullPath, view.FullPath);
            Assert.Equal(real.SkipEvaluation, view.SkipEvaluation);
            Assert.Equal(real.ThrowInsteadOfSplittingItemElement, view.ThrowInsteadOfSplittingItemElement);
            Assert.Equal(real.IsDirty, view.IsDirty);
            Assert.Equal(real.DisableMarkDirty, view.DisableMarkDirty);
            Assert.Equal(real.IsBuildEnabled, view.IsBuildEnabled);

            VerifySameLocation(real.ProjectFileLocation, view.ProjectFileLocation);

            // we currently dont support "Execution" remoting.
            // Verify(view.Targets, real.Targets, Verify, view, real);
            Assert.Equal(real.EvaluationCounter, view.EvaluationCounter);
            Assert.Equal(real.LastEvaluationId, view.LastEvaluationId);
        }

        public static void Verify<T>(IDictionary<string, T> viewCollection, IDictionary<string, T> realCollection, Action<T, T, ProjectPair> validator, ProjectPair pair = null)
        {
            if (viewCollection == null && realCollection == null) return;
            Assert.NotNull(viewCollection);
            Assert.NotNull(realCollection);

            Assert.Equal(realCollection.Count, viewCollection.Count);
            foreach (var k in realCollection.Keys)
            {
                Assert.True(viewCollection.TryGetValue(k, out var vv));
                Assert.True(realCollection.TryGetValue(k, out var rv));
                validator(vv, rv, pair);
            }
        }

        public static void Verify<T>(IEnumerable<T> viewCollection, IEnumerable<T> realCollection, Action<T, T, ProjectPair> validator, ProjectPair pair = null)
        {
            if (viewCollection == null && realCollection == null) return;
            Assert.NotNull(viewCollection);
            Assert.NotNull(realCollection);

            var viewXmlList = viewCollection.ToList();
            var realXmlList = realCollection.ToList();
            Assert.Equal(realXmlList.Count, viewXmlList.Count);
            for (int i = 0; i < realXmlList.Count; i++)
            {
                validator(viewXmlList[i], realXmlList[i], pair);
            }
        }
    }
}

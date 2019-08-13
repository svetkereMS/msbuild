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

    internal static partial class ViewValidation
    {
        public static void Verify(ProjectProperty view, ProjectProperty real, Project viewProject = null, Project realProject = null)
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
            if (viewProject != null)
            {
                Assert.Same(viewProject, view.Project);
                Assert.Same(realProject, real.Project);
            }

            Verify(view.Predecessor, real.Predecessor, viewProject, realProject);
        }

        public static void Verify(ProjectMetadata view, ProjectMetadata real, Project viewProject = null, Project realProject = null)
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
            if (viewProject != null)
            {
                Assert.Same(viewProject, view.Project);
                Assert.Same(realProject, real.Project);
            }

            Verify(view.Predecessor, real.Predecessor, viewProject, realProject);
        }

        public static void Verify(ProjectItemDefinition view, ProjectItemDefinition real, Project viewProject = null, Project realProject = null)
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

            Verify(view.Metadata, real.Metadata, Verify, viewProject, realProject);

            foreach (var rm in real.Metadata)
            {
                var rv = real.GetMetadataValue(rm.Name);
                var vv = view.GetMetadataValue(rm.Name);
                Assert.Equal(rv, vv);

                var grm = real.GetMetadata(rm.Name);
                var gvm = view.GetMetadata(rm.Name);

                Verify(gvm, grm, viewProject, realProject);
            }

            VerifyLinkedNotNull(view.Project);
            VerifyNotLinkedNotNull(real.Project);
            if (viewProject != null)
            {
                Assert.Same(viewProject, view.Project);
                Assert.Same(realProject, real.Project);
            }
        }

        public static void Verify(ProjectItem view, ProjectItem real, Project viewProject = null, Project realProject = null)
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
            Verify(view.DirectMetadata, real.DirectMetadata, Verify, viewProject, realProject);

            Assert.Equal(real.MetadataCount, view.MetadataCount);
            Verify(view.Metadata, real.Metadata, Verify, viewProject, realProject);

            foreach (var rm in real.Metadata)
            {
                var rv = real.GetMetadataValue(rm.Name);
                var vv = view.GetMetadataValue(rm.Name);
                Assert.Equal(rv, vv);

                var grm = real.GetMetadata(rm.Name);
                var gvm = view.GetMetadata(rm.Name);

                Verify(gvm, grm, viewProject, realProject);

                Assert.Equal(real.HasMetadata(rm.Name), view.HasMetadata(rm.Name));
            }

            Assert.Equal(real.HasMetadata("random non existent"), view.HasMetadata("random non existent"));
            Assert.Equal(real.GetMetadataValue("random non existent"), view.GetMetadataValue("random non existent"));

            VerifyLinkedNotNull(view.Project);
            VerifyNotLinkedNotNull(real.Project);
            if (viewProject != null)
            {
                Assert.Same(viewProject, view.Project);
                Assert.Same(realProject, real.Project);
            }
        }


        private static void Verify(SdkReference view, SdkReference real, Project viewProject = null, Project realProject = null)
        {
            if (view == null && real == null) return;
            Assert.NotNull(view);
            Assert.NotNull(real);
            
            Assert.Equal(real.Name, view.Name);
            Assert.Equal(real.Version, view.Version);
            Assert.Equal(real.MinimumVersion, view.MinimumVersion);

        }

        private static void Verify(SdkResult view, SdkResult real, Project viewProject = null, Project realProject = null)
        {
            if (view == null && real == null) return;
            Assert.NotNull(view);
            Assert.NotNull(real);
            Assert.Equal(real.Success, view.Success);
            Assert.Equal(real.Path, view.Path);
            Verify(view.SdkReference, real.SdkReference, viewProject, realProject);
        }

        private static void Verify(ResolvedImport view, ResolvedImport real, Project viewProject = null, Project realProject = null)
        {
            Assert.Equal(real.IsImported, view.IsImported);
            Verify(view.ImportingElement, real.ImportingElement);
            Verify(view.ImportedProject, real.ImportedProject);
            Verify(view.SdkResult, real.SdkResult, viewProject, realProject);
        }

        private static void Verify(List<string> viewProps, List<string> realProps, Project viewProject = null, Project realProject = null)
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
            VerifyLinkedNotNull(view);
            VerifyNotLinkedNotNull(real);

            Verify(view.Xml, real.Xml);

            Verify(view.ItemsIgnoringCondition, real.ItemsIgnoringCondition, Verify, view, real);
            Verify(view.Items, real.Items, Verify, view, real);
            Verify(view.ItemDefinitions, real.ItemDefinitions, Verify, view, real);
            Verify(view.ConditionedProperties, real.ConditionedProperties, Verify, view, real);
            Verify(view.Properties, real.Properties, Verify, view, real);
            Verify(view.GlobalProperties, real.GlobalProperties, (a, b, v, r) => Assert.Equal(b, a), view, real);
            Verify(view.Imports, real.Imports, Verify, view, real);
            Verify(view.ItemTypes, real.ItemTypes, (a, b, v, r) => Assert.Equal(b, a), view, real);

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
                Verify(view.ImportsIncludingDuplicates, real.ImportsIncludingDuplicates, Verify, view, real);
            }

            
            Verify(view.AllEvaluatedProperties, real.AllEvaluatedProperties, Verify, view, real);
            Verify(view.AllEvaluatedItemDefinitionMetadata, real.AllEvaluatedItemDefinitionMetadata, Verify, view, real);
            Verify(view.AllEvaluatedItems, real.AllEvaluatedItems, Verify, view, real);

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

        public static void Verify<T>(IDictionary<string, T> viewCollection, IDictionary<string, T> realCollection, Action<T, T, Project, Project> validator, Project viewProject = null, Project realProject = null)
        {
            if (viewCollection == null && realCollection == null) return;
            Assert.NotNull(viewCollection);
            Assert.NotNull(realCollection);

            Assert.Equal(realCollection.Count, viewCollection.Count);
            foreach (var k in realCollection.Keys)
            {
                Assert.True(viewCollection.TryGetValue(k, out var vv));
                Assert.True(realCollection.TryGetValue(k, out var rv));
                validator(vv, rv, viewProject, realProject);
            }
        }

        public static void Verify<T>(IEnumerable<T> viewCollection, IEnumerable<T> realCollection, Action<T, T, Project, Project> validator, Project viewProject = null, Project realProject = null)
        {
            if (viewCollection == null && realCollection == null) return;
            Assert.NotNull(viewCollection);
            Assert.NotNull(realCollection);

            var viewXmlList = viewCollection.ToList();
            var realXmlList = realCollection.ToList();
            Assert.Equal(realXmlList.Count, viewXmlList.Count);
            for (int i = 0; i < realXmlList.Count; i++)
            {
                validator(viewXmlList[i], realXmlList[i], viewProject, realProject);
            }
        }
    }
}

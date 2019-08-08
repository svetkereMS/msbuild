// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Sdk;



    public class LinkedConstructionReadOnly_Tests : IClassFixture<LinkedConstructionReadOnly_Tests.ROTestCollectionGroup>
    {
        public class ROTestCollectionGroup : TestCollectionGroup
        {
            public string BigFile { get; }
            public ProjectRootElement RealXml { get; }
            public ProjectRootElement ViewXml { get; private set; }

            public ROTestCollectionGroup()
                : base(1, 0)
            {
                this.BigFile = this.ImmutableDisk.WriteProjectFile($"Big.proj", TestCollectionGroup.BigProjectFile);
                var projReal = this.Remote[0].LoadProjectIgnoreMissingImports(this.BigFile);
                this.Local.Importing = true;
                Assert.NotNull(projReal);
                this.RealXml = projReal.Xml;
                Assert.NotNull(this.RealXml);
                var projView = this.Local.GetLoadedProjects(this.BigFile).FirstOrDefault();
                Assert.NotNull(projView);
                this.ViewXml = projView.Xml;

                LinkedObjectsValidation.VerifyNotLinkedNotNull(this.RealXml);
                LinkedObjectsValidation.VerifyLinkedNotNull(this.ViewXml);
            }

            public void ResetBeforeTests()
            {
                this.Group.ClearAllRemotes();

                var projView = this.Local.GetLoadedProjects(this.BigFile).FirstOrDefault();
                Assert.NotNull(projView);
                Assert.NotSame(projView, this.ViewXml);
                this.ViewXml = projView.Xml;

                LinkedObjectsValidation.VerifyLinkedNotNull(this.ViewXml);
            }
        }

        private static ROTestCollectionGroup xx = new ROTestCollectionGroup();
        private ROTestCollectionGroup StdGroup { get; }

        public LinkedConstructionReadOnly_Tests(ROTestCollectionGroup group)
        {
            this.StdGroup = group; // new ROTestCollectionGroup();
            this.StdGroup.ResetBeforeTests();
            // group.Clear();
        }


        // will be validated via ItemDefinition and Item tests. To much work to create a standalone UT.
        private void ValidateMetadataReadOnly(ProjectMetadataElement viewXml, ProjectMetadataElement realXml)
        {
            LinkedObjectsValidation.VerifyProjectElementView(viewXml, realXml, true);

            Assert.Equal(realXml.Name, viewXml.Name);
            Assert.Equal(realXml.Value, viewXml.Value);
            Assert.Equal(realXml.ExpressedAsAttribute, viewXml.ExpressedAsAttribute);
        }

        private void ValidateMetadataCollectionReadOnly(IEnumerable<ProjectMetadataElement> viewXmlCollection, IEnumerable<ProjectMetadataElement> realXmlCollection)
        {
            var viewXmlList = viewXmlCollection.ToList();
            var realXmlList = realXmlCollection.ToList();
            Assert.Equal(realXmlList.Count, viewXmlList.Count);
            for (int i= 0; i< realXmlList.Count; i++)
            {
                ValidateMetadataReadOnly(viewXmlList[i], realXmlList[i]);
            }
        }


        [Fact]
        public void ProjectRootElemetReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            LinkedObjectsValidation.VerifyProjectElementContainerView(preView, preReal, true);

            Assert.Equal(preReal.FullPath, preView.FullPath);
            Assert.Equal(preReal.DirectoryPath, preView.DirectoryPath);
            Assert.Equal(preReal.Encoding, preView.Encoding);
            Assert.Equal(preReal.DefaultTargets, preView.DefaultTargets);
            Assert.Equal(preReal.InitialTargets, preView.InitialTargets);
            Assert.Equal(preReal.Sdk, preView.Sdk);
            Assert.Equal(preReal.TreatAsLocalProperty, preView.TreatAsLocalProperty);
            Assert.Equal(preReal.ToolsVersion, preView.ToolsVersion);
            Assert.Equal(preReal.HasUnsavedChanges, preView.HasUnsavedChanges);
            Assert.Equal(preReal.PreserveFormatting, preView.PreserveFormatting);
            Assert.Equal(preReal.Version, preView.Version);
            Assert.Equal(preReal.TimeLastChanged, preView.TimeLastChanged);
            Assert.Equal(preReal.LastWriteTimeWhenRead, preView.LastWriteTimeWhenRead);

            LinkedObjectsValidation.VerifySameLocation(preReal.ProjectFileLocation, preView.ProjectFileLocation);
            LinkedObjectsValidation.VerifySameLocation(preReal.ToolsVersionLocation, preView.ToolsVersionLocation);
            LinkedObjectsValidation.VerifySameLocation(preReal.DefaultTargetsLocation, preView.DefaultTargetsLocation);
            LinkedObjectsValidation.VerifySameLocation(preReal.InitialTargetsLocation, preView.InitialTargetsLocation);
            LinkedObjectsValidation.VerifySameLocation(preReal.SdkLocation, preView.SdkLocation);
            LinkedObjectsValidation.VerifySameLocation(preReal.TreatAsLocalPropertyLocation, preView.TreatAsLocalPropertyLocation);
        }
        
        [Fact]
        public void ProjectChooseElemetReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            Assert.Single(preReal.ChooseElements);
            Assert.Single(preView.ChooseElements);

            var realChoose = preReal.ChooseElements.FirstOrDefault();
            var viewChoose = preView.ChooseElements.FirstOrDefault();

            LinkedObjectsValidation.VerifyProjectElementView(viewChoose, realChoose, true);
        }

        [Fact]
        public void ProjectExtensionsElementReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realExtensionsList = preReal.ChildrenReversed.OfType<ProjectExtensionsElement>().ToList();
            var viewExtensionsList = preView.ChildrenReversed.OfType<ProjectExtensionsElement>().ToList();

            Assert.Single(realExtensionsList);
            Assert.Single(viewExtensionsList);
            var realExtension = realExtensionsList.FirstOrDefault();
            var viewExtension = viewExtensionsList.FirstOrDefault();
            LinkedObjectsValidation.VerifyProjectElementView(viewExtension, realExtension, true);
            Assert.Equal(realExtension.Content, viewExtension.Content);

            Assert.Equal(realExtension["a"], viewExtension["a"]);
            Assert.Equal(realExtension["b"], viewExtension["b"]);
            Assert.Equal("x", viewExtension["a"]);
            Assert.Equal("y", viewExtension["b"]);
        }

        [Fact]
        public void ProjectImportElementReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realImports = preReal.Imports.ToList();
            var viewImports = preView.Imports.ToList();

            // note it is union of standalone and group imports.
            Assert.Equal(6, realImports.Count);
            Assert.Equal(realImports.Count, viewImports.Count);

            for (int i = 0; i< realImports.Count; i++)
            {
                var viewImport = viewImports[i];
                var realImport = realImports[i];
                LinkedObjectsValidation.VerifyProjectElementView(viewImport, realImport, true);

                Assert.Equal(realImport.Project, viewImport.Project);
                LinkedObjectsValidation.VerifySameLocation(realImport.ProjectLocation, viewImport.ProjectLocation);

                // mostly test the remoting infrastructure. Sdk Imports are not really covered by simple samples for now.
                // Todo: add mock SDK import closure to SdtGroup?
                Assert.Equal(realImport.Sdk, viewImport.Sdk);
                Assert.Equal(realImport.Version, viewImport.Version);
                Assert.Equal(realImport.MinimumVersion, viewImport.MinimumVersion);
                LinkedObjectsValidation.VerifySameLocation(realImport.SdkLocation, viewImport.SdkLocation);
                Assert.Equal(realImport.ImplicitImportLocation, viewImport.ImplicitImportLocation);
                LinkedObjectsValidation.VerifyProjectElementView(viewImport.OriginalElement, realImport.OriginalElement, true);
            }
        }

        [Fact]
        public void ProjectImportGroupElementReadOnly()
        {
            var strt = Environment.TickCount;
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realImportGroups = preReal.ImportGroups.ToList();
            var viewImportGroups = preView.ImportGroups.ToList();

            Assert.Equal(2, realImportGroups.Count);
            Assert.Equal(realImportGroups.Count, viewImportGroups.Count);

            for (int i = 0; i < realImportGroups.Count; i++)
            {
                var viewImportGroup = viewImportGroups[i];
                var realImportGroup = realImportGroups[i];
                LinkedObjectsValidation.VerifyProjectElementView(viewImportGroup, realImportGroup, true);

                Assert.Equal(realImportGroup.Imports.Count, viewImportGroup.Imports.Count);
            }
            Console.WriteLine($"ProjectImportGroupElementReadOnly:{Environment.TickCount - strt}");
        }

        [Fact]
        public void ProjectItemDefinitionElementReadOnly()
        {
            var strt = Environment.TickCount;
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realItemDefinitions = preReal.ItemDefinitions.ToList();
            var viewlItemDefinitions = preView.ItemDefinitions.ToList();

            Assert.Equal(3, realItemDefinitions.Count);
            Assert.Equal(realItemDefinitions.Count, viewlItemDefinitions.Count);

            for (int i = 0; i < realItemDefinitions.Count; i++)
            {
                var viewItemDef = viewlItemDefinitions[i];
                var realItemDef = realItemDefinitions[i];
                LinkedObjectsValidation.VerifyProjectElementView(viewItemDef, realItemDef, true);

                Assert.Equal(realItemDef.ItemType, viewItemDef.ItemType);
                ValidateMetadataCollectionReadOnly(viewItemDef.Metadata, realItemDef.Metadata);
            }
            Console.WriteLine($"ProjectItemDefinitionElementReadOnly:{Environment.TickCount -strt}");

        }
    }
}

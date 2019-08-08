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

    /// <summary>
    /// Most importantly we want to touch implementation to all public method to catch any
    /// potential transitional error.
    ///
    /// Since we have 2 independent views of the same object we have the "luxury" to do a full complete validation.
    /// </summary>
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
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realImportGroups = preReal.ImportGroups.ToList();
            var viewImportGroups = preView.ImportGroups.ToList();

            Assert.NotEmpty(realImportGroups);
            Assert.Equal(realImportGroups.Count, viewImportGroups.Count);

            for (int i = 0; i < realImportGroups.Count; i++)
            {
                var viewImportGroup = viewImportGroups[i];
                var realImportGroup = realImportGroups[i];
                LinkedObjectsValidation.VerifyProjectElementView(viewImportGroup, realImportGroup, true);

                Assert.Equal(realImportGroup.Imports.Count, viewImportGroup.Imports.Count);
            }
        }

        [Fact]
        public void ProjectItemDefinitionElementReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realItemDefinitions = preReal.ItemDefinitions.ToList();
            var viewlItemDefinitions = preView.ItemDefinitions.ToList();

            Assert.NotEmpty(realItemDefinitions);
            Assert.Equal(realItemDefinitions.Count, viewlItemDefinitions.Count);

            for (int i = 0; i < realItemDefinitions.Count; i++)
            {
                var viewItemDef = viewlItemDefinitions[i];
                var realItemDef = realItemDefinitions[i];
                LinkedObjectsValidation.VerifyProjectElementView(viewItemDef, realItemDef, true);

                Assert.Equal(realItemDef.ItemType, viewItemDef.ItemType);
                ValidateMetadataCollectionReadOnly(viewItemDef.Metadata, realItemDef.Metadata);
            }
        }

        [Fact]
        public void ProjectItemDefinitionGroupElementReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realItemDefinitionGroups = preReal.ItemDefinitionGroups.ToList();
            var viewlItemDefinitionGroups = preView.ItemDefinitionGroups.ToList();

            Assert.NotEmpty(realItemDefinitionGroups);
            Assert.Equal(realItemDefinitionGroups.Count, viewlItemDefinitionGroups.Count);

            for (int i = 0; i < realItemDefinitionGroups.Count; i++)
            {
                var viewItemDefGroup = viewlItemDefinitionGroups[i];
                var realItemDefGroup = realItemDefinitionGroups[i];
                LinkedObjectsValidation.VerifyProjectElementView(viewItemDefGroup, realItemDefGroup, true);

                Assert.Equal(viewItemDefGroup.ItemDefinitions.Count, viewItemDefGroup.ItemDefinitions.Count);
            }
        }

        [Fact]
        public void ProjectItemElementReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realItems = preReal.Items.ToList();
            var viewlItems = preView.Items.ToList();

            Assert.NotEmpty(realItems);
            Assert.Equal(realItems.Count, viewlItems.Count);

            for (int i = 0; i < realItems.Count; i++)
            {
                var viewItem = viewlItems[i];
                var realItem = realItems[i];
                LinkedObjectsValidation.VerifyProjectElementView(viewItem, realItem, true);

                Assert.Equal(realItem.ItemType, viewItem.ItemType);
                Assert.Equal(realItem.Include, viewItem.Include);
                Assert.Equal(realItem.Exclude, viewItem.Exclude);
                Assert.Equal(realItem.Remove, viewItem.Remove);
                Assert.Equal(realItem.Update, viewItem.Update);
                Assert.Equal(realItem.KeepMetadata, viewItem.KeepMetadata);
                Assert.Equal(realItem.RemoveMetadata, viewItem.RemoveMetadata);
                Assert.Equal(realItem.KeepDuplicates, viewItem.KeepDuplicates);
                Assert.Equal(realItem.HasMetadata, viewItem.HasMetadata);
                ValidateMetadataCollectionReadOnly(viewItem.Metadata, realItem.Metadata);

                LinkedObjectsValidation.VerifySameLocation(realItem.IncludeLocation, viewItem.IncludeLocation);
                LinkedObjectsValidation.VerifySameLocation(realItem.ExcludeLocation, viewItem.ExcludeLocation);
                LinkedObjectsValidation.VerifySameLocation(realItem.RemoveLocation, viewItem.RemoveLocation);
                LinkedObjectsValidation.VerifySameLocation(realItem.UpdateLocation, viewItem.UpdateLocation);
                LinkedObjectsValidation.VerifySameLocation(realItem.KeepMetadataLocation, viewItem.KeepMetadataLocation);
                LinkedObjectsValidation.VerifySameLocation(realItem.RemoveMetadataLocation, viewItem.RemoveMetadataLocation);
                LinkedObjectsValidation.VerifySameLocation(realItem.KeepDuplicatesLocation, viewItem.KeepDuplicatesLocation);
            }
        }

        [Fact]
        public void ProjectItemGroupElementReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realItemGroups = preReal.ItemGroups.ToList();
            var viewItemGroups = preView.ItemGroups.ToList();

            Assert.NotEmpty(realItemGroups);
            Assert.Equal(realItemGroups.Count, viewItemGroups.Count);

            for (int i = 0; i < realItemGroups.Count; i++)
            {
                var viewItemGroup = viewItemGroups[i];
                var realItemGroup = realItemGroups[i];
                LinkedObjectsValidation.VerifyProjectElementView(viewItemGroup, realItemGroup, true);

                Assert.Equal(viewItemGroup.Items.Count, viewItemGroup.Items.Count);
            }
        }

        [Fact]
        public void ProjectPropertyElementReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realProperties = preReal.Properties.ToList();
            var viewProperties = preView.Properties.ToList();

            Assert.NotEmpty(realProperties);
            Assert.Equal(realProperties.Count, viewProperties.Count);

            for (int i = 0; i < realProperties.Count; i++)
            {
                var viewProperty = viewProperties[i];
                var realProperty = realProperties[i];
                LinkedObjectsValidation.VerifyProjectElementView(viewProperty, realProperty, true);
                Assert.Equal(realProperty.Name, viewProperty.Name);
                Assert.Equal(realProperty.Value, viewProperty.Value);
            }
        }

        [Fact]
        public void ProjectPropertyGroupElementReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realPropertieGroups = preReal.PropertyGroups.ToList();
            var viewPropertieGroups = preView.PropertyGroups.ToList();

            Assert.NotEmpty(realPropertieGroups);
            Assert.Equal(realPropertieGroups.Count, viewPropertieGroups.Count);

            for (int i = 0; i < realPropertieGroups.Count; i++)
            {
                var viewPropertyGroup = viewPropertieGroups[i];
                var realPropertyGroup = realPropertieGroups[i];
                LinkedObjectsValidation.VerifyProjectElementView(viewPropertyGroup, realPropertyGroup, true);

                Assert.Equal(realPropertyGroup.Properties.Count, viewPropertyGroup.Properties.Count);
                Assert.Equal(realPropertyGroup.PropertiesReversed.Count, viewPropertyGroup.PropertiesReversed.Count);
            }
        }

        [Fact]
        public void ProjectOtherwiseElementReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realCollection = preReal.AllChildren.OfType<ProjectOtherwiseElement>().ToList();
            var viewCollection = preView.AllChildren.OfType<ProjectOtherwiseElement>().ToList();

            Assert.NotEmpty(realCollection);
            Assert.Equal(realCollection.Count, viewCollection.Count);

            for (int i = 0; i < realCollection.Count; i++)
            {
                var viewElement = viewCollection[i];
                var realElement = realCollection[i];
                LinkedObjectsValidation.VerifyProjectElementView(viewElement, realElement, true);
            }
        }

        [Fact]
        public void ProjectProjectWhenElementReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realCollection = preReal.AllChildren.OfType<ProjectWhenElement>().ToList();
            var viewCollection = preView.AllChildren.OfType<ProjectWhenElement>().ToList();

            Assert.NotEmpty(realCollection);
            Assert.Equal(realCollection.Count, viewCollection.Count);

            for (int i = 0; i < realCollection.Count; i++)
            {
                var viewElement = viewCollection[i];
                var realElement = realCollection[i];
                LinkedObjectsValidation.VerifyProjectElementView(viewElement, realElement, true);
            }
        }

        [Fact(Skip = "todo: need to figuew out how to add Sdk element")]
        public void ProjectProjectSdkElementReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realCollection = preReal.AllChildren.OfType<ProjectSdkElement>().ToList();
            var viewCollection = preView.AllChildren.OfType<ProjectSdkElement>().ToList();

            Assert.NotEmpty(realCollection);
            Assert.Equal(realCollection.Count, viewCollection.Count);

            for (int i = 0; i < realCollection.Count; i++)
            {
                var viewElement = viewCollection[i];
                var realElement = realCollection[i];
                LinkedObjectsValidation.VerifyProjectElementView(viewElement, realElement, true);
            }
        }

        [Fact]
        public void ProjectTargetElementReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realCollection = preReal.Targets.ToList();
            var viewCollection = preView.Targets.ToList();

            Assert.NotEmpty(realCollection);
            Assert.Equal(realCollection.Count, viewCollection.Count);

            for (int i = 0; i < realCollection.Count; i++)
            {
                var viewTarget = viewCollection[i];
                var realTarget = realCollection[i];
                LinkedObjectsValidation.VerifyProjectElementView(viewTarget, realTarget, true);

                Assert.Equal(realTarget.Name, viewTarget.Name);
                LinkedObjectsValidation.VerifySameLocation(realTarget.NameLocation, viewTarget.NameLocation);
                Assert.Equal(realTarget.Inputs, viewTarget.Inputs);
                LinkedObjectsValidation.VerifySameLocation(realTarget.InputsLocation, viewTarget.InputsLocation);
                Assert.Equal(realTarget.Outputs, viewTarget.Outputs);
                LinkedObjectsValidation.VerifySameLocation(realTarget.OutputsLocation, viewTarget.OutputsLocation);
                Assert.Equal(realTarget.KeepDuplicateOutputs, viewTarget.KeepDuplicateOutputs);
                LinkedObjectsValidation.VerifySameLocation(realTarget.KeepDuplicateOutputsLocation, viewTarget.KeepDuplicateOutputsLocation);
                Assert.Equal(realTarget.DependsOnTargets, viewTarget.DependsOnTargets);
                LinkedObjectsValidation.VerifySameLocation(realTarget.DependsOnTargetsLocation, viewTarget.DependsOnTargetsLocation);
                Assert.Equal(realTarget.BeforeTargets, viewTarget.BeforeTargets);
                LinkedObjectsValidation.VerifySameLocation(realTarget.BeforeTargetsLocation, viewTarget.BeforeTargetsLocation);
                Assert.Equal(realTarget.AfterTargets, viewTarget.AfterTargets);
                LinkedObjectsValidation.VerifySameLocation(realTarget.AfterTargetsLocation, viewTarget.AfterTargetsLocation);
                Assert.Equal(realTarget.Returns, viewTarget.Returns);
                LinkedObjectsValidation.VerifySameLocation(realTarget.ReturnsLocation, viewTarget.ReturnsLocation);
            }
        }
        ProjectTargetElement x7;
        ProjectTaskElement x8;

        UsingTaskParameterGroupElement x13;
        ProjectUsingTaskElement x10;
        ProjectUsingTaskBodyElement x9;
        ProjectUsingTaskParameterElement x11;

        ProjectOnErrorElement x1;
        ProjectOutputElement x3;
        ProjectSdkElement x6;


    }
}

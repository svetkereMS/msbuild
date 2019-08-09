// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Schema;
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

                ViewValidation.VerifyNotLinkedNotNull(this.RealXml);
                ViewValidation.VerifyLinkedNotNull(this.ViewXml);
            }

            public void ResetBeforeTests()
            {
                this.Group.ClearAllRemotes();

                var projView = this.Local.GetLoadedProjects(this.BigFile).FirstOrDefault();
                Assert.NotNull(projView);
                Assert.NotSame(projView, this.ViewXml);
                this.ViewXml = projView.Xml;

                ViewValidation.VerifyLinkedNotNull(this.ViewXml);
            }
        }

        private ROTestCollectionGroup StdGroup { get; }

        public LinkedConstructionReadOnly_Tests(ROTestCollectionGroup group)
        {
            this.StdGroup = group; // new ROTestCollectionGroup();
            this.StdGroup.ResetBeforeTests();
            // group.Clear();
        }


        [Fact]
        public void ProjectRootElemetReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            ViewValidation.Verify(preView, preReal);
        }
        
        [Fact]
        public void ProjectChooseElemetReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            Assert.NotEmpty(preReal.ChooseElements);

            ViewValidation.Verify(preView.ChooseElements, preReal.ChooseElements, ViewValidation.VerifyProjectElement);
        }

        [Fact]
        public void ProjectExtensionsElementReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realExtensionsList = preReal.ChildrenReversed.OfType<ProjectExtensionsElement>().ToList();
            var viewExtensionsList = preView.ChildrenReversed.OfType<ProjectExtensionsElement>().ToList();

            Assert.NotEmpty(realExtensionsList);

            ViewValidation.Verify(viewExtensionsList, realExtensionsList, ViewValidation.Verify);
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
                ViewValidation.VerifyProjectElement(viewImport, realImport);

                Assert.Equal(realImport.Project, viewImport.Project);
                ViewValidation.VerifySameLocation(realImport.ProjectLocation, viewImport.ProjectLocation);

                // mostly test the remoting infrastructure. Sdk Imports are not really covered by simple samples for now.
                // Todo: add mock SDK import closure to SdtGroup?
                Assert.Equal(realImport.Sdk, viewImport.Sdk);
                Assert.Equal(realImport.Version, viewImport.Version);
                Assert.Equal(realImport.MinimumVersion, viewImport.MinimumVersion);
                ViewValidation.VerifySameLocation(realImport.SdkLocation, viewImport.SdkLocation);
                Assert.Equal(realImport.ImplicitImportLocation, viewImport.ImplicitImportLocation);
                ViewValidation.VerifyProjectElement(viewImport.OriginalElement, realImport.OriginalElement);
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
                ViewValidation.VerifyProjectElement(viewImportGroup, realImportGroup);

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
                ViewValidation.VerifyProjectElement(viewItemDef, realItemDef);

                Assert.Equal(realItemDef.ItemType, viewItemDef.ItemType);
                ViewValidation.Verify(viewItemDef.Metadata, realItemDef.Metadata, ViewValidation.Verify);
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
                ViewValidation.VerifyProjectElement(viewItemDefGroup, realItemDefGroup);

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
                ViewValidation.VerifyProjectElement(viewItem, realItem);

                Assert.Equal(realItem.ItemType, viewItem.ItemType);
                Assert.Equal(realItem.Include, viewItem.Include);
                Assert.Equal(realItem.Exclude, viewItem.Exclude);
                Assert.Equal(realItem.Remove, viewItem.Remove);
                Assert.Equal(realItem.Update, viewItem.Update);
                Assert.Equal(realItem.KeepMetadata, viewItem.KeepMetadata);
                Assert.Equal(realItem.RemoveMetadata, viewItem.RemoveMetadata);
                Assert.Equal(realItem.KeepDuplicates, viewItem.KeepDuplicates);
                Assert.Equal(realItem.HasMetadata, viewItem.HasMetadata);

                ViewValidation.Verify(viewItem.Metadata, realItem.Metadata, ViewValidation.Verify);

                ViewValidation.VerifySameLocation(realItem.IncludeLocation, viewItem.IncludeLocation);
                ViewValidation.VerifySameLocation(realItem.ExcludeLocation, viewItem.ExcludeLocation);
                ViewValidation.VerifySameLocation(realItem.RemoveLocation, viewItem.RemoveLocation);
                ViewValidation.VerifySameLocation(realItem.UpdateLocation, viewItem.UpdateLocation);
                ViewValidation.VerifySameLocation(realItem.KeepMetadataLocation, viewItem.KeepMetadataLocation);
                ViewValidation.VerifySameLocation(realItem.RemoveMetadataLocation, viewItem.RemoveMetadataLocation);
                ViewValidation.VerifySameLocation(realItem.KeepDuplicatesLocation, viewItem.KeepDuplicatesLocation);
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
                ViewValidation.VerifyProjectElement(viewItemGroup, realItemGroup);

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
                ViewValidation.VerifyProjectElement(viewProperty, realProperty);
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
                ViewValidation.VerifyProjectElement(viewPropertyGroup, realPropertyGroup);

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
                ViewValidation.VerifyProjectElement(viewElement, realElement);
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
                ViewValidation.VerifyProjectElement(viewElement, realElement);
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
                ViewValidation.VerifyProjectElement(viewElement, realElement);
            }
        }

        [Fact]
        public void ProjectTargetElementReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realCollection = preReal.Targets.ToList();
            var viewCollection = preView.Targets.ToList();

            Assert.NotEmpty(realCollection);  // to ensure we actually have some elements in test project
            ViewValidation.Verify(viewCollection, realCollection, ViewValidation.Verify);
        }

        /// Also validates
        /// ProjectOutputElement
        /// 
        [Fact]
        public void ProjectTaskElementReadOnly()
        {
            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realCollection = preReal.AllChildren.OfType<ProjectTaskElement>().ToList();
            var viewCollection = preView.AllChildren.OfType<ProjectTaskElement>().ToList();

            Assert.NotEmpty(realCollection);  // to ensure we actually have some elements in test project
            ViewValidation.Verify(viewCollection, realCollection, ViewValidation.Verify);
        }

        // Also validates:
        // ProjectUsingTaskBodyElement
        // UsingTaskParameterGroupElement
        // ProjectUsingTaskParameterElement
        [Fact]
        public void ProjectUsingTaskElementReadOnly()
        {

            var preReal = this.StdGroup.RealXml;
            var preView = this.StdGroup.ViewXml;

            var realCollection = preReal.AllChildren.OfType<ProjectUsingTaskElement>().ToList();
            var viewCollection = preView.AllChildren.OfType<ProjectUsingTaskElement>().ToList();

            Assert.NotEmpty(realCollection); // to ensure we actually have some elements in test project
            ViewValidation.Verify(viewCollection, realCollection, ViewValidation.Verify);
        }

        ProjectOnErrorElement x1;
        ProjectOutputElement x3;
        ProjectSdkElement x6;


    }
}

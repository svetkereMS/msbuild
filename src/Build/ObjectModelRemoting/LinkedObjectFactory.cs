// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.ObjectModelRemoting
{
    using System.Collections.Generic;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Framework;

    /// <summary>
    /// Provide facility to ExternalProjectsProvider implementation
    /// to create local OM objects based on the remote link.
    /// These object are fully useful for associated Collection.
    /// </summary>
    public class LinkedObjectsFactory
    {
        public static LinkedObjectsFactory Get(ProjectCollection collection)
        {
            return new LinkedObjectsFactory(collection);
        }

        private LinkedObjectsFactory(ProjectCollection collection)
        {
            Collection = collection;
        }

        public ProjectCollection Collection { get; }

        #region Construction

        public ProjectItem Create(ProjectItemLink link, Project project = null, ProjectItemElement xml = null)
        {
            project = project ?? link.Project;
            xml = xml ?? link.Xml;

            return new LinkedProjectItem(xml, project, link);
        }

        public ProjectItemDefinition Create(ProjectItemDefinitionLink link, Project project = null)
        {
            project = project ?? link.Project;

            return new LinkedProjectItemDefinition(link, project, link.ItemType);
        }

        public Project Create(ProjectLink link)
        {
            return new Project(Collection, link);
        }

        public ProjectMetadata Create(ProjectMetadataLink link, object parent = null)
        {
            parent = parent ?? link.Parent;

            return new LinkedProjectMetadata(parent, link);
        }

        public ProjectProperty Create(ProjectPropertyLink link, Project project = null )
        {
            project = project ?? link.Project;

            return new LinkedProjectProperty(project, link);
        }

        public ResolvedImport Create(Project project, ProjectImportElement importingElement, ProjectRootElement importedProject, int versionEvaluated, SdkResult sdkResult)
        {
            return new ResolvedImport(project, importingElement, importedProject, versionEvaluated, sdkResult);
        }

        #endregion

        #region Construction

        public ProjectRootElement Create(ProjectRootElementLink link)
        {
            return new ProjectRootElement(link);
        }

        public ProjectChooseElement Create(ProjectChooseElementLink link)
        {
            return new ProjectChooseElement(link);
        }

        public ProjectExtensionsElement Create(ProjectExtensionsElementLink link)
        {
            return new ProjectExtensionsElement(link);
        }

        public ProjectImportElement Create(ProjectImportElementLink link)
        {
            return new ProjectImportElement(link);
        }

        public ProjectImportGroupElement Create(ProjectImportGroupElementLink link)
        {
            return new ProjectImportGroupElement(link);
        }

        public ProjectItemDefinitionElement Create(ProjectItemDefinitionElementLink link)
        {
            return new ProjectItemDefinitionElement(link);
        }

        public ProjectItemDefinitionGroupElement Create(ProjectItemDefinitionGroupElementLink link)
        {
            return new ProjectItemDefinitionGroupElement(link);
        }

        public ProjectItemElement Create(ProjectItemElementLink link)
        {
            return new ProjectItemElement(link);
        }

        public ProjectItemGroupElement Create(ProjectItemGroupElementLink link)
        {
            return new ProjectItemGroupElement(link);
        }

        public ProjectMetadataElement Create(ProjectMetadataElementLink link)
        {
            return new ProjectMetadataElement(link);
        }

        public ProjectOnErrorElement Create(ProjectOnErrorElementLink link)
        {
            return new ProjectOnErrorElement(link);
        }

        public ProjectOtherwiseElement Create(ProjectOtherwiseElementLink link)
        {
            return new ProjectOtherwiseElement(link);
        }

        public ProjectOutputElement Create(ProjectOutputElementLink link)
        {
            return new ProjectOutputElement(link);
        }

        public ProjectPropertyElement Create(ProjectPropertyElementLink link)
        {
            return new ProjectPropertyElement(link);
        }

        public ProjectPropertyGroupElement Create(ProjectPropertyGroupElementLink link)
        {
            return new ProjectPropertyGroupElement(link);
        }
        public ProjectSdkElement Create(ProjectSdkElementLink link)
        {
            return new ProjectSdkElement(link);
        }
        public ProjectTargetElement Create(ProjectTargetElementLink link)
        {
            return new ProjectTargetElement(link);
        }
        public ProjectTaskElement Create(ProjectTaskElementLink link)
        {
            return new ProjectTaskElement(link);
        }
        public ProjectUsingTaskBodyElement Create(ProjectUsingTaskBodyElementLink link)
        {
            return new ProjectUsingTaskBodyElement(link);
        }
        public ProjectUsingTaskElement Create(ProjectUsingTaskElementLink link)
        {
            return new ProjectUsingTaskElement(link);
        }
        public ProjectUsingTaskParameterElement Create(ProjectUsingTaskParameterElementLink link)
        {
            return new ProjectUsingTaskParameterElement(link);
        }
        public ProjectWhenElement Create(ProjectWhenElementLink link)
        {
            return new ProjectWhenElement(link);
        }
        public UsingTaskParameterGroupElement Create(UsingTaskParameterGroupElementLink link)
        {
            return new UsingTaskParameterGroupElement(link);
        }
        #endregion

        #region Linked classes helpers
        // Using the pattern with overloaded classes that provide "Link" object so we ensure we do not increase the
        // memory storage of original items (with the Link field) while it is small, some of the MSbuild items can be created
        // in millions so it does adds up otherwise.

        private class LinkedProjectItem : ProjectItem
        {
            internal LinkedProjectItem(ProjectItemElement xml, Project project, ProjectItemLink link)
                : base(xml, project)
            {
                this.Link = link;
            }

            internal override ProjectItemLink Link { get; }
        }

        private class LinkedProjectItemDefinition : ProjectItemDefinition
        {
            internal LinkedProjectItemDefinition(ProjectItemDefinitionLink link, Project project, string itemType)
                : base(project, itemType)
            {
                Link = link;
            }

            internal override ProjectItemDefinitionLink Link { get; }
        }

        private class LinkedProjectMetadata : ProjectMetadata
        {
            internal LinkedProjectMetadata(object parent, ProjectMetadataLink link)
                : base(parent, link.Xml)
            {
                Link = link;
            }

            internal override ProjectMetadataLink Link { get; }
        }

        private class LinkedProjectProperty : ProjectProperty
        {
            internal ProjectPropertyLink Link { get; }

            /// <summary>
            /// Creates a regular evaluated property, with backing XML.
            /// Called by Project.SetProperty.
            /// Property MAY NOT have reserved name and MAY NOT overwrite a global property.
            /// Predecessor is any immediately previous property that was overridden by this one during evaluation and may be null.
            /// </summary>
            internal LinkedProjectProperty(Project project, ProjectPropertyLink link)
                : base(project)
            {
                Link = link;
            }

            public override string Name => Link.Name;

            public override string UnevaluatedValue
            {
                get => Link.UnevaluatedValue;
                set => Link.UnevaluatedValue = value;
            }

            public override bool IsEnvironmentProperty => Link.IsEnvironmentProperty;

            public override bool IsGlobalProperty => Link.IsGlobalProperty;

            public override bool IsReservedProperty => Link.IsReservedProperty;

            public override ProjectPropertyElement Xml => Link.Xml;

            public override ProjectProperty Predecessor => Link.Predecessor;

            public override bool IsImported => Link.IsImported;
        }
        #endregion
    }


}

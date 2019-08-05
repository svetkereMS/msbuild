// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.ObjectModelRemoting
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Framework;

    /// <summary>
    /// implemented by MSBuild objects that support remote linking;
    /// </summary>
    internal interface ILinkableObject
    {
        /// <summary>
        /// Gets the current link, if any. For local objects returns null;
        /// </summary>
        object Link { get; }
    }

    /// <summary>
    /// Provide facility to ExternalProjectsProvider implementation
    /// to create local OM objects based on the remote link.
    /// These object are fully useful for associated Collection.
    /// </summary>
    public class LinkedObjectsFactory
    {
        private LinkedObjectsFactory(ProjectCollection collection)
        {
            Collection = collection;
        }

        /// <summary>
        /// Acquire a <see cref="LinkedObjectsFactory"/> instance for a given ProjectCollection.
        /// Allows creating a local MSBuild OM objects representing externally hosted Projects.
        /// </summary>
        public static LinkedObjectsFactory Get(ProjectCollection collection)
        {
            return new LinkedObjectsFactory(collection);
        }

        /// <summary>
        /// Local collection.
        /// </summary>
        public ProjectCollection Collection { get; }

        /// <summary>
        /// Get the underlying "link" proxy for a given MSBuild object model object (null if it is not linked).
        /// can be used by ExternalProjectsProvider to prevent double linking when implementing remote calls.
        /// </summary>
        public object GetLink(object localObject)
        {
            var linkable = localObject as ILinkableObject;
            return linkable?.Link;
        }

        public bool GetExpressedAsAttribute(ProjectElement xml)
        {
            return xml.ExpressedAsAttribute;
        }

        public void SetExpressedAsAttribute(ProjectElement xml, bool value)
        {
            xml.ExpressedAsAttribute = value;
        }

        public ElementLocation GetAttributeLocation(ProjectElement xml, string attributeName)
        {
            return xml.GetAttributeLocation(attributeName);
        }
        public string GetAttributeValue(ProjectElement xml, string attributeName, bool nullIfNotExists)
        {
            return xml.GetAttributeValue(attributeName, nullIfNotExists);
        }

        public void SetOrRemoveAttribute(ProjectElement xml, string name, string value, bool allowSettingEmptyAttributes, string reason, string param)
        {
            xml.SetOrRemoveAttribute(name, value, reason, param);
        }

        public void MarkDirty(ProjectElement xml, string reason, string param)
        {
            xml.MarkDirty(reason, param);
        }

        public void AddInitialChild(ProjectElementContainer xml, ProjectElement child)
        {
            xml.AddInitialChild(child);
        }

        public ProjectElementContainer DeepClone(ProjectElementContainer xml, ProjectRootElement factory, ProjectElementContainer parent)
        {
            return ProjectElementContainer.DeepClone(xml, factory, parent);
        }

        public ProjectElement CreateNewInstance(ProjectElement xml, ProjectRootElement owner)
        {
            return ProjectElement.CreateNewInstance(xml, owner);
        }

        #region Evaluation

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
            // note we do not use wrapper LikedProjects class in this case.
            // Project element storage is in fact increased to support linked (with few bytes)
            // but since the Projects objects number are relatively low, this is not a big concern
            // as with other items that can be typically  1000s of times the number of projects.
            // That is done for simplicity, but if needed we can use the same approach here as well.
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

        public ResolvedImport Create(ProjectImportElement importingElement, ProjectRootElement importedProject, int versionEvaluated, SdkResult sdkResult, bool isImported)
        {
            return new ResolvedImport(importingElement, importedProject, versionEvaluated, sdkResult, isImported);
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

        private class LinkedProjectItem : ProjectItem, ILinkableObject
        {
            internal LinkedProjectItem(ProjectItemElement xml, Project project, ProjectItemLink link)
                : base(xml, project)
            {
                this.Link = link;
            }

            internal override ProjectItemLink Link { get; }

            object ILinkableObject.Link => Link;
        }

        private class LinkedProjectItemDefinition : ProjectItemDefinition, ILinkableObject
        {
            internal LinkedProjectItemDefinition(ProjectItemDefinitionLink link, Project project, string itemType)
                : base(project, itemType)
            {
                Link = link;
            }

            internal override ProjectItemDefinitionLink Link { get; }
            object ILinkableObject.Link => Link;
        }

        private class LinkedProjectMetadata : ProjectMetadata, ILinkableObject
        {
            internal LinkedProjectMetadata(object parent, ProjectMetadataLink link)
                : base(parent, link.Xml)
            {
                Link = link;
            }

            internal override ProjectMetadataLink Link { get; }
            object ILinkableObject.Link => Link;

        }

        private class LinkedProjectProperty : ProjectProperty, ILinkableObject
        {
            internal ProjectPropertyLink Link { get; }
            object ILinkableObject.Link => Link;

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

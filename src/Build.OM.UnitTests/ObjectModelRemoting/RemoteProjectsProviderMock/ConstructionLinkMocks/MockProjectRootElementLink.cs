// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;
    using Microsoft.Build.Construction;
    using Microsoft.Build.ObjectModelRemoting;

    internal class MockProjectRootElementLinkRemoter : MockProjectElementContainerLinkRemoter
    {
        ProjectRootElement ProjectXml => (ProjectRootElement)Source;

        public override ProjectElement CreateLinkedObject(ProjectCollectionLinker remote)
        {
            var link = new MockProjectRootElementLink(this, remote);
            return remote.LinkFactory.Create(link);
        }

        public override ProjectElement Import(ProjectCollectionLinker remote)
        {
            return remote.Import<ProjectElement, MockProjectRootElementLinkRemoter>(this);
        }

        // ProjectRootElementLink remoting
        public int Version => this.ProjectXml.Version;
        public DateTime TimeLastChanged => this.ProjectXml.TimeLastChanged;
        public DateTime LastWriteTimeWhenRead => this.ProjectXml.LastWriteTimeWhenRead;
        public string DirectoryPath => this.ProjectXml.DirectoryPath;
        public string FullPath { get => this.ProjectXml.FullPath; set => this.ProjectXml.FullPath = value; }
        public ElementLocation ProjectFileLocation => new ElementLocationR(this.ProjectXml.ProjectFileLocation);
        public Encoding Encoding => this.ProjectXml.Encoding; //!! more complicated in reality when passing cross process.
        public string RawXml => this.ProjectXml.RawXml;
        public bool PreserveFormatting => this.ProjectXml.PreserveFormatting;

        public ProjectChooseElement CreateChooseElement() { throw new NotImplementedException(); }
        public ProjectImportElement CreateImportElement(string project) { throw new NotImplementedException(); }
        public ProjectItemElement CreateItemElement(string itemType) { throw new NotImplementedException(); }
        public ProjectItemElement CreateItemElement(string itemType, string include) { throw new NotImplementedException(); }
        public ProjectItemDefinitionElement CreateItemDefinitionElement(string itemType) { throw new NotImplementedException(); }
        public ProjectItemDefinitionGroupElement CreateItemDefinitionGroupElement() { throw new NotImplementedException(); }
        public ProjectItemGroupElement CreateItemGroupElement() { throw new NotImplementedException(); }
        public ProjectImportGroupElement CreateImportGroupElement() { throw new NotImplementedException(); }
        public ProjectMetadataElement CreateMetadataElement(string name) { throw new NotImplementedException(); }
        public ProjectMetadataElement CreateMetadataElement(string name, string unevaluatedValue) { throw new NotImplementedException(); }
        public ProjectOnErrorElement CreateOnErrorElement(string executeTargets) { throw new NotImplementedException(); }
        public ProjectOtherwiseElement CreateOtherwiseElement() { throw new NotImplementedException(); }
        public ProjectOutputElement CreateOutputElement(string taskParameter, string itemType, string propertyName) { throw new NotImplementedException(); }
        public ProjectExtensionsElement CreateProjectExtensionsElement() { throw new NotImplementedException(); }
        public ProjectPropertyGroupElement CreatePropertyGroupElement() { throw new NotImplementedException(); }
        public ProjectPropertyElement CreatePropertyElement(string name) { throw new NotImplementedException(); }
        public ProjectTargetElement CreateTargetElement(string name) { throw new NotImplementedException(); }
        public ProjectTaskElement CreateTaskElement(string name) { throw new NotImplementedException(); }
        public ProjectUsingTaskElement CreateUsingTaskElement(string taskName, string assemblyFile, string assemblyName, string runtime, string architecture) { throw new NotImplementedException(); }
        public UsingTaskParameterGroupElement CreateUsingTaskParameterGroupElement() { throw new NotImplementedException(); }
        public ProjectUsingTaskParameterElement CreateUsingTaskParameterElement(string name, string output, string required, string parameterType) { throw new NotImplementedException(); }
        public ProjectUsingTaskBodyElement CreateUsingTaskBodyElement(string evaluate, string body) { throw new NotImplementedException(); }
        public ProjectWhenElement CreateWhenElement(string condition) { throw new NotImplementedException(); }
        public ProjectSdkElement CreateProjectSdkElement(string sdkName, string sdkVersion) { throw new NotImplementedException(); }
        public void Save(Encoding saveEncoding) { throw new NotImplementedException(); }
        public void Save(TextWriter writer) { throw new NotImplementedException(); }

        public void ReloadFrom(string path, bool throwIfUnsavedChanges, bool preserveFormatting) { throw new NotImplementedException(); }
        public void ReloadFrom(XmlReader reader, bool throwIfUnsavedChanges, bool preserveFormatting) { throw new NotImplementedException(); }

        public void MarkDirty(string reason, string param) { this.OwningCollection.LinkFactory.MarkDirty(this.Source, reason, param); }
    }



    internal class MockProjectRootElementLink : ProjectRootElementLink, ILinkMock, IProjectElementLinkHelper, IProjectElementContainerLinkHelper
    {
        public MockProjectRootElementLink(MockProjectRootElementLinkRemoter proxy, ProjectCollectionLinker linker)
        {
            this.Linker = linker;
            this.Proxy = proxy;
        }

        public ProjectCollectionLinker Linker { get; }
        public MockProjectRootElementLinkRemoter Proxy { get; }
        object ILinkMock.Remoter => this.Proxy;

        #region ProjectElementLink redirectors
        private IProjectElementLinkHelper EImpl => (IProjectElementLinkHelper)this;
        public override ProjectElementContainer Parent => EImpl.GetParent();
        public override ProjectRootElement ContainingProject => EImpl.GetContainingProject();
        public override string ElementName => EImpl.GetElementName();
        public override string OuterElement => EImpl.GetOuterElement();
        public override bool ExpressedAsAttribute { get => EImpl.GetExpressedAsAttribute(); set => EImpl.SetExpressedAsAttribute(value); }
        public override ProjectElement PreviousSibling => EImpl.GetPreviousSibling();
        public override ProjectElement NextSibling => EImpl.GetNextSibling();
        public override ElementLocation Location => EImpl.GetLocation();
        public override void CopyFrom(ProjectElement element) { EImpl.CopyFrom(element); }
        public override ProjectElement CreateNewInstance(ProjectRootElement owner) { return EImpl.CreateNewInstance(owner); }
        public override ElementLocation GetAttributeLocation(string attributeName) { return EImpl.GetAttributeLocation(attributeName); }
        public override string GetAttributeValue(string attributeName, bool nullIfNotExists) { return EImpl.GetAttributeValue(attributeName, nullIfNotExists); }
        public override void SetOrRemoveAttribute(string name, string value, bool allowSettingEmptyAttributes, string reason, string param)
        {
            EImpl.SetOrRemoveAttribute(name, value, allowSettingEmptyAttributes, reason, param);
        }
        #endregion

        #region ProjectElementContainer link redirectors
        private IProjectElementContainerLinkHelper CImpl => (IProjectElementContainerLinkHelper)this;
        public override int Count => CImpl.GetCount();
        public override ProjectElement FirstChild => CImpl.GetFirstChild();
        public override ProjectElement LastChild => CImpl.GetLastChild();
        public override void InsertAfterChild(ProjectElement child, ProjectElement reference) { CImpl.InsertAfterChild(child, reference); }
        public override void InsertBeforeChild(ProjectElement child, ProjectElement reference) { CImpl.InsertBeforeChild(child, reference); }
        public override void AddInitialChild(ProjectElement child) { CImpl.AddInitialChild(child); }
        public override ProjectElementContainer DeepClone(ProjectRootElement factory, ProjectElementContainer parent) { return CImpl.DeepClone(factory, parent); }
        public override void RemoveChild(ProjectElement child) { CImpl.RemoveChild(child); }
        #endregion

        // ProjectRootElementLink remoting
        public override int Version => Proxy.Version;
        public override DateTime TimeLastChanged => Proxy.TimeLastChanged;
        public override DateTime LastWriteTimeWhenRead => Proxy.LastWriteTimeWhenRead;
        public override string DirectoryPath => Proxy.DirectoryPath;
        public override string FullPath { get => Proxy.FullPath; set => Proxy.FullPath = value; }
        public override ElementLocation ProjectFileLocation => Proxy.ProjectFileLocation;
        public override Encoding Encoding => Proxy.Encoding;
        public override string RawXml => Proxy.RawXml;
        public override bool PreserveFormatting => Proxy.PreserveFormatting;

        public override ProjectChooseElement CreateChooseElement()
        {
            var remoter = Proxy.CreateChooseElement();
            throw new NotImplementedException();
        }

        public override ProjectImportElement CreateImportElement(string project)
        {
            var remoter = Proxy.CreateImportElement(project);
            throw new NotImplementedException();
        }

        public override ProjectItemElement CreateItemElement(string itemType)
        {
            var remoter = Proxy.CreateItemElement(itemType);
            throw new NotImplementedException();
        }

        public override ProjectItemElement CreateItemElement(string itemType, string include)
        {
            var remoter = Proxy.CreateItemElement(itemType, include);
            throw new NotImplementedException();

        }

        public override ProjectItemDefinitionElement CreateItemDefinitionElement(string itemType)
        {
            var remoter = Proxy.CreateItemDefinitionElement(itemType);
            throw new NotImplementedException();
        }

        public override ProjectItemDefinitionGroupElement CreateItemDefinitionGroupElement()
        {
            var remoter = Proxy.CreateItemDefinitionGroupElement();
            throw new NotImplementedException();
        }

        public override ProjectItemGroupElement CreateItemGroupElement()
        {
            var remoter = Proxy.CreateItemGroupElement();
            throw new NotImplementedException();
        }

        public override ProjectImportGroupElement CreateImportGroupElement()
        {
            var remoter = Proxy.CreateImportGroupElement();
            throw new NotImplementedException();
        }

        public override ProjectMetadataElement CreateMetadataElement(string name)
        {
            var remoter = Proxy.CreateMetadataElement(name);
            throw new NotImplementedException();

        }

        public override ProjectMetadataElement CreateMetadataElement(string name, string unevaluatedValue)
        {
            var remoter = Proxy.CreateMetadataElement(name, unevaluatedValue);
            throw new NotImplementedException();
        }

        public override ProjectOnErrorElement CreateOnErrorElement(string executeTargets)
        {
            var remoter = Proxy.CreateOnErrorElement(executeTargets);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Facilitate remoting the <see cref="ProjectRootElement.CreateOtherwiseElement"/>.
        /// </summary>
        public override ProjectOtherwiseElement CreateOtherwiseElement()
        {
            throw new NotImplementedException();
        }

        public override ProjectOutputElement CreateOutputElement(string taskParameter, string itemType, string propertyName)
        {
            throw new NotImplementedException();
        }
        public override ProjectExtensionsElement CreateProjectExtensionsElement()
        {
            throw new NotImplementedException();
        }

        public override ProjectPropertyGroupElement CreatePropertyGroupElement()
        {
            throw new NotImplementedException();
        }

        public override ProjectPropertyElement CreatePropertyElement(string name)
        {
            throw new NotImplementedException();
        }

        public override ProjectTargetElement CreateTargetElement(string name)
        {
            throw new NotImplementedException();
        }
        public override ProjectTaskElement CreateTaskElement(string name)
        {
            throw new NotImplementedException();
        }
        public override ProjectUsingTaskElement CreateUsingTaskElement(string taskName, string assemblyFile, string assemblyName, string runtime, string architecture)
        {
            throw new NotImplementedException();
        }
        public override UsingTaskParameterGroupElement CreateUsingTaskParameterGroupElement()
        {
            throw new NotImplementedException();
        }
        public override ProjectUsingTaskParameterElement CreateUsingTaskParameterElement(string name, string output, string required, string parameterType)
        {
            throw new NotImplementedException();
        }
        public override ProjectUsingTaskBodyElement CreateUsingTaskBodyElement(string evaluate, string body)
        {
            throw new NotImplementedException();
        }
        public override ProjectWhenElement CreateWhenElement(string condition)
        {
            throw new NotImplementedException();
        }
        public override ProjectSdkElement CreateProjectSdkElement(string sdkName, string sdkVersion)
        {
            throw new NotImplementedException();
        }
        public override void Save(Encoding saveEncoding)
        {
            throw new NotImplementedException();
        }
        public override void Save(TextWriter writer)
        {
            throw new NotImplementedException();
        }
        public override void ReloadFrom(string path, bool throwIfUnsavedChanges, bool preserveFormatting)
        {
            throw new NotImplementedException();
        }
        public override void ReloadFrom(XmlReader reader, bool throwIfUnsavedChanges, bool preserveFormatting)
        {
            throw new NotImplementedException();
        }
        public override void MarkDirty(string reason, string param)
        {
            this.Proxy.MarkDirty(reason, param);
        }
    }
}

﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.ObjectModelRemoting;

    internal class MockProjectTargetElementLinkRemoter : MockProjectElementContainerLinkRemoter
    {
        public ProjectTargetElement TargetXml => (ProjectTargetElement)Source;

        public override ProjectElement ImportImpl(ProjectCollectionLinker remote)
        {
            return remote.Import<ProjectElement, MockProjectTargetElementLinkRemoter>(this);
        }

        public override ProjectElement CreateLinkedObject(ProjectCollectionLinker remote)
        {
            var link = new MockProjectTargetElementLink(this, remote);
            return remote.LinkFactory.Create(link);
        }


        public string Returns { set => this.TargetXml.Returns = value; }
    }

    internal class MockProjectTargetElementLink : ProjectTargetElementLink, ILinkMock, IProjectElementLinkHelper, IProjectElementContainerLinkHelper
    {
        public MockProjectTargetElementLink(MockProjectTargetElementLinkRemoter proxy, ProjectCollectionLinker linker)
        {
            this.Linker = linker;
            this.Proxy = proxy;
        }

        public ProjectCollectionLinker Linker { get; }
        public MockProjectTargetElementLinkRemoter Proxy { get; }
        object ILinkMock.Remoter => this.Proxy;
        MockProjectElementLinkRemoter IProjectElementLinkHelper.ElementProxy => this.Proxy;
        MockProjectElementContainerLinkRemoter IProjectElementContainerLinkHelper.ContainerProxy => this.Proxy;

        // ProjectTargetElementLink -----
        public override string Returns { set => this.Proxy.Returns = value; }
        // ----------------------------

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
    }
}

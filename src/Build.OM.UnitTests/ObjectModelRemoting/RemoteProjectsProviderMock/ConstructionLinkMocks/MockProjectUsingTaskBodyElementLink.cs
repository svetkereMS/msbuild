// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.ObjectModelRemoting;

    internal class MockProjectUsingTaskBodyElementLinkRemoter : MockProjectElementLinkRemoter
    {
        public ProjectUsingTaskBodyElement UsingTaskBodyXml => (ProjectUsingTaskBodyElement)Source;

        public override ProjectElement ImportImpl(ProjectCollectionLinker remote)
        {
            return remote.Import<ProjectElement, MockProjectUsingTaskBodyElementLinkRemoter>(this);
        }

        public override ProjectElement CreateLinkedObject(ProjectCollectionLinker remote)
        {
            var link = new MockProjectUsingTaskBodyElementLink(this, remote);
            return remote.LinkFactory.Create(link);
        }

        // ProjectUsingTaskBodyElementLink support
        public string TaskBody { get => this.UsingTaskBodyXml.TaskBody; set => this.UsingTaskBodyXml.TaskBody = value; }
    }

    internal class MockProjectUsingTaskBodyElementLink : ProjectUsingTaskBodyElementLink, ILinkMock, IProjectElementLinkHelper
    {
        public MockProjectUsingTaskBodyElementLink(MockProjectUsingTaskBodyElementLinkRemoter proxy, ProjectCollectionLinker linker)
        {
            this.Linker = linker;
            this.Proxy = proxy;
        }

        public ProjectCollectionLinker Linker { get; }
        public MockProjectUsingTaskBodyElementLinkRemoter Proxy { get; }
        object ILinkMock.Remoter => this.Proxy;
        MockProjectElementLinkRemoter IProjectElementLinkHelper.ElementProxy => this.Proxy;

        // ProjectUsingTaskBodyElementLink
        public override string TaskBody { get => this.Proxy.TaskBody; set => this.Proxy.TaskBody = value; }

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
    }
}

namespace Microsoft.Build.Engine.OM.UnitTests.ObjectModelRemoting.RemoteProjectsProviderMock.ConstructionLinkMocks
{
    class MockProjectUsingTaskBodyElementLink
    {
    }
}

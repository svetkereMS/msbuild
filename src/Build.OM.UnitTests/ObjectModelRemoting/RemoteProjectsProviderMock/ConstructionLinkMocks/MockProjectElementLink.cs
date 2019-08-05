// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{
    using System;
    using Microsoft.Build.Construction;
    using Microsoft.Build.ObjectModelRemoting;

    internal abstract class MockProjectElementLinkRemoter : MockLinkRemoter<ProjectElement>
    {
        public MockProjectElementLinkRemoter Export(ProjectElement xml)
        {
            if (xml == null) return null;

            if (xml is ProjectElementContainer)
            {

            }
            else
            {
                var projectPropertyXml = xml as ProjectPropertyElement;
                if (xml != null)
                {
                    return OwningCollection.Export<ProjectElement, MockProjectPropertyElementLinkRemoter>(xml);
                }
            }

            throw new NotImplementedException();
        }

        public abstract ProjectElement Import(ProjectCollectionLinker remote);


        // ProjectElementLink remoting
        public MockProjectElementContainerLinkRemoter Parent => (MockProjectElementContainerLinkRemoter)this.Export(Source.Parent);

        public MockProjectRootElementLinkRemoter ContainingProject => (MockProjectRootElementLinkRemoter)this.Export(Source.ContainingProject);

        public string ElementName => Source.ElementName;

        public string OuterElement => Source.OuterElement;

        public bool ExpressedAsAttribute { get => OwningCollection.LinkFactory.GetExpressedAsAttribute(Source); set => OwningCollection.LinkFactory.SetExpressedAsAttribute(Source, value); }

        public MockProjectElementLinkRemoter PreviousSibling => this.Export(Source.PreviousSibling);

        public MockProjectElementLinkRemoter NextSibling => this.Export(Source.NextSibling);

        public ElementLocation Location => new ElementLocationR(Source.Location);

        public void CopyFrom(MockProjectElementLinkRemoter element)
        {
            this.Source.CopyFrom(element.Import(this.OwningCollection));
        }

        public MockProjectElementLinkRemoter CreateNewInstance(MockProjectRootElementLinkRemoter owner)
        {
            var pre = (ProjectRootElement)owner.Import(OwningCollection);
            var result = OwningCollection.LinkFactory.CreateNewInstance(Source, pre);
            return Export(result);
        }

        public ElementLocation GetAttributeLocation(string attributeName)
        {
            return new ElementLocationR(OwningCollection.LinkFactory.GetAttributeLocation(Source, attributeName));
        }

        public string GetAttributeValue(string attributeName, bool nullIfNotExists)
        {
            return OwningCollection.LinkFactory.GetAttributeValue(Source, attributeName, nullIfNotExists);
        }

        public void SetOrRemoveAttribute(string name, string value, bool allowSettingEmptyAttributes, string reason, string param)
        {
            OwningCollection.LinkFactory.SetOrRemoveAttribute(Source, name, value, allowSettingEmptyAttributes, reason, param);
            if (reason != null)
            {
                OwningCollection.LinkFactory.MarkDirty(Source, reason, param);
            }
        }
    }

    internal class MockProjectElementLink : ProjectElementLink, ILinkMock, IProjectElementLinkHelper
    {
        object ILinkMock.Remoter => this.Proxy;
        public MockProjectElementLinkRemoter Proxy { get; }

        public MockProjectElementLink(MockProjectRootElementLinkRemoter proxy)
        {
            this.Proxy = proxy;
        }


        #region standard ProjectElementLink implementation
        IProjectElementLinkHelper EImpl => (IProjectElementLinkHelper)this;
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

    internal class MockProjectElementContainerLink : ProjectElementContainerLink, ILinkMock, IProjectElementLinkHelper, IProjectElementContainerLinkHelper
    {
        object ILinkMock.Remoter => this.Proxy;
        public MockProjectElementLinkRemoter Proxy { get; }

        public MockProjectElementContainerLink(MockProjectRootElementLinkRemoter proxy)
        {
            this.Proxy = proxy;
        }

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

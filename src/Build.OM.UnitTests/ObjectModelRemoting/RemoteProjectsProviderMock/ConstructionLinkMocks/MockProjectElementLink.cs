// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Evaluation.Context;
    using Microsoft.Build.Execution;
    using Microsoft.Build.ObjectModelRemoting;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Logging;
    using System.Diagnostics;

    class ElementLocationR : ElementLocation
    {
        public ElementLocationR(ElementLocation other)
        {
            this.File = other.File;
            this.Line = other.Line;
            this.Column = other.Column;
        }

        public override string File { get; }

        public override int Line { get; }

        public override int Column { get; }
    }


    internal interface IProjectElementRemoter
    {
        ProjectElement Import(ProjectCollectionLinker liner);
    }

    internal interface IProjectElementContainerRemoter
    {
        ProjectElementContainer Import(ProjectCollectionLinker liner);
    }

    internal struct ProjectContainerElementLinkRemoterHelper
    {
        ProjectElement Source;
        ProjectCollectionLinker Linker;

        public static IProjectElementContainerRemoter CreateForObject (ProjectElementContainer xml)
        {
        }
    }

    internal class MockProjectPropertyElementLink : ProjectPropertyElementLink, ILinkMock, IProjectElementLinkHelper
    {
        public MockProjectPropertyElementLinkRemoter Proxy { get; }
        object ILinkMock.Remoter => this.Proxy;

        public MockProjectPropertyElementLink(MockProjectPropertyElementLinkRemoter proxy)
        {
            this.Proxy = proxy;
        }


        public override string Value { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override void ChangeName(string newName)
        {
            throw new NotImplementedException();
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
    }


    internal class MockProjectPropertyElementLinkRemoter : ProjectElementLinkRemoter
    {
        ProjectPropertyElement PropertyXml => (ProjectPropertyElement)Source;

        public override ProjectElement CreateLinkedObject(ProjectCollectionLinker remote)
        {
            var link = new MockProjectPropertyElementLink(this);
            return remote.LinkFactory.Create(link);
        }
    }

    internal abstract class ProjectElementLinkRemoter : MockLinkRemoter<ProjectElement>
    {
        public static ProjectElementLinkRemoter CreateForObject(ProjectElement xml)
        {
            if (xml == null) return null;
            var 
            if ((object x = xml) != null)
            {

            }
        }

        public IProjectElementContainerRemoter Parent => ProjectContainerElementLinkRemoterHelper.CreateForObject(Source.Parent);

        public MockProjectRootElementLinkRemoter ContainingProject => throw new NotImplementedException();

        public string ElementName => Source.ElementName;

        public string OuterElement => Source.OuterElement;

        public bool ExpressedAsAttribute { get => Linker.LinkFactory.GetExpressedAsAttribute(Source); set => Linker.LinkFactory.SetExpressedAsAttribute(Source, value); }

        public IProjectElementRemoter PreviousSibling => ProjectElementLinkRemoterHelper.CreateForObject(Source.PreviousSibling);

        public IProjectElementRemoter NextSibling => ProjectElementLinkRemoterHelper.CreateForObject(Source.NextSibling);

        public ElementLocation Location => new ElementLocationR(Source.Location);

        public void CopyFrom(IProjectElementContainerRemoter element)
        {
            this.Source.CopyFrom(element.Import(Linker));
        }

        public IProjectElementRemoter CreateNewInstance(MockProjectRootElementLinkRemoter owner)
        {
            var pre = Linker.Import<ProjectRootElement, MockProjectRootElementLinkRemoter>(owner);
            return ProjectElementLinkRemoterHelper.CreateForObject(Source.CreateNewInstace(pre));
        }

        public ElementLocation GetAttributeLocation(string attributeName)
        {
            return new ElementLocationR(Linker.LinkFactory.GetAttributeLocation(Source, attributeName));
        }

        public string GetAttributeValue(string attributeName, bool nullIfNotExists)
        {
            throw new NotImplementedException();
        }

        public void SetOrRemoveAttribute(string name, string value, bool allowSettingEmptyAttributes, string reason, string param)
        {
            throw new NotImplementedException();
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

        public MockProjectElementLink(MockProjectRootElementLinkRemoter proxy)
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

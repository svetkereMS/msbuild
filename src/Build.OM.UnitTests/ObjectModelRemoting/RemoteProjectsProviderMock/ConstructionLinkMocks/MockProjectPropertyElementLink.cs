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

    internal class MockProjectPropertyElementLinkRemoter : MockProjectElementLinkRemoter
    {
        public ProjectPropertyElement PropertyXml => (ProjectPropertyElement)Source;

        public override ProjectElement Import(ProjectCollectionLinker remote)
        {
            return remote.Import<ProjectElement, MockProjectPropertyElementLinkRemoter>(this);
        }

        public override ProjectElement CreateLinkedObject(ProjectCollectionLinker remote)
        {
            var link = new MockProjectPropertyElementLink(this, remote);
            return remote.LinkFactory.Create(link);
        }

        // ProjectPropertyElementLink support
        public string Value { get => PropertyXml.Value; set => PropertyXml.Value = value; }
        public void ChangeName(string newName) { PropertyXml.Name = newName; }

    }

    internal class MockProjectPropertyElementLink : ProjectPropertyElementLink, ILinkMock, IProjectElementLinkHelper
    {
        public MockProjectPropertyElementLink(MockProjectPropertyElementLinkRemoter proxy, ProjectCollectionLinker linker)
        {
            this.Linker = linker;
            this.Proxy = proxy;
        }

        public ProjectCollectionLinker Linker { get; }
        public MockProjectPropertyElementLinkRemoter Proxy { get; }
        object ILinkMock.Remoter => this.Proxy;


        public override string Value { get => this.Proxy.Value; set => this.Proxy.Value = value; }
        public override void ChangeName(string newName)
        {
            this.Proxy.ChangeName(newName);
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


}

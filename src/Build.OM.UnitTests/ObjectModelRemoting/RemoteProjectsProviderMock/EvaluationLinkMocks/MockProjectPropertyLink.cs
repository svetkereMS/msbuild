﻿// Copyright (c) Microsoft. All rights reserved.
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

    internal class MockProjectPropertyLinkRemoter : MockLinkRemoter<ProjectProperty>
    {
        public override ProjectProperty CreateLinkedObject(ProjectCollectionLinker remote)
        {
            var link = new MockProjectPropertyLink(this, remote);
            return remote.LinkFactory.Create(link);
        }


        ///  ProjectPropertyLink remoting
        public MockProjectLinkRemoter Project => this.OwningCollection.Export<Project, MockProjectLinkRemoter>(this.Source.Project);
        public MockProjectRootElementLinkRemoter Xml => (MockProjectRootElementLinkRemoter)this.ExportElement(this.Source.Xml);
        public string Name => this.Source.Name;
        public string EvaluatedIncludeEscaped => ProjectPropertyLink.GetEvaluatedValueEscaped(this.Source);
        public string UnevaluatedValue { get => this.Source.UnevaluatedValue; set=> this.Source.UnevaluatedValue = value; }
        public bool IsEnvironmentProperty => this.Source.IsEnvironmentProperty;
        public bool IsGlobalProperty => this.Source.IsGlobalProperty;
        public bool IsReservedProperty => this.Source.IsReservedProperty;
        public MockProjectPropertyLinkRemoter Predecessor => this.Export<ProjectProperty, MockProjectPropertyLinkRemoter>(this.Source.Predecessor);
        public bool IsImported => this.Source.IsImported;
    }

    internal class MockProjectPropertyLink : ProjectPropertyLink, ILinkMock
    {
        public MockProjectPropertyLink(MockProjectPropertyLinkRemoter proxy, ProjectCollectionLinker linker)
        {
            this.Linker = linker;
            this.Proxy = proxy;
        }

        public ProjectCollectionLinker Linker { get; }
        public MockProjectPropertyLinkRemoter Proxy { get; }
        object ILinkMock.Remoter => this.Proxy;

        // ProjectPropertyLink
        public override Project Project => this.Linker.Import<Project, MockProjectLinkRemoter>(this.Proxy.Project);
        public override ProjectPropertyElement Xml => (ProjectPropertyElement)this.Proxy.Xml.Import(this.Linker);
        public override string Name => this.Proxy.Name;
        public override string EvaluatedIncludeEscaped => this.Proxy.EvaluatedIncludeEscaped;
        public override string UnevaluatedValue { get => this.Proxy.UnevaluatedValue; set => this.Proxy.UnevaluatedValue = value; }
        public override bool IsEnvironmentProperty => this.Proxy.IsEnvironmentProperty;
        public override bool IsGlobalProperty => this.Proxy.IsGlobalProperty;
        public override bool IsReservedProperty => this.Proxy.IsReservedProperty;
        public override ProjectProperty Predecessor => this.Linker.Import<ProjectProperty, MockProjectPropertyLinkRemoter>(this.Proxy.Predecessor);
        public override bool IsImported => this.Proxy.IsImported;
    }
}
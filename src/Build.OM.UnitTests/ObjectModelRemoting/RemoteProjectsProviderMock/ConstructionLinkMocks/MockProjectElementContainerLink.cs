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

    internal abstract class MockProjectElementContainerLinkRemoter : MockProjectElementLinkRemoter
    {
        public ProjectElementContainer ContainerXml => (ProjectElementContainer)Source;

        // ProjectElementContainerLink support
        public int Count => ContainerXml.Count;
        public MockProjectElementLinkRemoter FirstChild => this.Export(ContainerXml.FirstChild);
        public MockProjectElementLinkRemoter LastChild => this.Export(ContainerXml.LastChild);

        public void InsertAfterChild(MockProjectElementLinkRemoter child, MockProjectElementLinkRemoter reference)
        {
            this.ContainerXml.InsertAfterChild(child.Import(OwningCollection), reference.Import(OwningCollection));
        }

        public void InsertBeforeChild(MockProjectElementLinkRemoter child, MockProjectElementLinkRemoter reference)
        {
            this.ContainerXml.InsertBeforeChild(child.Import(OwningCollection), reference.Import(OwningCollection));
        }

        public void AddInitialChild(MockProjectElementLinkRemoter child)
        {
            this.OwningCollection.LinkFactory.AddInitialChild(this.ContainerXml, child.Import(OwningCollection));
        }

        public MockProjectElementContainerLinkRemoter DeepClone(MockProjectRootElementLinkRemoter factory, MockProjectElementContainerLinkRemoter parent)
        {
            var pre = (ProjectRootElement)factory.Import(OwningCollection);
            var pec = (ProjectElementContainer)parent.Import(OwningCollection);
            var result = this.OwningCollection.LinkFactory.DeepClone(this.ContainerXml, pre, pec);
            return (MockProjectElementContainerLinkRemoter)this.Export(result);
        }

        public void RemoveChild(MockProjectElementLinkRemoter child)
        {
            this.ContainerXml.RemoveChild(child.Import(this.OwningCollection));
        }
    }
}

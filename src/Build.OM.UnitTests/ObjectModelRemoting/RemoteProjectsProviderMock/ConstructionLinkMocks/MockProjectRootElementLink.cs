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

    internal class MockProjectRootElementLinkRemoter : MockLinkRemoter<ProjectRootElement>
    {
        public override ProjectRootElement CreateLinkedObject(ProjectCollectionLinker remote)
        {
            var link = new MockProjectRootElementLink(this);
            return remote.LinkFactory.Create(link);
        }
    }



    internal class MockProjectRootElementLink : ProjectRootElementLink, ILinkMock
    {
        object ILinkMock.Remoter => this.Proxy;
        public MockProjectRootElementLinkRemoter Proxy { get; }

        public MockProjectRootElementLink(MockProjectRootElementLinkRemoter proxy)
        {
            this.Proxy = proxy;
        }

    }
}

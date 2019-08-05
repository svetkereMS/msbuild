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
}

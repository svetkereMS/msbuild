// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Build.Collections;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Shared;
using System.Collections.Generic;
using System;
using System.Linq;


namespace Microsoft.Build.ObjectModelRemoting
{

    public abstract class ProjectItemDefinitionLink
    {
        public abstract Project Project { get; }

        public abstract string ItemType { get; }

        public abstract ICollection<ProjectMetadata> Metadata { get; }

        public abstract ProjectMetadata GetMetadata(string name);

        public abstract string GetMetadataValue(string name);

        public abstract ProjectMetadata SetMetadataValue(string name, string unevaluatedValue);
    }
}

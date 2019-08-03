// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.ObjectModelRemoting
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;

    public abstract class ProjectMetadataLink
    {
        public abstract object Parent { get; }

        public abstract ProjectMetadataElement Xml { get; }

        public abstract string EvaluatedValueEscaped { get; }

        public abstract ProjectMetadata Predecessor { get; }

        static object GetParent(ProjectMetadata metadata)
        {
            return metadata?.Parent;
        }
    }
}

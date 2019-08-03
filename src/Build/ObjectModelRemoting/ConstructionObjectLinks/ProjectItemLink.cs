// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.ObjectModelRemoting
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using System.Collections.Generic;

    public abstract class ProjectItemLink
    {
        public abstract Project Project { get; }

        public abstract ProjectItemElement Xml { get; }

        public abstract string EvaluatedInclude { get; }

        public abstract ICollection<ProjectMetadata> MetadataCollection { get; }

        public abstract ICollection<ProjectMetadata> DirectMetadata { get; }

        public abstract bool HasMetadata(string name);

        public abstract ProjectMetadata GetMetadata(string name);

        public abstract string GetMetadataValue(string name);

        public abstract ProjectMetadata SetMetadataValue(string name, string unevaluatedValue, bool propagateMetadataToSiblingItems);

        public abstract bool RemoveMetadata(string name);

        public abstract void Rename(string name);

        public abstract void ChangeItemType(string newItemType);
    }
}

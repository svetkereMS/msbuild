// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.ObjectModelRemoting
{
    public abstract class ProjectItemElementLink : ProjectElementContainerLink
    {
        public abstract void ChangeItemType(string newType);
    }
}

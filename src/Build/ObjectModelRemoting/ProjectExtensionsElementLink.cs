// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.ObjectModelRemoting
{
    public abstract class ProjectExtensionsElementLink : ProjectElementLink
    {
        public abstract string Content { get; set; }
        public abstract string GetSubElement(string name);
        public abstract void SetSubElement(string name, string value);
    }
}

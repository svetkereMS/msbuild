// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.ObjectModelRemoting
{
    public abstract class ProjectPropertyElementLink : ProjectElementLink
    {
        public abstract string Value { get; set; }
        public abstract void ChangeName(string newName);
    }
}

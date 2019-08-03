// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.ObjectModelRemoting
{
    using Microsoft.Build.Construction;

    public abstract class ProjectImportElementLink : ProjectElementLink
    {
        public abstract ImplicitImportLocation ImplicitImportLocation { get; }
        public abstract ProjectElement OriginalElement { get; }
    }
}

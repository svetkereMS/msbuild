// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.ObjectModelRemoting
{
    using Microsoft.Build.Construction;

    public abstract class ProjectElementContainerLink : ProjectElementLink
    {
        public abstract int Count { get; }
        public abstract ProjectElement FirstChild { get; }
        public abstract ProjectElement LastChild { get; }

        public abstract void InsertAfterChild(ProjectElement child, ProjectElement reference);
        public abstract void InsertBeforeChild(ProjectElement child, ProjectElement reference);

        public abstract void AddInitialChild(ProjectElement child);
        public abstract ProjectElementContainer DeepClone(ProjectRootElement factory, ProjectElementContainer parent);
        public abstract void RemoveChild(ProjectElement child);
    }

    // the "equivalence" classes in cases when we don't need additional functionality,
    // but want to allow for such to be added in the future.

    public abstract class ProjectChooseElementLink : ProjectElementContainerLink  { }

    public abstract class ProjectImportGroupElementLink : ProjectElementContainerLink { }

    public abstract class ProjectItemDefinitionElementLink : ProjectElementContainerLink { }

    public abstract class ProjectItemDefinitionGroupElementLink : ProjectElementContainerLink { }

    public abstract class ProjectItemGroupElementLink : ProjectElementContainerLink { }

    public abstract class ProjectOtherwiseElementLink : ProjectElementContainerLink { }

    public abstract class ProjectPropertyGroupElementLink : ProjectElementContainerLink { }

    public abstract class ProjectSdkElementLink : ProjectElementContainerLink { }

    public abstract class ProjectUsingTaskElementLink : ProjectElementContainerLink { }

    public abstract class ProjectWhenElementLink : ProjectElementContainerLink { }

    public abstract class UsingTaskParameterGroupElementLink : ProjectElementContainerLink { }
    
}

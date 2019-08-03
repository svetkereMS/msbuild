// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.ObjectModelRemoting
{
    using Microsoft.Build.Construction;
    using System.Xml;

    internal interface ILinkedXml
    {
        ProjectElementLink Link { get; }
        XmlElementWithLocation Xml { get; }
    }

    public abstract class ProjectElementLink : ILinkedXml
    {
        ProjectElementLink ILinkedXml.Link => this;

        XmlElementWithLocation ILinkedXml.Xml => null;

        public abstract ProjectElementContainer Parent { get; }

        public abstract ProjectRootElement ContainingProject { get; }

        public abstract string ElementName { get; }

        public abstract string OuterElement { get; }

        public abstract bool ExpressedAsAttribute { get; set; }

        public abstract ProjectElement PreviousSibling { get; }

        public abstract ProjectElement NextSibling { get; }

        public abstract ElementLocation Location { get; }

        public abstract ElementLocation GetAttributeLocation(string attributeName);

        public abstract string GetAttributeValue(string attributeName, bool nullIfNotExists);

        public abstract void SetOrRemoveAttribute(string name, string value, bool allowSettingEmptyAttributes, string reason, string param);

        public abstract void CopyFrom(ProjectElement element);

        public abstract ProjectElement CreateNewInstance(ProjectRootElement owner);
    }

    // the "equivalence" classes in cases when we don't need additional functionality,
    // but want to allow for such to be added in the future.
    public abstract class ProjectOnErrorElementLink : ProjectElementLink { }

    public abstract class ProjectOutputElementLink : ProjectElementLink { }

    public abstract class ProjectUsingTaskParameterElementLink : ProjectElementLink { }
}

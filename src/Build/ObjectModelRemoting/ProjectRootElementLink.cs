// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.ObjectModelRemoting
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;
    using Microsoft.Build.Construction;

    public abstract class ProjectRootElementLink : ProjectElementContainerLink
    {
        public abstract int Version { get; }
        public abstract DateTime TimeLastChanged { get; }
        public abstract DateTime LastWriteTimeWhenRead { get; }
        public abstract string DirectoryPath { get; }

        public abstract string FullPath { get; set; }

        public abstract ElementLocation ProjectFileLocation { get; }

        public abstract Encoding Encoding { get; }

        public abstract string RawXml { get; }

        public abstract bool PreserveFormatting { get; }

        public abstract ProjectChooseElement CreateChooseElement();
        public abstract ProjectImportElement CreateImportElement(string project);
        public abstract ProjectItemElement CreateItemElement(string itemType);
        public abstract ProjectItemElement CreateItemElement(string itemType, string include);
        public abstract ProjectItemDefinitionElement CreateItemDefinitionElement(string itemType);
        public abstract ProjectItemDefinitionGroupElement CreateItemDefinitionGroupElement();
        public abstract ProjectItemGroupElement CreateItemGroupElement();
        public abstract ProjectImportGroupElement CreateImportGroupElement();
        public abstract ProjectMetadataElement CreateMetadataElement(string name);
        public abstract ProjectMetadataElement CreateMetadataElement(string name, string unevaluatedValue);
        public abstract ProjectOnErrorElement CreateOnErrorElement(string executeTargets);
        public abstract ProjectOtherwiseElement CreateOtherwiseElement();
        public abstract ProjectOutputElement CreateOutputElement(string taskParameter, string itemType, string propertyName);
        public abstract ProjectExtensionsElement CreateProjectExtensionsElement();
        public abstract ProjectPropertyGroupElement CreatePropertyGroupElement();
        public abstract ProjectPropertyElement CreatePropertyElement(string name);
        public abstract ProjectTargetElement CreateTargetElement(string name);
        public abstract ProjectTaskElement CreateTaskElement(string name);
        public abstract ProjectUsingTaskElement CreateUsingTaskElement(string taskName, string assemblyFile, string assemblyName, string runtime, string architecture);
        public abstract UsingTaskParameterGroupElement CreateUsingTaskParameterGroupElement();
        public abstract ProjectUsingTaskParameterElement CreateUsingTaskParameterElement(string name, string output, string required, string parameterType);
        public abstract ProjectUsingTaskBodyElement CreateUsingTaskBodyElement(string evaluate, string body);
        public abstract ProjectWhenElement CreateWhenElement(string condition);
        public abstract ProjectSdkElement CreateProjectSdkElement(string sdkName, string sdkVersion);
        public abstract void Save(Encoding saveEncoding);
        public abstract void Save(TextWriter writer);

        public abstract void ReloadFrom(string path, bool throwIfUnsavedChanges, bool preserveFormatting);
        public abstract void ReloadFrom(XmlReader reader, bool throwIfUnsavedChanges, bool preserveFormatting);

        internal abstract void MarkDirty(string reason, string param);
    }
}

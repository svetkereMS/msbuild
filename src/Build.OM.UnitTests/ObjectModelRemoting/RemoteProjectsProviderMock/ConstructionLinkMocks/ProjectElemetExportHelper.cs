// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Xml;
    using Microsoft.Build.Construction;
    using Microsoft.Build.ObjectModelRemoting;


    /// <summary>
    /// We need to know the actual type of ProjectElements in order to do a proper remoting.
    /// Unless we do some explicit ProjectElement.GetXMLType() thing we need to use heuristic.
    ///
    /// Most of the types has a single implementation, but few has a wrapper classes. They are also internal for MSbuild.
    /// </summary>
    static class ProjectElemetExportHelper
    {
        delegate MockProjectElementLinkRemoter ExporterFactory(ProjectCollectionLinker exporter, ProjectElement xml);
        private class ElementInfo
        {
            public static ElementInfo New<T, RMock>()
                where RMock : MockProjectElementLinkRemoter, new()
            {
                return new ElementInfo(typeof(T), IsOfType<T>, Export<RMock>);
            }

            public ElementInfo(Type type, Func<ProjectElement, bool> checker, ExporterFactory factory)
            {
                this.CanonicalType = type;
                this.Checker = checker;
                this.ExportFactory = factory;
            }


            public ElementInfo(Func<ProjectElement, bool> checker, ExporterFactory factory)
            {
                this.Checker = checker;
                this.ExportFactory = factory;
            }

            public Type CanonicalType { get; }
            public Func<ProjectElement, bool> Checker { get; }
            public ExporterFactory ExportFactory { get; }
        }

        private static List<ElementInfo> canonicalTypes = new List<ElementInfo>()
        {
            ElementInfo.New<ProjectRootElement, MockProjectRootElementLinkRemoter>(),
            ElementInfo.New<ProjectChooseElement, MockProjectChooseElementLinkRemoter>(),
            ElementInfo.New<ProjectExtensionsElement, MockProjectExtensionsElementLinkRemoter>(),

            ElementInfo.New<ProjectImportElement, MockProjectImportElementLinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
            ElementInfo.New<., Mock.LinkRemoter>(),
        };


        private static Dictionary<Type, ElementInfo> canonicalTypeMap = new Dictionary<Type, ElementInfo>()
        {
            { typeof(ProjectImportElement),  new ElementInfo(IsOfType<ProjectImportElement>, ExportProjectImportElement) },
            { typeof(ProjectImportGroupElement),  new ElementInfo(IsOfType<ProjectImportGroupElement>, ExportProjectImportGroupElement) },
            { typeof(ProjectItemDefinitionElement),  new ElementInfo(IsOfType<ProjectItemDefinitionElement>, ExportProjectItemDefinitionElement) },
            { typeof(ProjectItemDefinitionGroupElement),  new ElementInfo(IsOfType<ProjectItemDefinitionGroupElement>, ExportProjectItemDefinitionGroupElement) },
            { typeof(ProjectItemElement),  new ElementInfo(IsOfType<ProjectItemElement>, ExportProjectItemElement) },
            { typeof(ProjectItemGroupElement),  new ElementInfo(IsOfType<ProjectItemGroupElement>, ExportProjectItemGroupElement) },
            { typeof(ProjectMetadataElement),  new ElementInfo(IsOfType<ProjectMetadataElement>, ExportProjectMetadataElement) },
            { typeof(ProjectOnErrorElement),  new ElementInfo(IsOfType<ProjectOnErrorElement>, ExportProjectOnErrorElement) },
            { typeof(ProjectOtherwiseElement),  new ElementInfo(IsOfType<ProjectOtherwiseElement>, ExportProjectOtherwiseElement) },
            { typeof(ProjectOtherwiseElement),  new ElementInfo(IsOfType<ProjectOtherwiseElement>, Export<MockProjectOtherwiseElementLinkRemoter>) },


            MockProjectOtherwiseElementLinkRemoter

            MockProjectOutputElementLinkRemoter
            MockProjectPropertyElementLinkRemoter
            MockProjectPropertyGroupElementLinkRemoter
            MockProjectSdkElementLinkRemoter
            MockProjectTargetElementLinkRemoter
            MockProjectTaskElementLinkRemoter
            MockProjectUsingTaskBodyElementLinkRemoter
            MockProjectUsingTaskElementLinkRemoter
            MockProjectUsingTaskParameterElementLinkRemoter
            MockProjectWhenElementLinkRemoter
            MockUsingTaskParameterGroupElementLinkRemoter
        };

        private static MockProjectElementLinkRemoter Export<RMock>(ProjectCollectionLinker exporter, ProjectElement xml)
            where RMock : MockProjectElementLinkRemoter, new()
        {
            return exporter.Export<ProjectElement, RMock>(xml);
        }

        private static MockProjectElementLinkRemoter ExportProjectOtherwiseElement(ProjectCollectionLinker exporter, ProjectElement xml)
        {
            throw new NotImplementedException();
        }

        private static MockProjectElementLinkRemoter ExportProjectOnErrorElement(ProjectCollectionLinker exporter, ProjectElement xml)
        {
            return exporter.Export<ProjectElement, MockProjectOnErrorElementLinkRemoter>(xml);
        }

        private static MockProjectElementLinkRemoter ExportProjectMetadataElement(ProjectCollectionLinker exporter, ProjectElement xml)
        {
            return exporter.Export<ProjectElement, MockProjectMetadataElementLinkRemoter>(xml);
        }

        private static MockProjectElementLinkRemoter ExportProjectItemGroupElement(ProjectCollectionLinker exporter, ProjectElement xml)
        {
            return exporter.Export<ProjectElement, MockProjectItemGroupElementLinkRemoter>(xml);
        }

        private static MockProjectElementLinkRemoter ExportProjectItemElement(ProjectCollectionLinker exporter, ProjectElement xml)
        {
            return exporter.Export<ProjectElement, MockProjectItemElementLinkRemoter>(xml);
        }

        private static MockProjectElementLinkRemoter ExportProjectItemDefinitionGroupElement(ProjectCollectionLinker exporter, ProjectElement xml)
        {
            return exporter.Export<ProjectElement, MockProjectItemDefinitionGroupElementLinkRemoter>(xml);
        }

        private static MockProjectElementLinkRemoter ExportProjectItemDefinitionElement(ProjectCollectionLinker exporter, ProjectElement xml)
        {
            return exporter.Export<ProjectElement, MockProjectItemDefinitionElementLinkRemoter>(xml);
        }

        private static MockProjectElementLinkRemoter ExportProjectImportGroupElement(ProjectCollectionLinker exporter, ProjectElement xml)
        {
            return exporter.Export<ProjectElement, MockProjectImportGroupElementLinkRemoter>(xml);
        }

        private static MockProjectElementLinkRemoter ExportProjectImportElement(ProjectCollectionLinker exporter, ProjectElement xml)
        {
            return exporter.Export<ProjectElement, MockProjectImportElementLinkRemoter>(xml);
        }

        private static MockProjectElementLinkRemoter ExportProjectRootElement(ProjectCollectionLinker exporter, ProjectElement xml)
        {
            return exporter.Export<ProjectElement, MockProjectRootElementLinkRemoter>(xml);
        }

        private static MockProjectElementLinkRemoter ExportProjectChooseElement(ProjectCollectionLinker exporter, ProjectElement xml)
        {
            return exporter.Export<ProjectElement, MockProjectChooseElementLinkRemoter>(xml);
        }

        private static MockProjectElementLinkRemoter ExportProjectExtensionsElement(ProjectCollectionLinker exporter, ProjectElement xml)
        {
            return exporter.Export<ProjectElement, MockProjectExtensionsElementLinkRemoter>(xml);
        }

        private static bool IsOfType<T> (ProjectElement xml) { return xml is T; }

        private static Dictionary<Type, ExporterFactory> knownTypes = new Dictionary<Type, ExporterFactory>();

        static ProjectElemetExportHelper()
        {
            foreach (var v in canonicalTypeMap)
            {
                knownTypes.Add(v.Key, v.Value.ExportFactory);
            }
        }


        private static MockProjectElementLinkRemoter NotImplemented(ProjectCollectionLinker exporter, ProjectElement xml)
        {
            throw new NotImplementedException();
        }

        public static MockProjectElementLinkRemoter Export(ProjectCollectionLinker exporter, ProjectElement xml)
        {
            var implType = xml.GetType();
            if (knownTypes.TryGetValue(implType, out var factory))
            {
                return factory(exporter, xml);
            }

            foreach (var t in canonicalTypeMap)
            {
                if (t.Value.Checker(xml))
                {
                    lock (knownTypes)
                    {
                        var newKnown = new Dictionary<Type, ExporterFactory>(knownTypes);
                        newKnown[implType] = t.Value.ExportFactory;
                        knownTypes = newKnown;
                    }
                    return t.Value.ExportFactory(exporter, xml);
                }
            }

            lock (knownTypes)
            {
                var newKnown = new Dictionary<Type, ExporterFactory>(knownTypes);
                newKnown[implType] = NotImplemented;
                knownTypes = newKnown;
            }

            return NotImplemented(exporter, xml);
        }

    }
}

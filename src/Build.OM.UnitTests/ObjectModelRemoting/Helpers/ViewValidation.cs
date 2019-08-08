// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Build.Construction;
    using Microsoft.Build.ObjectModelRemoting;
    using Microsoft.Build.Evaluation;
    using Xunit;
    using System.Runtime.ExceptionServices;
    using System.Xml.Schema;
    using System.Collections;

    internal static class ViewValidation
    {
        public static void VerifySameLocationWithException(Func<ElementLocation> expectedGetter, Func<ElementLocation> actualGetter)
        {
            Assert.Equal(GetWithExceptionCheck(expectedGetter, out var expected), GetWithExceptionCheck(actualGetter, out var actual));
            VerifySameLocation(expected, actual);
        }

        public static void VerifySameLocation(ElementLocation expected, ElementLocation actual)
        {
            if (object.ReferenceEquals(expected, actual)) return;

            Assert.NotNull(expected);
            Assert.NotNull(actual);

            Assert.Equal(expected.File, actual.File);
            Assert.Equal(expected.Line, actual.Line);
            Assert.Equal(expected.Column, actual.Column);
        }

        public static bool IsLinkedObject(object obj)
        {
            return LinkedObjectsFactory.GetLink(obj) != null;
        }

        private static bool dbgIgnoreLinked = false; 
        public static void VerifyNotLinked(object obj)
        {
            if (dbgIgnoreLinked) return;
            Assert.True(obj == null || !IsLinkedObject(obj));
        }

        public static void VerifyLinked(object obj)
        {
            if (dbgIgnoreLinked) return;
            Assert.True(obj == null || IsLinkedObject(obj));
        }

        public static void VerifyNotLinkedNotNull(object obj)
        {
            Assert.NotNull(obj);
            if (dbgIgnoreLinked) return;
            Assert.True(!IsLinkedObject(obj));
        }

        public static void VerifyLinkedNotNull(object obj)
        {
            Assert.NotNull(obj);
            if (dbgIgnoreLinked) return;
            Assert.True(IsLinkedObject(obj));
        }

        public static bool GetWithExceptionCheck<T>(Func<T> getter, out T result)
        {
            try
            {
                result = getter();
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }


        /* 
            public abstract class ProjectElement : IProjectElement
            {
                public string OuterElement { get; }
                public ElementLocation LabelLocation { get; }
                public virtual ElementLocation ConditionLocation { get; }
                public ProjectRootElement ContainingProject { get; }
                public ProjectElement NextSibling { get; }
                public ProjectElement PreviousSibling { get; }
                public IEnumerable<ProjectElementContainer> AllParents { get; }
                public ElementLocation Location { get; }
                public string ElementName { get; }
                public string Label { get; set; }
                public virtual string Condition { get; set; }
                public ProjectElementContainer Parent { get; }
                public ProjectElement Clone();
                public virtual void CopyFrom(ProjectElement element);
            }
        */

        public static void ValidateEqualWithException<T>(Func<T> viewGetter, Func<T> realGetter)
        {
            bool viewOk = GetWithExceptionCheck(viewGetter, out T viewValue);
            bool realOk = GetWithExceptionCheck(realGetter, out T realValue);
            Assert.Equal(realOk, viewOk);
            Assert.Equal(realValue, viewValue);
        }


        private static void VerifyProjectElementViewInternal(ProjectElement viewXml, ProjectElement realXml)
        {
            if (viewXml == null && realXml == null) return;

            VerifyLinkedNotNull(viewXml);
            VerifyNotLinkedNotNull(realXml);

            Assert.Equal(realXml.OuterElement, viewXml.OuterElement);

            VerifySameLocation(realXml.LabelLocation, viewXml.LabelLocation);

            VerifySameLocationWithException(()=>realXml.ConditionLocation, ()=>viewXml.ConditionLocation);

            VerifyNotLinked(realXml.ContainingProject);
            VerifyLinked(viewXml.ContainingProject);

            VerifyNotLinked(realXml.NextSibling);
            VerifyLinked(viewXml.NextSibling);

            VerifyNotLinked(realXml.PreviousSibling);
            VerifyLinked(viewXml.PreviousSibling);

            // skip AllParents, parent validation should cover it.
            VerifySameLocation(realXml.Location, viewXml.Location);

            Assert.Equal(realXml.ElementName, viewXml.ElementName);
            Assert.Equal(realXml.Label, viewXml.Label);

            ValidateEqualWithException(() => viewXml.Condition, () => realXml.Condition);

            VerifyNotLinked(realXml.Parent);
            VerifyLinked(viewXml.Parent);
        }

        public static void VerifyProjectElementView(ProjectElement viewXml, ProjectElement realXml, bool recursive)
        {
            if (viewXml is ProjectElementContainer viewContainer)
            {
                Assert.True(realXml is ProjectElementContainer);
                VerifyProjectElementContainerView(viewContainer, (ProjectElementContainer)realXml, recursive);
            }
            else
            {
                Assert.False(realXml is ProjectElementContainer);
                VerifyProjectElementViewInternal(viewXml, realXml);
            }
        }


        /*
        public abstract class ProjectElementContainer : ProjectElement
        {
            public IEnumerable<ProjectElement> AllChildren { get; }
            public ICollection<ProjectElement> Children { get; }
            public ICollection<ProjectElement> ChildrenReversed { get; }
            public int Count { get; }
            public ProjectElement FirstChild { get; }
            public ProjectElement LastChild { get; }
            public void AppendChild(ProjectElement child);
            public virtual void DeepCopyFrom(ProjectElementContainer element);
            public void InsertAfterChild(ProjectElement child, ProjectElement reference);
            public void InsertBeforeChild(ProjectElement child, ProjectElement reference);
            public void PrependChild(ProjectElement child);
            public void RemoveAllChildren();
            public void RemoveChild(ProjectElement child);
        }
        */

        public static void VerifyProjectElementContainerView(ProjectElementContainer viewXml, ProjectElementContainer realXml, bool recursive)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElementViewInternal(viewXml, realXml);

            Assert.Equal(realXml.Count, viewXml.Count);

            VerifyNotLinked(realXml.FirstChild);
            VerifyLinked(viewXml.FirstChild);

            VerifyNotLinked(realXml.LastChild);
            VerifyLinked(viewXml.LastChild);

            if (recursive)
            {
                var realChild = realXml.FirstChild;
                var viewChild = viewXml.FirstChild;

                while (realChild != null )
                {
                    Assert.NotNull(viewChild);
                    Assert.Same(realChild.Parent, realXml);
                    Assert.Same(viewChild.Parent, viewXml);

                    if (realChild is ProjectElementContainer realChildContainer)
                    {
                        Assert.True(viewChild is ProjectElementContainer);

                        VerifyProjectElementContainerView((ProjectElementContainer)viewChild, realChildContainer, true);
                    }
                    else
                    {
                        Assert.False(viewChild is ProjectElementContainer);
                        VerifyProjectElementViewInternal(viewChild, realChild);
                    }

                    realChild = realChild.NextSibling;
                    viewChild = viewChild.NextSibling;
                }

                Assert.Null(viewChild);
            }
        }



        public static void VerifyProjectCollectionLinks(this ProjectCollectionLinker linker, string path, int expectedLocal, int expectedLinks)
            => VerifyProjectCollectionLinks(linker.Collection, path, expectedLocal, expectedLinks);

        public static void VerifyProjectCollectionLinks(this ProjectCollection collection, string path, int expectedLocal, int expectedLinks)
            => VerifyProjectCollectionLinks(collection.GetLoadedProjects(path), expectedLocal, expectedLinks);

        public static void VerifyProjectCollectionLinks(IEnumerable<Project> projects, int expectedLocal, int expectedLinks)
        {
            HashSet<Project> remotes = new HashSet<Project>();
            int actualLocal = 0;
            int actualLinks = 0;
            foreach (var prj in projects)
            {
                Assert.NotNull(prj);
                if (IsLinkedObject(prj))
                {
                    Assert.DoesNotContain(prj, remotes);
                    actualLinks++;
                    remotes.Add(prj);
                }
                else
                {
                    actualLocal++;
                }
            }

            Assert.Equal(expectedLocal, actualLocal);
            Assert.Equal(expectedLinks, actualLinks);
        }


        internal static void Verify(ProjectMetadataElement viewXml, ProjectMetadataElement realXml)
        {
            VerifyProjectElementView(viewXml, realXml, true);

            Assert.Equal(realXml.Name, viewXml.Name);
            Assert.Equal(realXml.Value, viewXml.Value);
            Assert.Equal(realXml.ExpressedAsAttribute, viewXml.ExpressedAsAttribute);
        }

        internal static void Verify(ProjectOutputElement viewXml, ProjectOutputElement realXml)
        {
            VerifyProjectElementView(viewXml, realXml, true);

            Assert.Equal(realXml.TaskParameter, viewXml.TaskParameter);
            VerifySameLocation(realXml.TaskParameterLocation, viewXml.TaskParameterLocation);
            Assert.Equal(realXml.IsOutputItem, viewXml.IsOutputItem);
            Assert.Equal(realXml.IsOutputProperty, viewXml.IsOutputProperty);
            Assert.Equal(realXml.ItemType, viewXml.ItemType);
            Assert.Equal(realXml.PropertyName, viewXml.PropertyName);
            VerifySameLocation(realXml.PropertyNameLocation, viewXml.PropertyNameLocation);
        }


        internal static void Verify<T>(IEnumerable<T> viewXmlCollection, IEnumerable<T> realXmlCollection, Action<T, T> elementValidator)
        {
            var viewXmlList = viewXmlCollection.ToList();
            var realXmlList = realXmlCollection.ToList();
            Assert.Equal(realXmlList.Count, viewXmlList.Count);
            for (int i = 0; i < realXmlList.Count; i++)
            {
                elementValidator(viewXmlList[i], realXmlList[i]);
            }
        }
    }
}

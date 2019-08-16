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

    internal class ElementLinkPair<T> : LinkPair<T>
        where T : ProjectElement
    {
        public ElementLinkPair(T view, T real) : base(view, real) { }
    }

    internal class ProjectXmlPair : ElementLinkPair<ProjectRootElement>
    {
        public ProjectXmlPair(ProjectPair pair) : base(pair.View.Xml, pair.Real.Xml) { }
        public ProjectXmlPair(ProjectRootElement viewXml, ProjectRootElement realXml) : base(viewXml, realXml) { }

        public ElementLinkPair<T> QuerySingleChildrenWithValidation<T>(Func<T, bool> matcher)
            where T : ProjectElement
        {
            var result = QueryChildrenWithValidation(matcher);
            Assert.Equal(1, result.Count);
            return result.FirstOrDefault();
        }

        public ICollection<ElementLinkPair<T>> QueryChildrenWithValidation<T>(Func<T, bool> matcher, int expectedCount)
            where T : ProjectElement
        {
            var result = QueryChildrenWithValidation(matcher);
            Assert.Equal(expectedCount, result.Count);
            return result;
        }

        public ICollection<ElementLinkPair<T>> QueryChildrenWithValidation<T>(Func<T, bool> matcher)
            where T : ProjectElement
        {
            var viewResult = new List<T>();
            var realResult = new List<T>();
            var finalResult = new List<ElementLinkPair<T>>();

            foreach ( var v in View.AllChildren)
            {
                if (v is T vt)
                {
                    if (matcher(vt))
                    {
                        viewResult.Add(vt);
                    }
                }
            }

            foreach (var r in Real.AllChildren)
            {
                if (r is T rt)
                {
                    if (matcher(rt))
                    {
                        realResult.Add(rt);
                    }
                }
            }
            // slow form viw VerifyFindType, since we dont know the T.
            ViewValidation.Verify(viewResult, realResult);

            for (int i = 0; i < viewResult.Count; i++)
            {
                finalResult.Add(new ElementLinkPair<T>(viewResult[i], realResult[i]));
            }

            return finalResult;
        }
    }


    internal static partial class ViewValidation
    {
        public static string Ver(this string str, int ver) => $"{str}{ver}";

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


        private static void VerifyProjectElementContainerView(ProjectElementContainer viewXml, ProjectElementContainer realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElementViewInternal(viewXml, realXml);

            Assert.Equal(realXml.Count, viewXml.Count);

            VerifyNotLinked(realXml.FirstChild);
            VerifyLinked(viewXml.FirstChild);

            VerifyNotLinked(realXml.LastChild);
            VerifyLinked(viewXml.LastChild);

            var realChild = realXml.FirstChild;
            var viewChild = viewXml.FirstChild;

            while (realChild != null )
            {
                Assert.NotNull(viewChild);
                Assert.Same(realChild.Parent, realXml);

                if (!object.ReferenceEquals(viewChild.Parent, viewXml))
                {
                    var lm = LinkedObjectsFactory.GetLink(viewXml) as ILinkMock;
                    lm.Linker.ValidateNoDuplocates();
                }

                Assert.Same(viewChild.Parent, viewXml);

                if (realChild is ProjectElementContainer realChildContainer)
                {
                    Assert.True(viewChild is ProjectElementContainer);

                    VerifyProjectElementContainerView((ProjectElementContainer)viewChild, realChildContainer);
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


        public static void VerifyProjectElement(ProjectElement viewXml, ProjectElement realXml)
        {
            if (viewXml == null && realXml == null) return;

            if (viewXml is ProjectElementContainer viewContainer)
            {
                Assert.True(realXml is ProjectElementContainer);
                VerifyProjectElementContainerView(viewContainer, (ProjectElementContainer)realXml);
            }
            else
            {
                Assert.False(realXml is ProjectElementContainer);
                VerifyProjectElementViewInternal(viewXml, realXml);
            }
        }

        public static void Verify(ProjectRootElement viewXml, ProjectRootElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);

            Assert.Equal(realXml.FullPath, viewXml.FullPath);
            Assert.Equal(realXml.DirectoryPath, viewXml.DirectoryPath);
            Assert.Equal(realXml.Encoding, viewXml.Encoding);
            Assert.Equal(realXml.DefaultTargets, viewXml.DefaultTargets);
            Assert.Equal(realXml.InitialTargets, viewXml.InitialTargets);
            Assert.Equal(realXml.Sdk, viewXml.Sdk);
            Assert.Equal(realXml.TreatAsLocalProperty, viewXml.TreatAsLocalProperty);
            Assert.Equal(realXml.ToolsVersion, viewXml.ToolsVersion);
            Assert.Equal(realXml.HasUnsavedChanges, viewXml.HasUnsavedChanges);
            Assert.Equal(realXml.PreserveFormatting, viewXml.PreserveFormatting);
            Assert.Equal(realXml.Version, viewXml.Version);
            Assert.Equal(realXml.TimeLastChanged, viewXml.TimeLastChanged);
            Assert.Equal(realXml.LastWriteTimeWhenRead, viewXml.LastWriteTimeWhenRead);

            ViewValidation.VerifySameLocation(realXml.ProjectFileLocation, viewXml.ProjectFileLocation);
            ViewValidation.VerifySameLocation(realXml.ToolsVersionLocation, viewXml.ToolsVersionLocation);
            ViewValidation.VerifySameLocation(realXml.DefaultTargetsLocation, viewXml.DefaultTargetsLocation);
            ViewValidation.VerifySameLocation(realXml.InitialTargetsLocation, viewXml.InitialTargetsLocation);
            ViewValidation.VerifySameLocation(realXml.SdkLocation, viewXml.SdkLocation);
            ViewValidation.VerifySameLocation(realXml.TreatAsLocalPropertyLocation, viewXml.TreatAsLocalPropertyLocation);

            ViewValidation.Verify(viewXml.ChooseElements, realXml.ChooseElements, Verify);
            ViewValidation.Verify(viewXml.ItemDefinitionGroups, realXml.ItemDefinitionGroups, Verify);
            ViewValidation.Verify(viewXml.ItemDefinitions, realXml.ItemDefinitions, Verify);
            ViewValidation.Verify(viewXml.ItemGroups, realXml.ItemGroups, Verify);
            ViewValidation.Verify(viewXml.Items, realXml.Items, Verify);
            ViewValidation.Verify(viewXml.ImportGroups, realXml.ImportGroups, Verify);
            ViewValidation.Verify(viewXml.Imports, realXml.Imports, Verify);
            ViewValidation.Verify(viewXml.PropertyGroups, realXml.PropertyGroups, Verify);
            ViewValidation.Verify(viewXml.Properties, realXml.Properties, Verify);
            ViewValidation.Verify(viewXml.Targets, realXml.Targets, Verify);
            ViewValidation.Verify(viewXml.UsingTasks, realXml.UsingTasks, Verify);
            ViewValidation.Verify(viewXml.ItemGroupsReversed, realXml.ItemGroupsReversed, Verify);
            ViewValidation.Verify(viewXml.ItemDefinitionGroupsReversed, realXml.ItemDefinitionGroupsReversed, Verify);
            ViewValidation.Verify(viewXml.ImportGroupsReversed, realXml.ImportGroupsReversed, Verify);
            ViewValidation.Verify(viewXml.PropertyGroupsReversed, realXml.PropertyGroupsReversed, Verify);
        }

        public static void Verify(ProjectChooseElement viewXml, ProjectChooseElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);


            Verify(viewXml.WhenElements, realXml.WhenElements, Verify);
            Verify(viewXml.OtherwiseElement, realXml.OtherwiseElement);
        }

        public static void Verify(ProjectWhenElement viewXml, ProjectWhenElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);


            Verify(viewXml.ChooseElements, realXml.ChooseElements);
            Verify(viewXml.ItemGroups, realXml.ItemGroups);
            Verify(viewXml.PropertyGroups, realXml.PropertyGroups);
        }

        public static void Verify(ProjectOtherwiseElement viewXml, ProjectOtherwiseElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);


            Verify(viewXml.ChooseElements, realXml.ChooseElements);
            Verify(viewXml.ItemGroups, realXml.ItemGroups);
            Verify(viewXml.PropertyGroups, realXml.PropertyGroups);
        }

        public static void Verify(ProjectExtensionsElement viewXml, ProjectExtensionsElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);

            Assert.Equal(realXml.Content, viewXml.Content);

            Assert.Equal(realXml["a"], viewXml["a"]);
            Assert.Equal(realXml["b"], viewXml["b"]);
            Assert.Equal("x", viewXml["a"]);
            Assert.Equal("y", viewXml["b"]);
        }

        public static void Verify(ProjectMetadataElement viewXml, ProjectMetadataElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);

            Assert.Equal(realXml.Name, viewXml.Name);
            Assert.Equal(realXml.Value, viewXml.Value);
            Assert.Equal(realXml.ExpressedAsAttribute, viewXml.ExpressedAsAttribute);
        }

        public static void Verify(ProjectTaskElement viewXml, ProjectTaskElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);

            Assert.Equal(realXml.Name, viewXml.Name);

            Assert.Equal(realXml.ContinueOnError, viewXml.ContinueOnError);
            ViewValidation.VerifySameLocation(realXml.ContinueOnErrorLocation, viewXml.ContinueOnErrorLocation);
            Assert.Equal(realXml.MSBuildRuntime, viewXml.MSBuildRuntime);
            ViewValidation.VerifySameLocation(realXml.MSBuildRuntimeLocation, viewXml.MSBuildRuntimeLocation);

            Assert.Equal(realXml.MSBuildArchitecture, viewXml.MSBuildArchitecture);
            ViewValidation.VerifySameLocation(realXml.MSBuildArchitectureLocation, viewXml.MSBuildArchitectureLocation);

            ViewValidation.Verify(viewXml.Outputs, realXml.Outputs, ViewValidation.Verify);

            var realParams = realXml.Parameters;
            var viewParams = viewXml.Parameters;
            if (realParams == null)
            {
                Assert.Null(viewParams);
            }
            else
            {
                Assert.NotNull(viewParams);

                Assert.Equal(realParams.Count, viewParams.Count);
                foreach (var k in realParams.Keys)
                {
                    Assert.True(viewParams.ContainsKey(k));
                    Assert.Equal(realParams[k], viewParams[k]);
                }
            }

            var realParamsLoc = realXml.ParameterLocations;
            var viewParamsLoc = viewXml.ParameterLocations;
            if (realParamsLoc == null)
            {
                Assert.Null(viewParamsLoc);
            }
            else
            {
                Assert.NotNull(viewParamsLoc);

                var realPLocList = realParamsLoc.ToList();
                var viewPLocList = viewParamsLoc.ToList();

                Assert.Equal(realPLocList.Count, viewPLocList.Count);
                for (int li = 0; li < realPLocList.Count; li++)
                {
                    var rkvp = realPLocList[li];
                    var vkvp = viewPLocList[li];

                    Assert.Equal(rkvp.Key, vkvp.Key);
                    ViewValidation.VerifySameLocation(rkvp.Value, vkvp.Value);
                }
            }
        }

        public static void Verify(ProjectOutputElement viewXml, ProjectOutputElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);

            Assert.Equal(realXml.TaskParameter, viewXml.TaskParameter);
            VerifySameLocation(realXml.TaskParameterLocation, viewXml.TaskParameterLocation);
            Assert.Equal(realXml.IsOutputItem, viewXml.IsOutputItem);
            Assert.Equal(realXml.IsOutputProperty, viewXml.IsOutputProperty);
            Assert.Equal(realXml.ItemType, viewXml.ItemType);
            Assert.Equal(realXml.PropertyName, viewXml.PropertyName);
            VerifySameLocation(realXml.PropertyNameLocation, viewXml.PropertyNameLocation);
        }

        public static void Verify(ProjectUsingTaskBodyElement viewXml, ProjectUsingTaskBodyElement realXml)
        {
            if (viewXml == null && realXml == null) return;

            VerifyProjectElement(viewXml, realXml);

            Assert.Equal(realXml.TaskBody, viewXml.TaskBody);
            Assert.Equal(realXml.Evaluate, viewXml.Evaluate);
            VerifySameLocation(realXml.EvaluateLocation, viewXml.EvaluateLocation);
        }

        public static void Verify(ProjectUsingTaskParameterElement viewXml, ProjectUsingTaskParameterElement realXml)
        {
            if (viewXml == null && realXml == null) return;

            VerifyProjectElement(viewXml, realXml);

            Assert.Equal(realXml.Name, viewXml.Name);
            Assert.Equal(realXml.ParameterType, viewXml.ParameterType);
            VerifySameLocation(realXml.ParameterTypeLocation, viewXml.ParameterTypeLocation);
            Assert.Equal(realXml.Output, viewXml.Output);
            VerifySameLocation(realXml.OutputLocation, viewXml.OutputLocation);
            Assert.Equal(realXml.Required, viewXml.Required);
            VerifySameLocation(realXml.RequiredLocation, viewXml.RequiredLocation);
        }

        public static void Verify(UsingTaskParameterGroupElement viewXml, UsingTaskParameterGroupElement realXml)
        {
            if (viewXml == null && realXml == null) return;

            VerifyProjectElement(viewXml, realXml);

            Verify(viewXml.Parameters, realXml.Parameters, Verify);
        }

        public static void Verify(ProjectUsingTaskElement viewXml, ProjectUsingTaskElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);


            Assert.Equal(realXml.AssemblyFile, viewXml.AssemblyFile);
            ViewValidation.VerifySameLocation(realXml.AssemblyFileLocation, viewXml.AssemblyFileLocation);

            Assert.Equal(realXml.AssemblyName, viewXml.AssemblyName);
            ViewValidation.VerifySameLocation(realXml.AssemblyNameLocation, viewXml.AssemblyNameLocation);

            Assert.Equal(realXml.TaskName, viewXml.TaskName);
            ViewValidation.VerifySameLocation(realXml.TaskNameLocation, viewXml.TaskNameLocation);

            Assert.Equal(realXml.TaskFactory, viewXml.TaskFactory);
            ViewValidation.VerifySameLocation(realXml.TaskFactoryLocation, viewXml.TaskFactoryLocation);

            Assert.Equal(realXml.Runtime, viewXml.Runtime);
            ViewValidation.VerifySameLocation(realXml.RuntimeLocation, viewXml.RuntimeLocation);

            Assert.Equal(realXml.Architecture, viewXml.Architecture);
            ViewValidation.VerifySameLocation(realXml.ArchitectureLocation, viewXml.ArchitectureLocation);

            ViewValidation.Verify(viewXml.TaskBody, realXml.TaskBody);
            ViewValidation.Verify(viewXml.ParameterGroup, realXml.ParameterGroup);
        }

        public static void Verify(ProjectTargetElement viewXml, ProjectTargetElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);


            Assert.Equal(realXml.Name, viewXml.Name);
            ViewValidation.VerifySameLocation(realXml.NameLocation, viewXml.NameLocation);
            Assert.Equal(realXml.Inputs, viewXml.Inputs);
            ViewValidation.VerifySameLocation(realXml.InputsLocation, viewXml.InputsLocation);
            Assert.Equal(realXml.Outputs, viewXml.Outputs);
            ViewValidation.VerifySameLocation(realXml.OutputsLocation, viewXml.OutputsLocation);
            Assert.Equal(realXml.KeepDuplicateOutputs, viewXml.KeepDuplicateOutputs);
            ViewValidation.VerifySameLocation(realXml.KeepDuplicateOutputsLocation, viewXml.KeepDuplicateOutputsLocation);
            Assert.Equal(realXml.DependsOnTargets, viewXml.DependsOnTargets);
            ViewValidation.VerifySameLocation(realXml.DependsOnTargetsLocation, viewXml.DependsOnTargetsLocation);
            Assert.Equal(realXml.BeforeTargets, viewXml.BeforeTargets);
            ViewValidation.VerifySameLocation(realXml.BeforeTargetsLocation, viewXml.BeforeTargetsLocation);
            Assert.Equal(realXml.AfterTargets, viewXml.AfterTargets);
            ViewValidation.VerifySameLocation(realXml.AfterTargetsLocation, viewXml.AfterTargetsLocation);
            Assert.Equal(realXml.Returns, viewXml.Returns);
            ViewValidation.VerifySameLocation(realXml.ReturnsLocation, viewXml.ReturnsLocation);

            ViewValidation.Verify(viewXml.ItemGroups, realXml.ItemGroups, ViewValidation.Verify);
            ViewValidation.Verify(viewXml.PropertyGroups, realXml.PropertyGroups, ViewValidation.Verify);
            ViewValidation.Verify(viewXml.OnErrors, realXml.OnErrors, ViewValidation.Verify);
            ViewValidation.Verify(viewXml.Tasks, realXml.Tasks, ViewValidation.Verify);
        }

        public static void Verify(ProjectImportElement viewXml, ProjectImportElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);


            Assert.Equal(realXml.Project, viewXml.Project);
            ViewValidation.VerifySameLocation(realXml.ProjectLocation, viewXml.ProjectLocation);

            // mostly test the remoting infrastructure. Sdk Imports are not really covered by simple samples for now.
            // Todo: add mock SDK import closure to SdtGroup?
            Assert.Equal(realXml.Sdk, viewXml.Sdk);
            Assert.Equal(realXml.Version, viewXml.Version);
            Assert.Equal(realXml.MinimumVersion, viewXml.MinimumVersion);
            ViewValidation.VerifySameLocation(realXml.SdkLocation, viewXml.SdkLocation);
            Assert.Equal(realXml.ImplicitImportLocation, viewXml.ImplicitImportLocation);
            ViewValidation.VerifyProjectElement(viewXml.OriginalElement, realXml.OriginalElement);
        }

        public static void Verify(ProjectImportGroupElement viewXml, ProjectImportGroupElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);

            ViewValidation.Verify(viewXml.Imports, realXml.Imports, ViewValidation.Verify);
        }

        public static void Verify(ProjectItemDefinitionElement viewXml, ProjectItemDefinitionElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);

            Assert.Equal(realXml.ItemType, viewXml.ItemType);
            ViewValidation.Verify(viewXml.Metadata, realXml.Metadata, ViewValidation.Verify);
        }

        public static void Verify(ProjectItemDefinitionGroupElement viewXml, ProjectItemDefinitionGroupElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);

            ViewValidation.Verify(viewXml.ItemDefinitions, realXml.ItemDefinitions, ViewValidation.Verify);
        }

        public static void Verify(ProjectItemElement viewXml, ProjectItemElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);

            Assert.Equal(realXml.ItemType, viewXml.ItemType);
            Assert.Equal(realXml.Include, viewXml.Include);
            Assert.Equal(realXml.Exclude, viewXml.Exclude);
            Assert.Equal(realXml.Remove, viewXml.Remove);
            Assert.Equal(realXml.Update, viewXml.Update);
            Assert.Equal(realXml.KeepMetadata, viewXml.KeepMetadata);
            Assert.Equal(realXml.RemoveMetadata, viewXml.RemoveMetadata);
            Assert.Equal(realXml.KeepDuplicates, viewXml.KeepDuplicates);
            Assert.Equal(realXml.HasMetadata, viewXml.HasMetadata);

           Verify(viewXml.Metadata, realXml.Metadata, ViewValidation.Verify);

           VerifySameLocation(realXml.IncludeLocation, viewXml.IncludeLocation);
           VerifySameLocation(realXml.ExcludeLocation, viewXml.ExcludeLocation);
           VerifySameLocation(realXml.RemoveLocation, viewXml.RemoveLocation);
           VerifySameLocation(realXml.UpdateLocation, viewXml.UpdateLocation);
           VerifySameLocation(realXml.KeepMetadataLocation, viewXml.KeepMetadataLocation);
           VerifySameLocation(realXml.RemoveMetadataLocation, viewXml.RemoveMetadataLocation);
           VerifySameLocation(realXml.KeepDuplicatesLocation, viewXml.KeepDuplicatesLocation);
        }

        public static void Verify(ProjectItemGroupElement viewXml, ProjectItemGroupElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);

            Verify(viewXml.Items, realXml.Items, Verify);
        }

        public static void Verify(ProjectPropertyElement viewXml, ProjectPropertyElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);

            Assert.Equal(realXml.Name, viewXml.Name);
            Assert.Equal(realXml.Value, viewXml.Value);
        }

        public static void Verify(ProjectPropertyGroupElement viewXml, ProjectPropertyGroupElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);
            Verify(viewXml.Properties, realXml.Properties, Verify);
            Verify(viewXml.PropertiesReversed, realXml.PropertiesReversed, Verify);
        }

        public static void Verify(ProjectSdkElement viewXml, ProjectSdkElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);
            Assert.Equal(realXml.Name, viewXml.Name);
            Assert.Equal(realXml.Version, viewXml.Version);
            Assert.Equal(realXml.MinimumVersion, viewXml.MinimumVersion);
        }

        public static void Verify(ProjectOnErrorElement viewXml, ProjectOnErrorElement realXml)
        {
            if (viewXml == null && realXml == null) return;
            VerifyProjectElement(viewXml, realXml);

            Assert.Equal(realXml.ExecuteTargetsAttribute, viewXml.ExecuteTargetsAttribute);
            VerifySameLocation(realXml.ExecuteTargetsLocation, viewXml.ExecuteTargetsLocation);
        }


        public static void Verify<T>(IEnumerable<T> viewXmlCollection, IEnumerable<T> realXmlCollection)
            where T : ProjectElement
        {
            var viewXmlList = viewXmlCollection.ToList();
            var realXmlList = realXmlCollection.ToList();
            Assert.Equal(realXmlList.Count, viewXmlList.Count);
            for (int i = 0; i < realXmlList.Count; i++)
            {
                VerifyFindType(viewXmlList[i], realXmlList[i]);
            }
        }

        public static void Verify<T>(IEnumerable<T> viewXmlCollection, IEnumerable<T> realXmlCollection, Action<T, T> elementValidator)
            where T : ProjectElement
        {
            if (viewXmlCollection == null && realXmlCollection == null) return;
            Assert.NotNull(viewXmlCollection);
            Assert.NotNull(realXmlCollection);

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

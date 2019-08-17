// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Tasks;
    using Xunit;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public class LinkedConstructionModify_Tests : IClassFixture<LinkedConstructionModify_Tests.MyTestCollectionGroup>
    {
        public class MyTestCollectionGroup : TestCollectionGroup
        {
            public string LocalBigPath { get; }
            public string TargetBigPath { get; }
            public string GuestBigPath { get; }

            public Project LocalBig { get; }
            public Project TargetBig { get; }
            public Project GuestBig { get; }

            internal ProjectCollectionLinker Target { get; }
            internal ProjectCollectionLinker Guest { get; }

            public MyTestCollectionGroup()
                : base(2, 0)
            {
                this.LocalBigPath = this.ImmutableDisk.WriteProjectFile($"BigLocal.proj", TestCollectionGroup.BigProjectFile);
                this.TargetBigPath = this.ImmutableDisk.WriteProjectFile($"BigTarget.proj", TestCollectionGroup.BigProjectFile);
                this.GuestBigPath = this.ImmutableDisk.WriteProjectFile($"BigGuest.proj", TestCollectionGroup.BigProjectFile);

                this.Target = this.Remote[0];
                this.Guest = this.Remote[1];

                this.LocalBig = this.Local.LoadProjectIgnoreMissingImports(this.LocalBigPath);
                this.TargetBig = this.Target.LoadProjectIgnoreMissingImports(this.TargetBigPath);
                this.GuestBig = this.Guest.LoadProjectIgnoreMissingImports(this.GuestBigPath);

                this.TakeSnaphot();
                this.Local.Importing = true;
            }
        }

        public MyTestCollectionGroup StdGroup { get; }
        public LinkedConstructionModify_Tests(MyTestCollectionGroup group)
        {

            this.StdGroup = group;
            group.Clear();
            this.StdGroup.Local.Importing = true;
        }

        private ProjectPair GetNewInMemoryProject(string path)
        {
            var tempPath = this.StdGroup.Disk.GetAbsolutePath(path);
            var newReal = this.StdGroup.Target.LoadInMemoryWithSettings(TestCollectionGroup.SampleProjectFile);
            newReal.Xml.FullPath = tempPath;
            var newView = this.StdGroup.Local.GetLoadedProjects(tempPath).FirstOrDefault();
            Assert.NotNull(newView);

            ViewValidation.Verify(newView, newReal);

            return new ProjectPair(newView, newReal);
        }

        [Fact]
        public void ProjectRootElementModify()
        {
            var pair = GetNewInMemoryProject("temp.prj");
            var xmlPair = new ProjectXmlPair(pair);

            xmlPair.VerifySetter(this.StdGroup.Disk.GetAbsolutePath("tempRenamed"), (p) => p.FullPath, (p, v) => p.FullPath = v);
            xmlPair.VerifySetter("build", (p) => p.DefaultTargets, (p, v) => p.DefaultTargets = v);
            xmlPair.VerifySetter("init", (p) => p.InitialTargets, (p, v) => p.InitialTargets = v);
            xmlPair.VerifySetter("YetAnotherSDK", (p) => p.Sdk, (p, v) => p.Sdk = v);
            xmlPair.VerifySetter("NonLocalProp", (p) => p.TreatAsLocalProperty, (p, v) => p.TreatAsLocalProperty = v);
            xmlPair.VerifySetter("xmakever", (p) => p.ToolsVersion, (p, v) => p.ToolsVersion = v);

            var newImport = this.StdGroup.Disk.GetAbsolutePath("import");
            xmlPair.Add2NewChildrenWithVerify<ProjectImportElement>(newImport, (p, i) => p.AddImport(i), (pi, i) => pi.Project == i, out var import1, out var import2);
            xmlPair.Add2NewLabaledChildrenWithVerify<ProjectImportGroupElement>("ImportGroupLabel", (p) => p.AddImportGroup(), out var importGroup1, out var importGroup2);

            var newItem = this.StdGroup.Disk.GetAbsolutePath("newfile.cpp");
            xmlPair.Add2NewChildrenWithVerify<ProjectItemElement>(newItem, (p, i) => p.AddItem("cpp", i), (pi, i) => pi.Include == i, out var item1, out var item2);
            var newItemWithMetadata = this.StdGroup.Disk.GetAbsolutePath("newfile2.cpp");
            List<KeyValuePair<string, string>> itemMetadata = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("m1", "v1"),
                new KeyValuePair<string, string>("m2", "v2"),
                new KeyValuePair<string, string>("m3", "v3"),
            };

            xmlPair.Add2NewChildrenWithVerify<ProjectItemElement>(newItemWithMetadata, (p, i) => p.AddItem("cpp", i, itemMetadata), (pi, i) => pi.Include == i, out var itemWithMetadata1, out var itemWithMetadata2);



            // funny API (not that anyone will use it).
            var clone = xmlPair.View.DeepClone();
            ViewValidation.IsLinkedObject(clone);
            Assert.NotSame(clone, xmlPair.View);
            Assert.True(string.IsNullOrEmpty(clone.FullPath));
        }


        [Fact]
        public void ProjectTargetElementModify()
        {
            var pair = GetNewInMemoryProject("temp.prj");
            var xmlPair = new ProjectXmlPair(pair);

            // create new target
            const string NewTargetName = "NewTargetName";

            xmlPair.Add2NewChildrenWithVerify<ProjectTargetElement>(NewTargetName, (p, n) => p.AddTarget(n), (t, n) => string.Equals(t.Name, n), out var newTarget1, out var newTarget2);


            // add tasks to target
            const string NewTaskName = "NewTaskName";
            newTarget1.Add2NewNamedChildrenWithVerify<ProjectTaskElement>(NewTaskName, (t, n) => t.AddTask(n), out var newTask1, out var newTask2);

            // Add item groups
            const string NewTargetItemGroup = "NewTargetItemGroup";
            newTarget1.Add2NewLabaledChildrenWithVerify<ProjectItemGroupElement>(NewTargetItemGroup, (t) => t.AddItemGroup(), out var newItemGroup1, out var newItemGroup2);

            // Add property groups
            const string NewPropertyGroup = "NewPropertyGroup";
            newTarget1.Add2NewLabaledChildrenWithVerify<ProjectPropertyGroupElement>(NewPropertyGroup, (t) => t.AddPropertyGroup(), out var newPropertyGroup1, out var newPropertyGroup2);

            // string setters
            newTarget1.VerifySetter("newBeforeTargets", (t) => t.BeforeTargets, (t, v) => t.BeforeTargets = v);
            newTarget1.VerifySetter("newDependsOnTargets", (t) => t.DependsOnTargets, (t, v) => t.DependsOnTargets = v);
            newTarget1.VerifySetter("newAfterTargets", (t) => t.AfterTargets, (t, v) => t.AfterTargets = v);
            newTarget1.VerifySetter("newReturns", (t) => t.Returns, (t, v) => t.Returns = v);
            newTarget1.VerifySetter("newInputs", (t) => t.Inputs, (t, v) => t.Inputs = v);
            newTarget1.VerifySetter("newOutputs", (t) => t.Outputs, (t, v) => t.Outputs = v);
            newTarget1.VerifySetter("newKeepDuplicateOutputs", (t) => t.KeepDuplicateOutputs, (t, v) => t.KeepDuplicateOutputs = v);


            newTarget1.VerifySetter("'Configuration' == 'Foo'", (t) => t.Condition, (t, v) => t.Condition = v);
            newTarget1.VerifySetter("newLabel", (t) => t.Label, (t, v) => t.Label = v);

            // rename target. First validate we do not change identity of the view
            const string NewTargetRenamed = "NewTargetRenamed";
            newTarget1.View.Name = NewTargetRenamed;
            Assert.Empty(xmlPair.QueryChildrenWithValidation<ProjectTargetElement>((t) => string.Equals(t.Name, NewTargetName)));
            newTarget1.VerifySame(xmlPair.QuerySingleChildrenWithValidation<ProjectTargetElement>((t) => string.Equals(t.Name, NewTargetRenamed)));

            newTarget1.Real.Name = NewTargetRenamed.Ver(2);
            Assert.Empty(xmlPair.QueryChildrenWithValidation<ProjectTargetElement>((t) => string.Equals(t.Name, NewTargetRenamed)));
            Assert.Empty(xmlPair.QueryChildrenWithValidation<ProjectTargetElement>((t) => string.Equals(t.Name, NewTargetName)));

            newTarget1.VerifySame(xmlPair.QuerySingleChildrenWithValidation<ProjectTargetElement>((t) => string.Equals(t.Name, NewTargetRenamed.Ver(2))));

            // this will rename back, as well as check the reqular way (after we confirmed the view identity dont change on rename).
            newTarget1.VerifySetter(NewTargetName, (t) => t.Name, (t, v) => t.Name = v);


            // removes
            newTarget1.View.RemoveChild(newTask2.View);
            Assert.ThrowsAny<ArgumentException>( () => newTarget1.Real.RemoveChild(newTask2.Real) );
            Assert.Equal(1, newTarget1.View.Tasks.Count);
            newTarget1.Real.RemoveChild(newTask1.Real);
            Assert.ThrowsAny<ArgumentException>(() => newTarget1.View.RemoveChild(newTask1.View));
            Assert.Empty(newTarget1.View.Tasks);

            Assert.NotEmpty(newTarget1.View.ItemGroups);
            Assert.NotEmpty(newTarget1.View.PropertyGroups);
            newTarget1.View.RemoveAllChildren();

            Assert.Empty(newTarget1.View.ItemGroups);
            Assert.Empty(newTarget1.View.PropertyGroups);


            newTarget1.Verify();
        }

        [Fact]
        public void ProjectTaskElementModify()
        {
            var pair = GetNewInMemoryProject("temp.prj");
            var xmlPair = new ProjectXmlPair(pair);

            // create new target
            const string NewTasktName = "NewTaskName";

            var newTarget = xmlPair.AddNewChildrenWithVerify<ProjectTargetElement>(ObjectType.View, "TargetToTestTask", (p, n) => p.AddTarget(n), (t, n) => string.Equals(t.Name, n));
            var newTask = newTarget.AddNewNamedChildrenWithVerify<ProjectTaskElement>(ObjectType.View, NewTasktName, (t, n) => t.AddTask(n));

            Assert.Equal(0, newTask.View.Outputs.Count);
            const string NewOutputItem = "NewOutputItem";
            newTask.Add2NewChildrenWithVerify<ProjectOutputElement>(NewOutputItem, (t, n) => t.AddOutputItem(n, "CPP"), (oi, n) => oi.TaskParameter == n, out var newOutputItem1, out var newOutputItem2);
            Assert.True(newOutputItem1.View.IsOutputItem);
            Assert.False(newOutputItem1.View.IsOutputProperty);


            const string NewOutputItemWithConfig = "NewOutputItemCfg";
            newTask.Add2NewChildrenWithVerify<ProjectOutputElement>(NewOutputItemWithConfig, (t, n) => t.AddOutputItem(n, "source", "'Configuration'='Foo'"), (oi, n) => oi.TaskParameter == n, out var newOutputItemWithConfig1, out var newOutputItemWithConfig2);
            Assert.True(newOutputItemWithConfig1.View.IsOutputItem);
            Assert.False(newOutputItemWithConfig1.View.IsOutputProperty);

            const string NewOutputProperty = "NewOutputProperty";
            newTask.Add2NewChildrenWithVerify<ProjectOutputElement>(NewOutputProperty, (t, n) => t.AddOutputProperty(n, "taskprop"), (oi, n) => oi.TaskParameter == n, out var newOutputProp1, out var newOutputProp2);
            Assert.False(newOutputProp1.View.IsOutputItem);
            Assert.True(newOutputProp1.View.IsOutputProperty);


            const string NewOutputPropertyWithConfig = "NewOutputPropertyCfg";
            newTask.Add2NewChildrenWithVerify<ProjectOutputElement>(NewOutputPropertyWithConfig, (t, n) => t.AddOutputProperty(n, "source", "'Configuration'='Foo'"), (oi, n) => oi.TaskParameter == n, out var newOutputPropWithConfig1, out var newOutputPropWithConfig2);
            Assert.False(newOutputPropWithConfig1.View.IsOutputItem);
            Assert.True(newOutputPropWithConfig1.View.IsOutputProperty);

            Assert.Equal(8, newTask.View.Outputs.Count);

            newTask.VerifySetter("ErrorAndContinue", (t) => t.ContinueOnError, (t, v) => t.ContinueOnError = v);
            newTask.VerifySetter("v665+1", (t) => t.MSBuildRuntime, (t, v) => t.MSBuildRuntime = v);
            newTask.VerifySetter("msbuild256bit", (t) => t.MSBuildArchitecture, (t, v) => t.MSBuildArchitecture = v);

            // test parameters
            newTask.View.RemoveAllParameters();
            newTask.Verify();

            Assert.Equal(0, newTask.View.Parameters.Count);

            const string paramName = "paramName";
            const string paramValue = "paramValue";
            for (int i = 1; i <= 5; i++)
            {
                newTask.VerifySetter(paramValue.Ver(i), (t) => t.GetParameter(paramName.Ver(i)), (t, v) => t.SetParameter(paramName.Ver(i), v));
            }

            newTask.Verify(); 
            Assert.Equal(5, newTask.View.Parameters.Count);
            for (int i = 1; i<= 5; i++)
            {
                Assert.Equal(paramValue.Ver(i), newTask.View.Parameters[paramName.Ver(i)]);
            }

            newTask.View.RemoveParameter(paramName.Ver(1));
            newTask.Real.RemoveParameter(paramName.Ver(5));
            newTask.Verify();
            Assert.Equal(3, newTask.View.Parameters.Count);
            for (int i = 2; i <= 4; i++)
            {
                Assert.Equal(paramValue.Ver(i), newTask.View.Parameters[paramName.Ver(i)]);
            }

            Assert.False(newTask.View.Parameters.ContainsKey(paramName.Ver(1)));
            Assert.False(newTask.Real.Parameters.ContainsKey(paramName.Ver(1)));
            Assert.False(newTask.View.Parameters.ContainsKey(paramName.Ver(5)));
            Assert.False(newTask.Real.Parameters.ContainsKey(paramName.Ver(5)));


            newTask.View.RemoveAllParameters();
            newTask.Verify();
            Assert.Equal(0, newTask.View.Parameters.Count);


            newTask.View.RemoveChild(newOutputItem2.View);
            Assert.ThrowsAny<ArgumentException>(() => newTask.Real.RemoveChild(newOutputItem2.Real));
            Assert.Equal(7, newTask.View.Outputs.Count);
            newTask.Real.RemoveChild(newOutputItemWithConfig2.Real);
            Assert.ThrowsAny<ArgumentException>(() => newTask.View.RemoveChild(newOutputItem2.View));

            Assert.Equal(6, newTask.View.Outputs.Count);

            newTask.Real.RemoveChild(newOutputProp2.Real);
            Assert.Equal(5, newTask.View.Outputs.Count);
            newTask.View.RemoveChild(newOutputPropWithConfig2.View);
            Assert.Equal(4, newTask.View.Outputs.Count);

            newTask.QueryChildrenWithValidation<ProjectOutputElement>((po) => po.TaskParameter.EndsWith("1"), 4);

            newTask.Verify();
        }

        [Fact]
        public void ProjectOutputElementModify()
        {
            var pair = GetNewInMemoryProject("temp.prj");
            var xmlPair = new ProjectXmlPair(pair);

            var newTarget = xmlPair.AddNewChildrenWithVerify<ProjectTargetElement>(ObjectType.View, "TargetToTestTask", (p, n) => p.AddTarget(n), (t, n) => string.Equals(t.Name, n));
            var newTask = newTarget.AddNewNamedChildrenWithVerify<ProjectTaskElement>(ObjectType.Real, "NewTaskName", (t, n) => t.AddTask(n));

            const string NewOutputItem = "NewOutputItem";
            const string ItemType = "CPPSource";
            var newOutputItem =  newTask.AddNewChildrenWithVerify<ProjectOutputElement>(ObjectType.View, NewOutputItem, (t, n) => t.AddOutputItem(n, ItemType), (oi, n) => oi.TaskParameter == n);

            Assert.True(newOutputItem.View.IsOutputItem);
            Assert.False(newOutputItem.View.IsOutputProperty);

            const string NewOutputProperty = "NewOutputProperty";
            const string PropertyName = "OutputPropName";
            var newOutputProp = newTask.AddNewChildrenWithVerify<ProjectOutputElement>(ObjectType.View, NewOutputProperty, (t, n) => t.AddOutputProperty(n, PropertyName), (oi, n) => oi.TaskParameter == n);
            Assert.False(newOutputProp.View.IsOutputItem);
            Assert.True(newOutputProp.View.IsOutputProperty);

            newOutputItem.VerifySetter(NewOutputItem.Ver(1), (o) => o.TaskParameter, (o, v) => o.TaskParameter = v);
            newOutputProp.VerifySetter(NewOutputProperty.Ver(1), (o) => o.TaskParameter, (o, v) => o.TaskParameter = v);

            newOutputItem.VerifySetter(ItemType.Ver(1), (o) => o.ItemType, (o, v) => o.ItemType = v);
            Assert.ThrowsAny<InvalidOperationException>(() => newOutputProp.View.ItemType = "foo");

            newOutputProp.VerifySetter(PropertyName.Ver(1), (o) => o.PropertyName, (o, v) => o.PropertyName = v);
            Assert.ThrowsAny<InvalidOperationException>(() => newOutputItem.View.PropertyName = "foo");
        }

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectMetadataElementModify()
        {
            var pair = GetNewInMemoryProject("temp.prj");
            var xmlPair = new ProjectXmlPair(pair);
        }

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectChooseElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectWhenElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectOtherwiseElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectUsingTaskBodyElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectUsingTaskParameterElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void UsingTaskParameterGroupElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectUsingTaskElementModify() => throw new NotImplementedException();


        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectExtensionsElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectImportElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectImportGroupElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectItemDefinitionElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectItemDefinitionGroupElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectItemElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectItemGroupElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectPropertyElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectPropertyGroupElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectSdkElementModify() => throw new NotImplementedException();

        [Fact(Skip = "TODO: NotImplemented")]
        public void ProjectOnErrorElementModify() => throw new NotImplementedException();
    }
}


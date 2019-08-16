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

        // validates:
        // ProjectTargetElement
        // ProjectTaskElement
        // ProjectOutputElement
        [Fact]
        public void ProjectTargetAndRelatedModify()
        {
            var pair = GetNewInMemoryProject("temp.prj");
            var xmlPair = new ProjectXmlPair(pair);

            // create new target
            const string NewTargetName = "NewTargetName";
            var newTargetView = xmlPair.View.AddTarget(NewTargetName);
            var newTarget = xmlPair.QuerySingleChildrenWithValidation<ProjectTargetElement>((t) => string.Equals(t.Name, NewTargetName));
            Assert.Same(newTargetView, newTarget.View);

            var newTarget2View = xmlPair.View.AddTarget(NewTargetName.Ver(2));
            var newTarget2 = xmlPair.QuerySingleChildrenWithValidation<ProjectTargetElement>((t) => string.Equals(t.Name, NewTargetName.Ver(2)));
            Assert.Same(newTarget2View, newTarget2.View);


            // add task to target
            const string NewTaskName = "NewTaskName";
            newTarget.Add2NewNamedChildrenWithVerify<ProjectTaskElement>(NewTaskName, NewTaskName.Ver(2), (t, n) => t.AddTask(n), out var newTask, out var newTask2);

            // ProjectTaskElement validation (embedded here since task are only accessible from targets)
            const string NewOutputItem = "NewOutputItem";
            newTask.Add2NewChildrenWithVerify<ProjectOutputElement>(NewOutputItem, (t, n) => t.AddOutputItem(n, "CPP"), (oi, n) => oi.TaskParameter == n, out var newOutputItem1, out var newOutputItem2);
            Assert.True(newOutputItem1.View.IsOutputItem);
            Assert.False(newOutputItem1.View.IsOutputProperty);


            const string NewOutputItemWithConfig = "NewOutputItemCfg";
            newTask.Add2NewChildrenWithVerify<ProjectOutputElement>(NewOutputItemWithConfig, (t, n) => t.AddOutputItem(n, "source", "'Configuration'='Foo'"), (oi, n) => oi.TaskParameter == n, out var newOutputItemWithConfig1, out var newOutputWithConfig2);
            Assert.True(newOutputItemWithConfig1.View.IsOutputItem);
            Assert.False(newOutputItemWithConfig1.View.IsOutputProperty);




            // Add ItemGroup2
            const string NewTargetItemGroup = "NewTargetItemGroup";
            newTarget.Add2NewLabaledChildrenWithVerify<ProjectItemGroupElement>(NewTargetItemGroup, NewTargetItemGroup.Ver(2), (t) => t.AddItemGroup(), out var newItemGroup1, out var newItemGroup2);

            // Add property group
            const string NewPropertyGroup = "NewPropertyGroup";
            newTarget.Add2NewLabaledChildrenWithVerify<ProjectPropertyGroupElement>(NewPropertyGroup, NewPropertyGroup.Ver(2), (t) => t.AddPropertyGroup(), out var newPropertyGroup1, out var newPropertyGroup2);


            // string setters
            newTarget.VerifySetter("newBeforeTargets", (t) => t.BeforeTargets, (t, v) => t.BeforeTargets = v);
            newTarget.VerifySetter("newDependsOnTargets", (t) => t.DependsOnTargets, (t, v) => t.DependsOnTargets = v);
            newTarget.VerifySetter("newAfterTargets", (t) => t.AfterTargets, (t, v) => t.AfterTargets = v);
            newTarget.VerifySetter("newReturns", (t) => t.Returns, (t, v) => t.Returns = v);
            newTarget.VerifySetter("newInputs", (t) => t.Inputs, (t, v) => t.Inputs = v);
            newTarget.VerifySetter("newOutputs", (t) => t.Outputs, (t, v) => t.Outputs = v);
            newTarget.VerifySetter("newKeepDuplicateOutputs", (t) => t.KeepDuplicateOutputs, (t, v) => t.KeepDuplicateOutputs = v);


            newTarget.VerifySetter("'Configuration' == 'Foo'", (t) => t.Condition, (t, v) => t.Condition = v);
            newTarget.VerifySetter("newLabel", (t) => t.Label, (t, v) => t.Label= v);
            
            // rename target. First validate we do not change identity of the view
            const string NewTargetRenamed = "NewTargetRenamed";
            newTarget.View.Name = NewTargetRenamed;
            Assert.Empty(xmlPair.QueryChildrenWithValidation<ProjectTargetElement>((t) => string.Equals(t.Name, NewTargetName)));
            newTarget.VerifySame(xmlPair.QuerySingleChildrenWithValidation<ProjectTargetElement>((t) => string.Equals(t.Name, NewTargetRenamed)));

            newTarget.Real.Name = NewTargetRenamed.Ver(2);
            Assert.Empty(xmlPair.QueryChildrenWithValidation<ProjectTargetElement>((t) => string.Equals(t.Name, NewTargetRenamed)));
            Assert.Empty(xmlPair.QueryChildrenWithValidation<ProjectTargetElement>((t) => string.Equals(t.Name, NewTargetName)));

            newTarget.VerifySame(xmlPair.QuerySingleChildrenWithValidation<ProjectTargetElement>((t) => string.Equals(t.Name, NewTargetRenamed.Ver(2))));

            // this will rename back, as well as check the reqular way (after we confirmed the view identity dont change on rename).
            newTarget.VerifySetter(NewTargetName, (t) => t.Name, (t, v) => t.Name = v);

            // check everything before start removing
            newTarget.Verify();






        }
    }
}


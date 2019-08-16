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
        public void ProjectTargetAndTaskElementsModify()
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


            // embeded ProjectTaskElement validation.
            // add task to target
            const string NewTaskName = "NewTaskName";
            var newTaskView = newTarget.View.AddTask(NewTaskName);
            var newTask = xmlPair.QuerySingleChildrenWithValidation<ProjectTaskElement>((t) => string.Equals(t.Name, NewTaskName));
            Assert.Same(newTaskView, newTask.View);

            // Add ItemGroup
            const string NewTargetItemGroup = "NewTargetItemGroup";

            var newItemGroup1View = newTarget.View.AddItemGroup();
            Assert.NotNull(newItemGroup1View);
            newItemGroup1View.Label = NewTargetItemGroup.Ver(1);

            var newItemGroup2Real = newTarget.Real.AddItemGroup();
            Assert.NotNull(newItemGroup2Real);
            newItemGroup2Real.Label = NewTargetItemGroup.Ver(2);

            var newItemGroup1 = xmlPair.QuerySingleChildrenWithValidation<ProjectItemGroupElement>((t) => string.Equals(t.Label, NewTargetItemGroup.Ver(1)));
            var newItemGroup2 = xmlPair.QuerySingleChildrenWithValidation<ProjectItemGroupElement>((t) => string.Equals(t.Label, NewTargetItemGroup.Ver(2)));

            Assert.Same(newItemGroup1View, newItemGroup1.View);
            Assert.Same(newItemGroup2Real, newItemGroup2.Real);

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


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
            var newTargetView = xmlPair.ViewXml.AddTarget(NewTargetName);

            Assert.Same(newTargetView, xmlPair.QuerySingleChildrenWithValidation<ProjectTargetElement>(ObjectType.View, (t) => string.Equals(t.Name, NewTargetName)));

            // add task to target
            const string NewTaskName = "NewTaskName";
            var newTaskView = newTargetView.AddTask(NewTaskName);
            Assert.Same(newTaskView, xmlPair.QuerySingleChildrenWithValidation<ProjectTaskElement>(ObjectType.View, (t) => string.Equals(t.Name, NewTaskName)));

            // rename target
            const string NewTargetRenamed = "NewTargetRenamed";
            newTargetView.Name = NewTargetRenamed;
            Assert.Empty(xmlPair.QueryChildrenWithValidation<ProjectTargetElement>(ObjectType.View, (t) => string.Equals(t.Name, NewTargetName)));
            Assert.Same(newTargetView, xmlPair.QuerySingleChildrenWithValidation<ProjectTargetElement>(ObjectType.View, (t) => string.Equals(t.Name, NewTargetRenamed)));

        }
    }
}


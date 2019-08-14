// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{

    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Microsoft.Build.Evaluation;
    using Xunit;
    using Xunit.Abstractions;

    public class LinkedEvaluationModify_Tests : IClassFixture<LinkedEvaluationModify_Tests.MyTestCollectionGroup>
    {
        public class MyTestCollectionGroup : TestCollectionGroup
        {
            public MyTestCollectionGroup() : base(2, 1) { }
        }

        public TestCollectionGroup StdGroup { get; }
        public LinkedEvaluationModify_Tests(MyTestCollectionGroup group)
        {

            this.StdGroup = group;
            group.Clear();
        }

        [Fact]
        public void ProjectRenameAndSafeAs()
        {
            var pcLocal = this.StdGroup.Local;
            var pcRemote = this.StdGroup.Remote[0];

            var proj1Path = this.StdGroup.StdProjectFiles[0];
            var realProj = pcRemote.LoadProject(proj1Path);
            pcLocal.Importing = true;
            var viewProj = pcLocal.Collection.GetLoadedProjects(proj1Path).FirstOrDefault();


            ViewValidation.Verify(viewProj, realProj);
            var savedPath = this.StdGroup.Disk.GetAbsolutePath("Saved.proj");

            Assert.NotEqual(proj1Path, savedPath);
            Assert.Equal(proj1Path, viewProj.FullPath);
            Assert.True(File.Exists(proj1Path));
            Assert.False(File.Exists(savedPath));

            var lwtBefore = new FileInfo(proj1Path).LastWriteTimeUtc;

            Assert.False(realProj.IsDirty);
            Assert.False(viewProj.IsDirty);

            viewProj.FullPath = savedPath;
            Assert.Equal(savedPath, realProj.FullPath);
            Assert.True(realProj.IsDirty);
            Assert.True(viewProj.IsDirty);

            viewProj.Save();

            Assert.True(realProj.IsDirty);
            Assert.True(viewProj.IsDirty);
            // it should still be dirty since it is not reevaluated.


            Assert.True(File.Exists(savedPath));

            var lwtAfter = new FileInfo(proj1Path).LastWriteTimeUtc;
            Assert.Equal(lwtBefore, lwtAfter);


            viewProj.ReevaluateIfNecessary();

            // now it should be not dirty anymore.
            Assert.False(realProj.IsDirty);
            Assert.False(viewProj.IsDirty);

            // and finally just ensure that all is identical
            ViewValidation.Verify(viewProj, realProj);
        }


        [Fact]
        public void ProjectItemModify()
        {
            var pcLocal = this.StdGroup.Local;
            var pcRemote = this.StdGroup.Remote[0];

            var proj1Path = this.StdGroup.StdProjectFiles[0];
            var realProj = pcRemote.LoadProject(proj1Path);
            pcLocal.Importing = true;
            var viewProj = pcLocal.Collection.GetLoadedProjects(proj1Path).FirstOrDefault();

            ViewValidation.Verify(viewProj, realProj);

            var added = viewProj.AddItem("cpp", "foo.cpp");
            Assert.NotNull(added);
            Assert.Equal(1, added.Count);
            var fooView1 = added.FirstOrDefault();
            var justAdded = viewProj.GetItemsByEvaluatedInclude("foo.cpp");
            Assert.NotNull(justAdded);
            Assert.Equal(1, justAdded.Count);
            var fooView2 = justAdded.FirstOrDefault();
            Assert.Same(fooView1, fooView2);




            ViewValidation.Verify(viewProj, realProj);

        }

    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Xunit;
    using Xunit.Abstractions;

    public class LinkedConstruction_Tests
    {

        [Fact]
        public void ProjectRootElemetBasic()
        {
            var group = ProjectCollectionLinker.CreateGroup();
            var pcLocal = group.AddNew();
            var pcRemote = group.AddNew();
            pcLocal.Importing = true;

            using (var disk = new TransientIO())
            {
                var proj1Path = disk.WriteProjectFile("Proj1.proj", TestCollectionGroup.SampleProjectFile);
                var proj1Real = pcRemote.LoadProject(proj1Path);

                var proj1View = pcLocal.GetLoadedProjects(proj1Path).FirstOrDefault();
                Assert.NotNull(proj1View);

                var preReal = proj1Real.Xml;
                var preView = proj1View.Xml;

                Assert.NotNull(preView);
                LinkedObjectsValidation.VerifyProjectElementContainerView(preView, preReal, true);


                Assert.Equal(preView.FullPath, preReal.FullPath);
                Assert.Equal(preView.DirectoryPath, preReal.DirectoryPath);
                Assert.Equal(preView.RawXml, preReal.RawXml);

                Assert.Equal(preView.Sdk, preReal.Sdk);
                Assert.Equal(preView.Sdk, preReal.Sdk);
            }
        }
    }
}

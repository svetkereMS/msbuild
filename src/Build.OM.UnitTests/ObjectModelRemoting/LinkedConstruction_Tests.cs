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

    public class LinkedConstruction_Tests : IDisposable
    {
        private class MyTestCollectionGroup : TestCollectionGroup
        {
            public string BigFile { get;}

            public MyTestCollectionGroup()
                : base (2, 4)
            {
                this.BigFile = this.ImmutableDisk.WriteProjectFile($"Big.proj", TestCollectionGroup.BigProjectFile);
            }
        }

        private MyTestCollectionGroup StdGroup { get; }

        public LinkedConstruction_Tests(ITestOutputHelper output)
        {
            this.StdGroup = new MyTestCollectionGroup();
        }

        private void ResetBeforeTest()
        {
            // we do not modify "disk" for collection tests.
            this.StdGroup.Clear();
        }

        [Fact]
        public void ProjectRootElemetMatch()
        {
            ResetBeforeTest();
            var pcLocal = this.StdGroup.Local;
            var pcRemote = this.StdGroup.Remote[0];
            pcLocal.Importing = true;

            var projPath = this.StdGroup.BigFile;
            var projReal = pcRemote.LoadProject(projPath);

            var projView = pcLocal.GetLoadedProjects(projPath).FirstOrDefault();
            Assert.NotNull(projView);

            var preReal = projReal.Xml;
            var preView = projView.Xml;

            Assert.NotNull(preView);
            LinkedObjectsValidation.VerifyProjectElementContainerView(preView, preReal, true);


            Assert.Equal(preView.FullPath, preReal.FullPath);
            Assert.Equal(preView.DirectoryPath, preReal.DirectoryPath);
            Assert.Equal(preView.RawXml, preReal.RawXml);

            Assert.Equal(preView.Sdk, preReal.Sdk);
            Assert.Equal(preView.Sdk, preReal.Sdk);
        }


        [Fact]
        public void ProjectChooseElemet()
        {
            ResetBeforeTest();
            var pcLocal = this.StdGroup.Local;
            var pcRemote = this.StdGroup.Remote[0];
            pcLocal.Importing = true;

            var projPath = this.StdGroup.BigFile;
            var projReal = pcRemote.LoadProject(projPath);

            var projView = pcLocal.GetLoadedProjects(projPath).FirstOrDefault();
            Assert.NotNull(projView);

            var preReal = projReal.Xml;
            var preView = projView.Xml;
            Assert.NotNull(preReal);
            Assert.NotNull(preView);

            Assert.Single(preReal.ChooseElements);
            Assert.Single(preView.ChooseElements);

            var realChoose = preReal.ChooseElements.FirstOrDefault();
            var viewChoose = preView.ChooseElements.FirstOrDefault();

            LinkedObjectsValidation.VerifyProjectElementView(viewChoose, realChoose, true);
        }

        [Fact]
        public void ProjectExtensionsElement()
        {
            ResetBeforeTest();
            var pcLocal = this.StdGroup.Local;
            var pcRemote = this.StdGroup.Remote[0];
            pcLocal.Importing = true;

            var projPath = this.StdGroup.BigFile;
            var projReal = pcRemote.LoadProject(projPath);

            var projView = pcLocal.GetLoadedProjects(projPath).FirstOrDefault();
            Assert.NotNull(projView);

            var preReal = projReal.Xml;
            var preView = projView.Xml;
            Assert.NotNull(preReal);
            Assert.NotNull(preView);

            var realExtensionsList = preReal.ChildrenReversed.OfType<ProjectExtensionsElement>().ToList();
            var viewExtensionsList = preView.ChildrenReversed.OfType<ProjectExtensionsElement>().ToList();

            Assert.Single(realExtensionsList);
            Assert.Single(viewExtensionsList);
            var realExtension = realExtensionsList.FirstOrDefault();
            var viewExtension = viewExtensionsList.FirstOrDefault();
            LinkedObjectsValidation.VerifyProjectElementView(viewExtension, realExtension, true);
            Assert.Equal(realExtension.Content, viewExtension.Content);

            Assert.Equal(realExtension["a"], viewExtension["a"]);
            Assert.Equal(realExtension["b"], viewExtension["b"]);
            Assert.Equal("x", viewExtension["a"]);
            Assert.Equal("y", viewExtension["b"]);
        }







        public void Dispose()
        {
            this.StdGroup.Dispose();
        }
    }
}

// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
namespace Microsoft.Build.UnitTests.OM.ObjectModelRemoting
{

    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class TestCollectionGroup : IDisposable
    {
        public static string SampleProjectFile = ObjectModelHelpers.CleanupFileContents(@"
                    <Project xmlns='msbuildnamespace' ToolsVersion='2.0' InitialTargets='it' DefaultTargets='dt'>
                        <PropertyGroup Condition=""'$(Configuration)'=='Foo'"">
                            <p>v1</p>
                        </PropertyGroup>
                        <PropertyGroup Condition=""'$(Configuration)'!='Foo'"">
                            <p>v2</p>
                        </PropertyGroup>
                        <PropertyGroup>
                            <p2>X$(p)</p2>
                        </PropertyGroup>
                        <ItemGroup>
                            <i Condition=""'$(Configuration)'=='Foo'"" Include='i0'/>
                            <i Include='i1'/>
                            <i Include='$(p)X;i3'/>
                        </ItemGroup>
                        <Target Name='t'>
                            <task/>
                        </Target>
                    </Project>
                ");

        public static string BigProjectFile = ObjectModelHelpers.CleanupFileContents(@"
                    <Project xmlns='msbuildnamespace' ToolsVersion='2.0' InitialTargets='it' DefaultTargets='dt'>
                        <PropertyGroup Condition=""'$(Configuration)'=='Foo'"">
                            <p>v1</p>
                        </PropertyGroup>
                        <PropertyGroup Condition=""'$(Configuration)'!='Foo'"">
                            <p>v2</p>
                        </PropertyGroup>
                        <PropertyGroup>
                            <p2>X$(p)</p2>
                        </PropertyGroup>
                        <ItemGroup>
                            <i Condition=""'$(Configuration)'=='Foo'"" Include='i0'/>
                            <i Include='i1'/>
                            <i Include='$(p)X;i3'/>
                        </ItemGroup>

                        <Choose>
                            <When Condition=""'$(Configuration)'=='Foo'"">
                              <PropertyGroup>
                                <p>vFoo</p>
                              </PropertyGroup> 
                            </When>
                            <When Condition='false'>
                              <PropertyGroup>
                                <p>vFalse</p>
                              </PropertyGroup> 
                            </When>      
                            <When Condition='true'>
                              <PropertyGroup>
                                <p>vTrue</p>
                              </PropertyGroup> 
                            </When>      
                            <Otherwise>
                              <PropertyGroup>
                                <p>vOtherwise</p>
                              </PropertyGroup> 
                            </Otherwise>
                        </Choose>

                        <Target Name='t'>
                            <task/>
                        </Target>

                       <ProjectExtensions>
                         <a>x</a>
                         <b>y</b>
                       </ProjectExtensions>
                    </Project>
                ");


        public int RemoteCount { get; }

        public ProjectCollectionLinker.ConnectedProjectCollections Group { get; }
        public ProjectCollectionLinker Local { get; }

        public ProjectCollectionLinker[] Remote { get; } = new ProjectCollectionLinker[2];
        public TransientIO Disk { get; }
        protected TransientIO ImmutableDisk { get; }
        public IReadOnlyList<string> StdProjectFiles { get; }


        public TestCollectionGroup(int remoteCount, int stdFilesCount)
        {
            this.RemoteCount = 2;

            this.Group = ProjectCollectionLinker.CreateGroup();

            this.Local = this.Group.AddNew();
            this.Remote = new ProjectCollectionLinker[this.RemoteCount];
            for (int i = 0; i < this.RemoteCount; i++)
            {
                this.Remote[i] = this.Group.AddNew();
            }

            this.ImmutableDisk = new TransientIO();
            this.Disk = this.ImmutableDisk.GetSubFolder("Mutable");

            List<string> stdFiles = new List<string>();
            for (int i=0; i< stdFilesCount; i++)
            {
                stdFiles.Add(this.ImmutableDisk.WriteProjectFile($"Proj{i}.proj", TestCollectionGroup.SampleProjectFile));
            }

            this.StdProjectFiles = stdFiles;
        }

        public void Clear()
        {
            this.Local.Importing = false;
            this.Local.Collection.UnloadAllProjects();
            foreach (var remote in this.Remote)
            {
                remote.Importing = false;
                remote.Collection.UnloadAllProjects();
            }

            this.Group.ClearAllRemotes();
            this.Disk.Clear();
        }

        public void Dispose()
        {
            this.Clear();
            this.ImmutableDisk.Dispose();
        }
    }
}

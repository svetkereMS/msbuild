// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.UnitTests
{
    using System;
    using System.IO;
    using Microsoft.Build.Shared;

    internal class TransientIO : IDisposable
    {
        private DirectoryInfo root;

        private DirectoryInfo EnsureTempRoot()
        {
            if (this.root == null)
            {
                this.root = new DirectoryInfo(FileUtilities.GetTemporaryDirectory(true));
            }

            return this.root;
        }

        private static bool IsDirSlash(char c) => c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;

        public bool IsControled(string path)
        {
            if (this.root == null || path == null) return false;
            var tempRoot = this.RootFolder;
            path = Path.GetFullPath(path);
            return path != null && tempRoot != null
                && path.Length > tempRoot.Length
                && IsDirSlash(path[tempRoot.Length])
                && path.StartsWith(tempRoot, StringComparison.OrdinalIgnoreCase);
        }

        public string RootFolder => EnsureTempRoot().FullName;

        public void EnsureFileLocation(string path)
        {
            var absolute = this.GetAbsolutePath(path);
            var parent = Path.GetDirectoryName(absolute);
            if (!Directory.Exists(parent))
            {
                Directory.CreateDirectory(parent);
            }
        }

        public string WriteProjectFile(string path, string content)
        {
            var absolute = this.GetAbsolutePath(path);
            content = ObjectModelHelpers.CleanupFileContents(content);
            this.EnsureFileLocation(absolute);
            File.WriteAllText(absolute, content);
            return absolute;
        }

        public string GetAbsolutePath(string relative)
        {
            var tempRoot = this.RootFolder;
            var absolute = Path.GetFullPath(Path.Combine(tempRoot, relative));
            if (!IsControled(absolute))
            {
                throw new ArgumentException(nameof(relative));
            }

            return absolute;
        }

        public void Dispose()
        {
            this.Clear();
            // this object still can be used ... 
        }

        public void Clear()
        {
            if (this.root != null)
            {
                FileUtilities.DeleteDirectoryNoThrow(this.root.FullName, true);
                this.root = null;
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using NUnit.Framework;
using System.IO;

namespace Tests
{
    public static class TestFileSystem
    {
        [NUnit.Framework.Test]
        public static void TestPathToFromDirDir()
        {
            /// <summary>
            /// Generate a list of DirectoryInfos representing all the folders in between the input ancestor and descendant
            /// The first element is 'ancestor'
            /// The last element is descendant's parent
            /// If 'descendant' is not a descendant of 'ancestor', returns null
            /// </summary>

            List<DirectoryInfo> path = FileSystem.PathToFrom(new DirectoryInfo(@"C:\a\b\\\\c\d\e\f"), new DirectoryInfo(@"C:\a\"));
            Assert.AreEqual(5, path.Count);
            Assert.True(DirectoryEqualityComparer.SameDir(new DirectoryInfo(@"C:\a"), path.ElementAt(0)));
            Assert.True(DirectoryEqualityComparer.SameDir(new DirectoryInfo(@"C:\a\b"), path.ElementAt(1)));
            Assert.True(DirectoryEqualityComparer.SameDir(new DirectoryInfo(@"C:\a\b\c"), path.ElementAt(2)));
            Assert.True(DirectoryEqualityComparer.SameDir(new DirectoryInfo(@"C:\a\b\c\d"), path.ElementAt(3)));
            Assert.True(DirectoryEqualityComparer.SameDir(new DirectoryInfo(@"C:\a\b\c\d\e"), path.ElementAt(4)));
        }
    }
}

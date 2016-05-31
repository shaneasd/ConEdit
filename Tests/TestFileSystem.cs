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
            {
                List<DirectoryInfo> path = FileSystem.PathToFrom(new DirectoryInfo(@"C:\a\b\\\\c\d\e\\\f"), new DirectoryInfo(@"C:\a\\"));
                Assert.AreEqual(6, path.Count);
                Assert.True(DirectoryEqualityComparer.SamePath(new DirectoryInfo(@"C:\a"), path.ElementAt(0)));
                Assert.True(DirectoryEqualityComparer.SamePath(new DirectoryInfo(@"C:\a\b"), path.ElementAt(1)));
                Assert.True(DirectoryEqualityComparer.SamePath(new DirectoryInfo(@"C:\a\b\c"), path.ElementAt(2)));
                Assert.True(DirectoryEqualityComparer.SamePath(new DirectoryInfo(@"C:\a\b\c\d"), path.ElementAt(3)));
                Assert.True(DirectoryEqualityComparer.SamePath(new DirectoryInfo(@"C:\a\b\c\d\e"), path.ElementAt(4)));
                Assert.True(DirectoryEqualityComparer.SamePath(new DirectoryInfo(@"C:\a\b\c\d\e\f"), path.ElementAt(5)));
            }

            {
                List<DirectoryInfo> path = FileSystem.PathToFrom(new DirectoryInfo(@"C:\a\b\\\\c\d\e\f\\"), new DirectoryInfo(@"C:\a\"));
                Assert.AreEqual(6, path.Count);
                Assert.True(DirectoryEqualityComparer.SamePath(new DirectoryInfo(@"C:\a"), path.ElementAt(0)));
                Assert.True(DirectoryEqualityComparer.SamePath(new DirectoryInfo(@"C:\a\b"), path.ElementAt(1)));
                Assert.True(DirectoryEqualityComparer.SamePath(new DirectoryInfo(@"C:\a\b\c"), path.ElementAt(2)));
                Assert.True(DirectoryEqualityComparer.SamePath(new DirectoryInfo(@"C:\a\b\c\d"), path.ElementAt(3)));
                Assert.True(DirectoryEqualityComparer.SamePath(new DirectoryInfo(@"C:\a\b\c\d\e"), path.ElementAt(4)));
                Assert.True(DirectoryEqualityComparer.SamePath(new DirectoryInfo(@"C:\a\b\c\d\e\f"), path.ElementAt(5)));
            }

            {
                DirectoryInfo anscestor = new DirectoryInfo(@"c:\");
                Assert.True(DirectoryEqualityComparer.SamePath(anscestor, FileSystem.PathToFrom(anscestor, anscestor).Single()));
            }

            {
                DirectoryInfo anscestor = new DirectoryInfo(@"c:\a\");
                Assert.True(DirectoryEqualityComparer.SamePath(anscestor, FileSystem.PathToFrom(anscestor, anscestor).Single()));
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Utilities;

namespace Utilities
{
    public static class FileSystem
    {
        public static bool SameDir(DirectoryInfo dir1, DirectoryInfo dir2)
        {
            return dir1.FullName == dir2.FullName;
        }

        public static bool AncestorOf(this FileInfo child, DirectoryInfo ancestor)
        {
            for (var test = child.Directory; test != null; test = test.Parent)
                if (SameDir(test, ancestor))
                    return true;
            return false;
        }

        public static List<DirectoryInfo> PathToFrom(FileInfo child, DirectoryInfo ancestor)
        {
            Stack<DirectoryInfo> stack = new Stack<DirectoryInfo>();
            for (var test = child.Directory; test != null; test = test.Parent)
            {
                stack.Push(test);
                if (SameDir(test, ancestor))
                {
                    List<DirectoryInfo> result = new List<DirectoryInfo>();
                    while (stack.Any())
                        result.Add(stack.Pop());
                    return result;
                }
            }
            return null;
        }

        public static List<DirectoryInfo> PathToFrom(DirectoryInfo child, DirectoryInfo ancestor)
        {
            Stack<DirectoryInfo> stack = new Stack<DirectoryInfo>();
            for (var test = child.Parent; test != null; test = test.Parent)
            {
                stack.Push(test);
                if (SameDir(test, ancestor))
                {
                    List<DirectoryInfo> result = new List<DirectoryInfo>();
                    while (stack.Any())
                        result.Add(stack.Pop());
                    return result;
                }
            }
            return null;
        }

        public static string RelativePath(FileInfo child, DirectoryInfo root)
        {
            var path = PathToFrom(child, root);
            return string.Join("" + Path.DirectorySeparatorChar, path.Skip(1).Select(f => f.Name).Concat(child.Name.Only()).ToArray());
        }
    }
}

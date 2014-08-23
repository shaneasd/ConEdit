using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Utilities;
using System.Threading;

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

        /// <summary>
        /// Replace current with copy of replacement if contents are different
        /// </summary>
        /// <param name="current">The current contents of the stream, stream is replaced if contents of current are different to contents of replacement</param>
        /// <param name="replacement">Reference stream (Can be disposed after this method completes)</param>
        /// <param name="abort">If abort is set, operation is cancelled, method returns false, no change to current</param>
        /// <returns>Returns true iff the contents of current and replacement are different (and as such current is replaced)</returns>
        public static bool ChangeIfDifferent(ref MemoryStream current, Stream replacement, WaitHandle abort)
        {
            current.Position = 0;
            replacement.Position = 0;
            bool same = current.Length == replacement.Length;

            MemoryStream m = null;
            try
            {
                m = new MemoryStream((int)replacement.Length);
                m.SetLength(replacement.Length);
                var buffer = m.GetBuffer();
                var asyncWait = replacement.BeginRead(buffer, 0, (int)replacement.Length, null, null);
                int index = WaitHandle.WaitAny(new WaitHandle[] { abort, asyncWait.AsyncWaitHandle });

                if (index == 0)
                    return false;

                if (same)
                {
                    for (int j = 0; j < replacement.Length && same; j += 128)
                    {
                        long stop = Math.Min(replacement.Length, j + 128);
                        for (int i = j; i < stop && same; i++)
                        {
                            int c = current.ReadByte();
                            int r = buffer[i];
                            same &= c == r;
                        }
                        if (abort.WaitOne(0))
                            return false;
                    }
                }

                if (same)
                    return false;

                current.Dispose();
                current = m;
                m = null;
                return true;
            }
            finally
            {
                if (m != null)
                    m.Dispose();
            }
        }
    }
}

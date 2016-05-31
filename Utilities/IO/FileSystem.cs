using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Utilities;
using System.Threading;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Utilities
{
    public static class FileSystem
    {
        [SuppressMessage("Microsoft.Design", "CA1011",
            Justification = "We only use the FullName member so 'ancestor' could be a FileSystemInfo but we already know that only a DirectoryInfo can be the parent of another FileSystemInfo")]
        public static bool AncestorOf(this FileInfo descendant, DirectoryInfo ancestor)
        {
            var pathA = ancestor.FullName;
            var pathD = descendant.FullName;
            return pathD.StartsWith(pathA, StringComparison.OrdinalIgnoreCase);
        }

        public static List<DirectoryInfo> PathToFrom(FileInfo descendant, DirectoryInfo ancestor)
        {
            var result = PathToFrom(descendant.Directory, ancestor);
            return result;
        }

        /// <summary>
        /// Generate a list of DirectoryInfos representing all the folders in between the input ancestor and descendant
        /// The first element is 'ancestor'
        /// The last element is 'descendant'
        /// If 'descendant' is not a descendant of 'ancestor', returns null
        /// </summary>
        public static List<DirectoryInfo> PathToFrom(DirectoryInfo descendant, DirectoryInfo ancestor)
        {
            string pathA = ancestor.FullName.TrimEnd('\\');
            string pathD = descendant.FullName.TrimEnd('\\');

            if (!pathD.StartsWith(pathA, StringComparison.OrdinalIgnoreCase))
                return null;

            List<DirectoryInfo> result = new List<DirectoryInfo>();

            result.Add(ancestor); //The ancestor is always the first element

            if (pathA != pathD)
            {
                for (int i = pathD.IndexOf('\\', pathA.Length + 1); i != -1; i = i + 1 < pathD.Length ? pathD.IndexOf('\\', i + 1) : -1)
                {
                    result.Add(new DirectoryInfo(pathD.Substring(0, i)));
                }

                //The descendant is always the last element
                result.Add(descendant); //We know pathD does not end in a slash and so the last substring has not yet been included
            }
            return result;
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

                using (StreamWriter a = new StreamWriter(@"C:\fileA.txt"))
                {
                    a.Write(current.ToArray().Select(x=>(char)x).ToArray());
                }
                using (StreamWriter a = new StreamWriter(@"C:\fileB.txt"))
                {
                    a.Write(m.ToArray().Select(x => (char)x).ToArray());
                }

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

        public static bool EnsureExists(this DirectoryInfo dir)
        {
            //This operation will return false if we recurse all the way to the root which doesn't exist.
            //This will likely end up giving a more logical exception in subsequent operations than we could generate here
            if (dir == null)
                return false;

            if (!dir.Exists)
            {
                if (!EnsureExists(dir.Parent))
                    return false;
                dir.Create();
                return true;
            }
            return true;
        }
    }
}

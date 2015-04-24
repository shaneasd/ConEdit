using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace Utilities
{
    public class DirectoryInfoHashed
    {
        public static int collisions = 0;
        private DirectoryInfo m_wrapped;
        private int m_hashCode;
        public DirectoryInfoHashed(DirectoryInfo wrapped)
        {
            m_wrapped = wrapped;
            m_hashCode = DirectoryEqualityComparer.GetHashCodeStatic(wrapped);
        }

        public override bool Equals(object obj)
        {
            DirectoryInfoHashed other = (DirectoryInfoHashed)obj;
            if ( other == null )
                return false;
            if (other.m_hashCode == m_hashCode)
                collisions++;
            return DirectoryEqualityComparer.SameDir(other.m_wrapped, m_wrapped);
        }

        public override int GetHashCode()
        {
            return m_hashCode;
        }
    }
    public class DirectoryEqualityComparer : IEqualityComparer<DirectoryInfo>
    {
        //http://stackoverflow.com/questions/1794025/how-to-check-whether-2-directoryinfo-objects-are-pointing-to-the-same-directory
        static public bool SameDir(DirectoryInfo dir1, DirectoryInfo dir2)
        {
            return 0 == String.Compare(
                    dir1.FullName.TrimEnd('\\'),
                    dir2.FullName.TrimEnd('\\'),
                    StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(DirectoryInfo x, DirectoryInfo y)
        {
            return SameDir(x, y);
        }

        public int GetHashCode(DirectoryInfo obj)
        {
            return GetHashCodeStatic(obj);
        }

        public static int GetHashCodeStatic(FileSystemInfo obj)
        {
            return obj.FullName.TrimEnd('\\').ToUpper(CultureInfo.InvariantCulture).GetHashCode();
        }
    }
}

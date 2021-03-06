﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Utilities
{
    public class DirectoryInfoHashed
    {
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
            if (other == null)
                return false;
            return DirectoryEqualityComparer.SamePath(other.m_wrapped, m_wrapped);
        }

        public override int GetHashCode()
        {
            return m_hashCode;
        }
    }
    public class DirectoryEqualityComparer : IEqualityComparer<DirectoryInfo>
    {
        //http://stackoverflow.com/questions/1794025/how-to-check-whether-2-directoryinfo-objects-are-pointing-to-the-same-directory
        static public bool SamePath<T>(T dir1, T dir2) where T : FileSystemInfo //Valid for FileInfo-FileInfo or DirectoryInfo-DirectoryInfo but not a mix
        {
            return 0 == String.Compare(
                    dir1.FullName.TrimEnd('\\'),
                    dir2.FullName.TrimEnd('\\'),
                    StringComparison.OrdinalIgnoreCase);
        }

        public bool Equals(DirectoryInfo x, DirectoryInfo y)
        {
            return SamePath(x, y);
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace ConversationEditor
{
    public class DocumentPath
    {
        public string AbsolutePath { get; }
        public string RelativePath { get; }
        public FileInfo FileInfo { get { return new FileInfo(AbsolutePath); } }
        public bool Exists { get { return FileInfo.Exists; } }

        public override bool Equals(object obj)
        {
            return (obj as DocumentPath)?.AbsolutePath?.Equals(AbsolutePath) ?? false;
        }

        public override int GetHashCode()
        {
            return AbsolutePath.GetHashCode();
        }

        public override string ToString()
        {
            throw new NotSupportedException("Tried to generate a string from a DocumentPath. Probably indicates an error");
        }

        private DocumentPath(string absolutePath, string relativePath)
        {
            AbsolutePath = absolutePath;
            RelativePath = relativePath;
        }

        public static DocumentPath FromAbsolutePath(string absolutePath, DirectoryInfo origin)
        {
            string relativePath = FileSystem.RelativePath(new FileInfo(absolutePath), origin);
            return new DocumentPath(absolutePath, relativePath);
        }

        public static DocumentPath FromRelativePath(string relativePath, DirectoryInfo origin)
        {
            string absolutePath = Path.Combine(origin.FullName, relativePath);
            return new DocumentPath(absolutePath, relativePath);
        }

        public static DocumentPath FromPath(string path, DirectoryInfo origin)
        {
            if (Path.IsPathRooted(path))
                return FromAbsolutePath(path, origin);
            else
                return FromRelativePath(path, origin);
        }

        public static DocumentPath FromPath(FileInfo path, DirectoryInfo origin)
        {
            //path.FullName has already resolved a relative path
            //path.Name is just the file name. Not the path specified to the constructor
            //path.ToString appears to be the original string
            return FromPath(path.ToString(), origin);
        }
    }
}
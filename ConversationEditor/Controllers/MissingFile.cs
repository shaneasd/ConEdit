using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Utilities;

namespace ConversationEditor
{
    internal class MissingFile : Disposable, ISaveableFile
    {
        private FileInfo m_file;

        public MissingFile(FileInfo file)
        {
            m_file = file;
        }

        public FileInfo File
        {
            get { return m_file; }
        }

        public bool Move(System.IO.FileInfo newPath, Func<bool> replace)
        {
            throw new NotImplementedException();
        }

        public void GotMoved(FileInfo newPath)
        {
            m_file = newPath;
        }

        public bool CanClose()
        {
            return true;
        }

        public event Action Modified { add { } remove { } } //Missing file cannot be modified

        public event Action<Changed<System.IO.FileInfo>> Moved { add { } remove { } } //Missing file cannot be moved

        public event Action SaveStateChanged { add { } remove { } } //Can't be modified

        public event Action FileModifiedExternally { add { } remove { } }
        public event Action FileDeletedExternally { add { } remove { } }

        public bool Exists
        {
            get { return false; }
        }

        public IUndoQueue UndoQueue
        {
            get { return NoUndoQueue.Instance; }
        }

        protected override void Dispose(bool disposing)
        {
        }

        public IWritable Writable
        {
            get { return null; }
        }
    }
}

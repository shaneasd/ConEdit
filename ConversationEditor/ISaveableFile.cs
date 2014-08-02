using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Utilities;

namespace ConversationEditor
{
    public interface ISaveableFileProvider : IDisposable
    {
        ISaveableFile File { get; }
        event Action FileModifiedExternally;
        event Action FileDeletedExternally;
    }

    public interface ISaveableFileUndoableProvider : ISaveableFileProvider
    {
        ISaveableFileUndoable UndoableFile { get; }
    }

    //public interface IFileSystemEntity
    //{
    //    string Name { get; }
    //}

    public interface ISaveableFileBase : IDisposable
    {
        FileInfo File { get; }
        void Save();
        void SaveAs(FileInfo path);
        bool Move(FileInfo newPath, Func<bool> replace);
        /// <summary>
        /// Notify the file that it has been moved due to a parent folder being renamed
        /// </summary>
        void GotMoved(FileInfo newPath);
        bool Changed { get; }
        bool CanClose();
        bool Exists { get; }

        event Action FileModifiedExternally;
        event Action FileDeletedExternally;
    }

    public interface ISaveableFile : ISaveableFileBase
    {
        IUndoQueue UndoQueue { get; }
        event Action<FileInfo, FileInfo> Moved;
        event Action Modified;
        event Action SaveStateChanged;
    }

    public interface ISaveableFileUndoable : ISaveableFile
    {
        void Change(UndoAction actions);
    }

    public abstract class SaveableFile : ISaveableFileBase, IDisposable
    {
        private FileSystemWatcher m_modifiedwatcher;
        private FileSystemWatcher m_deletedwatcher;
        private FileInfo m_file;
        private Action<Stream> m_saveTo;

        public SaveableFile(FileInfo path, Action<Stream> saveTo)
        {
            m_modifiedwatcher = new FileSystemWatcher();
            m_deletedwatcher = new FileSystemWatcher();
            File = path;
            m_saveTo = saveTo;
        }

        public FileInfo File
        {
            get { return m_file; }
            private set
            {
                if (m_file != value)
                {
                    m_file = value;
                    foreach (var watcher in new[] { m_modifiedwatcher, m_deletedwatcher })
                    {
                        watcher.Path = m_file.Directory.FullName;
                        watcher.Filter = m_file.Name;
                        watcher.EnableRaisingEvents = true;
                    }
                    m_modifiedwatcher.Changed += OnFileModifiedExternally;
                    m_deletedwatcher.Deleted += OnFileDeletedExternally;
                    m_deletedwatcher.Renamed += OnFileDeletedExternally;
                }
            }
        }

        public abstract bool Changed { get; }

        /// <summary>
        /// Provide the user a Save, Don't Save, Cancel option if the file needs to be saved
        /// Return true if the file doesn't need to be saved following this method call.
        /// </summary>
        public bool CanClose()
        {
            if (Changed)
            {
                var result = MessageBox.Show(File.Name + " has unsaved changes. Do you want to save?", "Unsaved changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                switch (result)
                {
                    case DialogResult.Yes:
                        Save(); //If the file has been saved we can close it
                        return true;
                    case DialogResult.No:
                        return true; //If the user doesn't want to save his changes we can close the file
                    case DialogResult.Cancel:
                        return false; //If the user doesn't want to close the file we can't
                    default:
                        throw new Exception();
                }
            }
            return true; //If the file hasn't been changed we can close it
        }

        public void Save()
        {
            //if (Changed)
            {
                m_modifiedwatcher.EnableRaisingEvents = false;
                try
                {
                    using (FileStream stream = Util.LoadFileStream(File, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                    {
                        m_saveTo(stream);
                        stream.Flush(true);
                    }
                }
                finally
                {
                    m_modifiedwatcher.EnableRaisingEvents = true;
                }
                m_deletedwatcher.EnableRaisingEvents = true;

                Saved();
            }
        }

        public void SaveAs(FileInfo path)
        {
            var oldPath = File;
            File = path;
            Save();
            Moved.Execute(oldPath, File);
        }

        protected abstract void Saved();

        public string Name
        {
            get { return File.Name; }
        }

        public event Action<FileInfo, FileInfo> Moved;

        public bool Exists
        {
            get { return true; }
        }

        public bool Move(FileInfo path, Func<bool> replace)
        {
            var oldFile = File;
            if (path.Exists)
                if (replace())
                    path.Delete();
                else
                    return false;
            System.IO.File.Move(File.FullName, path.FullName);
            File = path;
            Moved.Execute(oldFile, File);
            return true;
        }

        public void GotMoved(FileInfo newPath)
        {
            var oldFile = File;
            File = newPath;
            Moved.Execute(oldFile, File);
        }

        private void OnFileModifiedExternally(object sender, FileSystemEventArgs e)
        {
            m_modifiedwatcher.EnableRaisingEvents = false; //Stop listening so that we can only queue modified events while not processing them
            try
            {
                FileModifiedExternally.Execute();
            }
            finally
            {
                if (File.Exists && m_modifiedwatcher != null) //modified watcher could be disposed (and set to null) in the callback
                    m_modifiedwatcher.EnableRaisingEvents = true;
            }
        }

        private void OnFileDeletedExternally(object sender, FileSystemEventArgs e)
        {
            m_deletedwatcher.EnableRaisingEvents = false; //Stop listening for deletions as the file can't be deleted more than once (unless someone recreates it which we'll ignore)
            m_modifiedwatcher.EnableRaisingEvents = false; //Stop listening for modifications as the file can't be modified if it doesn't exist (and if someone recreated one we don't care)
            FileDeletedExternally.Execute();
        }

        public event Action FileModifiedExternally;

        public event Action FileDeletedExternally;

        public void Dispose()
        {
            //Can't dispose the watchers from within their own thread
            //m_modifiedwatcher.Dispose();
            //m_deletedwatcher.Dispose();

            //So make sure they won't trigger any more,
            m_modifiedwatcher.EnableRaisingEvents = false;
            m_deletedwatcher.EnableRaisingEvents = false;

            //remove our reference to them,
            var a = m_modifiedwatcher; m_modifiedwatcher = null;
            var b = m_deletedwatcher; m_deletedwatcher = null;

            //and dispose them as soon as we can
            Action dispose = () => { a.Dispose(); b.Dispose(); };
            dispose.BeginInvoke(null, null);
        }
    }

    public class SaveableFileUndoable : SaveableFile, ISaveableFileUndoable
    {
        public SaveableFileUndoable(FileInfo path, Action<Stream> saveTo)
            : base(path, saveTo)
        {
            FileModifiedExternally += () => m_undoQueue.NeverSaved();
            FileDeletedExternally += () => m_undoQueue.NeverSaved();
        }

        readonly UndoQueue m_undoQueue = new UndoQueue();
        public IUndoQueue UndoQueue { get { return m_undoQueue; } }

        public override bool Changed { get { return m_undoQueue.Modified; } }

        public void Change(UndoAction actions)
        {
            Change(actions.Actions(), actions.Description);
        }

        public void Change(SimpleUndoPair actions, string description)
        {
            var action = new GenericUndoAction(new SimpleUndoPair
            {
                Redo = () => { actions.Redo(); Modified.Execute(); },
                Undo = () => { actions.Undo(); Modified.Execute(); }
            }, description);
            action.Redo();
            m_undoQueue.Queue(action);
        }

        protected override void Saved()
        {
            m_undoQueue.Saved();
        }

        public event Action Modified;
        public event Action SaveStateChanged { add { m_undoQueue.ModifiedChanged += value; } remove { m_undoQueue.ModifiedChanged -= value; } }
    }

    public class SaveableFileNotUndoable : SaveableFile, ISaveableFile
    {
        public SaveableFileNotUndoable(FileInfo path, Action<Stream> saveTo) : base(path, saveTo) { }

        readonly NoUndoQueue m_undoQueue = new NoUndoQueue();
        public IUndoQueue UndoQueue { get { return m_undoQueue; } }

        bool m_changed = false;

        public void Change()
        {
            if (!m_changed)
            {
                m_changed = true;
                SaveStateChanged.Execute();
            }
            Modified.Execute();
        }

        public override bool Changed
        {
            get { return m_changed; }
        }

        protected override void Saved()
        {
            m_changed = false;
            SaveStateChanged.Execute();
        }

        public event Action SaveStateChanged;
        public event Action Modified;
    }

    public class ReadonlyFile : ISaveableFile
    {
        private FileSystemWatcher m_modifiedwatcher;
        private FileSystemWatcher m_deletedwatcher;
        private FileInfo m_file;

        public ReadonlyFile(FileInfo file)
        {
            File = file;
            m_modifiedwatcher = new FileSystemWatcher();
            m_deletedwatcher = new FileSystemWatcher();
            m_modifiedwatcher.Changed += OnFileModifiedExternally;
            m_deletedwatcher.Deleted += OnFileDeletedExternally;
            m_deletedwatcher.Renamed += OnFileDeletedExternally;
        }

        public FileInfo File
        {
            get { return m_file; }
            private set
            {
                if (m_file != value)
                {
                    m_file = value;
                    foreach (var watcher in new[] { m_modifiedwatcher, m_deletedwatcher })
                    {
                        watcher.Path = m_file.Directory.FullName;
                        watcher.Filter = m_file.Name;
                        watcher.EnableRaisingEvents = true;
                    }
                    m_modifiedwatcher.Changed += OnFileModifiedExternally;
                    m_deletedwatcher.Deleted += OnFileDeletedExternally;
                    m_deletedwatcher.Renamed += OnFileDeletedExternally;
                }
            }
        }

        public void Save()
        {
            //No need
        }

        void ISaveableFileBase.SaveAs(FileInfo path)
        {
            throw new NotSupportedException("Can't save a readonly file");
        }

        public bool Move(FileInfo path, Func<bool> replace)
        {
            var oldFile = File;
            if (path.Exists)
                if (replace())
                    path.Delete();
                else
                    return false;
            System.IO.File.Move(oldFile.FullName, path.FullName);
            File = path;
            Moved.Execute(oldFile, File);
            return true;
        }

        public void GotMoved(FileInfo newPath)
        {
            var oldFile = File;
            File = newPath;
            Moved.Execute(oldFile, File);
        }

        public bool Changed
        {
            get { return false; }
        }

        public bool CanClose()
        {
            return true;
        }

        public bool Exists
        {
            get { return true; }
        }

        public event Action<FileInfo, FileInfo> Moved;
        public event Action Modified { add { } remove { } } //Can't be modified

        public void Change(UndoAction actions)
        {
            throw new NotSupportedException("Cannot modify a readonly file");
        }

        public IUndoQueue UndoQueue
        {
            get { return new NoUndoQueue(); }
        }

        public event Action SaveStateChanged { add { } remove { } } //Can't be modified/saved

        private void OnFileModifiedExternally(object sender, FileSystemEventArgs e)
        {
            m_modifiedwatcher.EnableRaisingEvents = false; //Stop listening so that we can only queue modified events while not processing them
            try
            {
                FileModifiedExternally.Execute();
            }
            finally
            {
                if (File.Exists && m_modifiedwatcher != null) //modified watcher could be disposed (and set to null) in the callback
                    m_modifiedwatcher.EnableRaisingEvents = true;
            }
        }

        private void OnFileDeletedExternally(object sender, FileSystemEventArgs e)
        {
            m_deletedwatcher.EnableRaisingEvents = false; //Stop listening for deletions as the file can't be deleted more than once (unless someone recreates it which we'll ignore)
            m_modifiedwatcher.EnableRaisingEvents = false; //Stop listening for modifications as the file can't be modified if it doesn't exist (and if someone recreated one we don't care)
            FileDeletedExternally.Execute();
        }

        public event Action FileModifiedExternally;

        public event Action FileDeletedExternally;

        public void Dispose()
        {
            //Can't dispose the watchers from within their own thread
            //m_modifiedwatcher.Dispose();
            //m_deletedwatcher.Dispose();

            //So make sure they won't trigger any more,
            m_modifiedwatcher.EnableRaisingEvents = false;
            m_deletedwatcher.EnableRaisingEvents = false;

            //remove our reference to them,
            var a = m_modifiedwatcher; m_modifiedwatcher = null;
            var b = m_deletedwatcher; m_deletedwatcher = null;

            //and dispose them as soon as we can
            Action dispose = () => { a.Dispose(); b.Dispose(); };
            dispose.BeginInvoke(null, null);
        }
    }
}

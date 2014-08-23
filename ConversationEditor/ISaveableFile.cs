using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Utilities;
using System.Threading;

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
        bool Writable { get; } //TODO: If it's not writable is it really a saveablefile?

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

    public class UpToDateFile : IDisposable
    {
        public FileInfo File { get { return m_file; } }

        private FileSystemWatcher m_watcher;
        private FileInfo m_file;
        private AutoResetEvent m_reopen;
        private Thread m_worker;
        private MemoryStream m_onDisk;
        private object m_onDiskAccess = new object();
        private Action<Stream> m_saveTo;
        public event Action FileChanged; //A change was made to the file externally
        public event Action FileDeleted; //The file was deleted
        private ManualResetEventSlim m_abort;
        private ManualResetEventSlim m_aborted;
        private ManualResetEventSlim m_deleted;

        public UpToDateFile(MemoryStream lastSavedOrLoaded, FileInfo file, Action<Stream> saveTo)
        {
            m_file = file;
            m_onDisk = lastSavedOrLoaded;
            m_saveTo = saveTo;

            m_abort = new ManualResetEventSlim(false);
            m_reopen = new AutoResetEvent(false);
            m_deleted = new ManualResetEventSlim(false);
            m_aborted = new ManualResetEventSlim(false);
            m_worker = new Thread(UpdateThread);
            m_worker.Start();
            m_watcher = new FileSystemWatcher();
            m_watcher.Path = m_file.Directory.FullName;
            m_watcher.Filter = m_file.Name;
            m_watcher.Changed += m_watcher_Changed;
            m_watcher.Deleted += m_watcher_Changed;
            m_watcher.Renamed += m_watcher_Changed;
            m_watcher.EnableRaisingEvents = true;

            Application.ApplicationExit += (a, b) => { m_abort.Set(); }; //Rather than clients being responsible for disposing the object and thus killing the thread, just have the thread monitor whether the application wants to exit
        }

        void m_watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
                m_reopen.Set();
            else
                m_deleted.Set();
        }

        private void UpdateThread()
        {
            try
            {
                while (true)
                {
                    var condition = WaitHandle.WaitAny(new WaitHandle[] { m_abort.WaitHandle, m_deleted.WaitHandle, m_reopen });
                    if (m_abort.IsSet)
                        return;
                    else if (m_deleted.IsSet)
                    {
                        FileDeleted.Execute();
                        return;
                    }
                    else
                    {
                        bool fileChanged = false;
                        while (true)
                        {
                            if (m_abort.IsSet)
                                return;
                            try
                            {
                                using (var stream = Util.LoadFileStream(m_file, FileMode.Open, FileAccess.Read))
                                {
                                    int length = (int)stream.Length;
                                    if (length > 100e6)
                                        MessageBox.Show("Attempting to load more than 100MB from a file. This is probably why we get the out of memory exception");
                                    //MemoryStream m = null;
                                    lock (m_onDiskAccess)
                                    {
                                        fileChanged = FileSystem.ChangeIfDifferent(ref m_onDisk, stream, m_abort.WaitHandle);
                                        break;
                                        //try
                                        //{
                                        //    m = new MemoryStream(length);
                                        //    stream.CopyTo(m);

                                        //    var data1 = m.GetBuffer().Take((int)m.Length);

                                        //    var data2 = m_onDisk.GetBuffer().Take((int)m_onDisk.Length);
                                        //    //TODO: Introduce periodic checking if m_abort for particularly long comparisons
                                        //    if (!data1.SequenceEqual(data2)) //TODO: Refactor this comparison into a utility that could be optimized
                                        //    {
                                        //        //The data has changed
                                        //        m_onDisk = m;
                                        //        m = null;
                                        //        fileChanged = true;
                                        //    }
                                        //    break;
                                        //}
                                        //finally //Almost a using block except we want to be able to cancel the dispose if we decide to keep the new data
                                        //{
                                        //    if (m != null)
                                        //        m.Dispose();
                                        //}
                                    }
                                }
                            }
                            catch (MyFileLoadException)
                            {
                            }

                            //Essentially wait 100ms before trying the file again but if we get terminated in the mean time just abort
                            if (m_abort.Wait(100))
                                return;
                        }
                        if (fileChanged)
                            ThreadPool.QueueUserWorkItem(a => FileChanged.Execute()); //Queue the changes on a thread so this thread can be aborted due to a dispose triggered from the callback
                    }
                }
            }
            finally
            {
                m_aborted.Set();
            }
        }

        public void Save()
        {
            using (FileStream file = Util.LoadFileStream(m_file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                m_deleted.Reset();
                lock (m_onDiskAccess)
                {
                    m_onDisk.Position = 0;
                    m_onDisk.SetLength(0);
                    m_saveTo(m_onDisk);
                    m_onDisk.Position = 0;
                    file.SetLength(0);
                    m_onDisk.CopyTo(file);
                    file.Flush(true);
                }
            }
        }

        public void Dispose()
        {
            m_abort.Set();
            bool success = m_aborted.Wait(100000); //Wait a second. If we were unlucky and in the middle of a stream comparison it could take a while.
            DisposeContents();
        }

        private void DisposeContents()
        {
            m_watcher.Dispose();
            m_reopen.Dispose();
            if (m_onDisk != null)
                m_onDisk.Dispose();
            m_abort.Dispose();
            m_aborted.Dispose();
            m_deleted.Dispose();
        }

        public MemoryStream Migrate()
        {
            m_abort.Set();
            MemoryStream temp = m_onDisk;
            m_onDisk = null;
            this.Dispose();
            return temp;
        }
    }

    public abstract class SaveableFile : ISaveableFileBase, IDisposable
    {
        private UpToDateFile m_upToDateFile;

        private MemoryStream m_lastSavedOrLoaded;
        private Action<Stream> m_saveTo;

        public SaveableFile(MemoryStream initialContent, FileInfo path, Action<Stream> saveTo)
        {
            m_upToDateFile = new UpToDateFile(initialContent, path, saveTo);
            m_upToDateFile.FileChanged += () => FileModifiedExternally.Execute();
            m_upToDateFile.FileDeleted += () => FileDeletedExternally.Execute();
            m_saveTo = saveTo;
            m_lastSavedOrLoaded = initialContent;
        }

        public FileInfo File
        {
            get { return m_upToDateFile.File; }
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
            m_upToDateFile.Save();
            Saved();
        }

        public void SaveAs(FileInfo path)
        {
            var oldPath = File;
            m_upToDateFile.Dispose();
            m_upToDateFile = new UpToDateFile(new MemoryStream(), path, m_saveTo);
            m_upToDateFile.FileChanged += () => FileModifiedExternally.Execute();
            m_upToDateFile.FileDeleted += () => FileDeletedExternally.Execute();
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
            var stream = m_upToDateFile.Migrate();
            System.IO.File.Move(File.FullName, path.FullName);
            m_upToDateFile = new UpToDateFile(stream, path, m_saveTo);
            m_upToDateFile.FileChanged += () => FileModifiedExternally.Execute();
            m_upToDateFile.FileDeleted += () => FileDeletedExternally.Execute();
            Moved.Execute(oldFile, File);
            return true;
        }

        public void GotMoved(FileInfo newPath)
        {
            var oldFile = File;
            m_upToDateFile = new UpToDateFile(m_upToDateFile.Migrate(), newPath, m_saveTo);
            m_upToDateFile.FileChanged += () => FileModifiedExternally.Execute();
            m_upToDateFile.FileDeleted += () => FileDeletedExternally.Execute();
            Moved.Execute(oldFile, File);
        }

        public event Action FileModifiedExternally;

        public event Action FileDeletedExternally;

        public void Dispose()
        {
            m_upToDateFile.Dispose();
        }

        public bool Writable
        {
            get { return true; }
        }
    }

    public class SaveableFileUndoable : SaveableFile, ISaveableFileUndoable
    {
        public SaveableFileUndoable(MemoryStream initialContent, FileInfo path, Action<Stream> saveTo)
            : base(initialContent, path, saveTo)
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
        public SaveableFileNotUndoable(MemoryStream initialContent, FileInfo path, Action<Stream> saveTo) : base(initialContent, path, saveTo) { }

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

    public class ReadonlyFileUnmonitored : ISaveableFile
    {
        public ReadonlyFileUnmonitored(FileInfo path)
        {
            File = path;
        }

        public IUndoQueue UndoQueue
        {
            get { throw new NotImplementedException(); }
        }

        public event Action<FileInfo, FileInfo> Moved;

        public event Action Modified { add { } remove { } }

        public event Action SaveStateChanged { add { } remove { } }

        public FileInfo File
        {
            get;
            private set;
        }

        public void Save()
        {
        }

        public void SaveAs(FileInfo path)
        {
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

        public bool Writable
        {
            get { return false; }
        }

        public event Action FileModifiedExternally { add { } remove { } }

        public event Action FileDeletedExternally { add { } remove { } }

        public void Dispose()
        {
        }
    }

    public class ReadonlyFile : ISaveableFile
    {
        private UpToDateFile m_upToDateFile;

        public ReadonlyFile(MemoryStream initialContent, FileInfo path)
        {
            m_upToDateFile = new UpToDateFile(initialContent, path, s => { });
            m_upToDateFile.FileChanged += () => FileModifiedExternally.Execute();
            m_upToDateFile.FileDeleted += () => FileDeletedExternally.Execute();
        }

        public FileInfo File
        {
            get { return m_upToDateFile.File; }
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

            var stream = m_upToDateFile.Migrate();
            m_upToDateFile.Dispose();
            System.IO.File.Move(oldFile.FullName, path.FullName);
            m_upToDateFile = new UpToDateFile(stream, path, s => { });
            m_upToDateFile.FileChanged += () => FileModifiedExternally.Execute();
            m_upToDateFile.FileDeleted += () => FileDeletedExternally.Execute();
            Moved.Execute(oldFile, File);
            return true;
        }

        public void GotMoved(FileInfo newPath)
        {
            var oldFile = File;
            m_upToDateFile = new UpToDateFile(m_upToDateFile.Migrate(), newPath, s => { });
            m_upToDateFile.FileChanged += () => FileModifiedExternally.Execute();
            m_upToDateFile.FileDeleted += () => FileDeletedExternally.Execute();
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

        public event Action FileModifiedExternally;

        public event Action FileDeletedExternally;

        public void Dispose()
        {
            m_upToDateFile.Dispose();
        }

        public bool Writable
        {
            get { return false; }
        }
    }
}

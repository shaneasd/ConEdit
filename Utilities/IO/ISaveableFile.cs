﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Utilities;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace Utilities
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

    public interface IWritable : IDisposable
    {
        void Save();
        void SaveAs(FileInfo path);
        bool Changed { get; }
    }

    public static class ISaveableFileBaseUtil
    {
        public static bool Changed(this ISaveableFileBase f)
        {
            return f.Writable != null && f.Writable.Changed;
        }
    }

    public interface ISaveableFileBase : IDisposable
    {
        FileInfo File { get; }
        bool Move(FileInfo newPath, Func<bool> replace);
        /// <summary>
        /// Notify the file that it has been moved due to a parent folder being renamed
        /// </summary>
        void GotMoved(FileInfo newPath);
        bool CanClose();
        bool Exists { get; }

        IWritable Writable { get; }

        event Action FileModifiedExternally;
        event Action FileDeletedExternally;
    }

    public interface ISaveableFile : ISaveableFileBase
    {
        IUndoQueue UndoQueue { get; }
        event Action<Changed<FileInfo>> Moved;
        event Action Modified;
        event Action SaveStateChanged;
    }

    public interface ISaveableFileUndoable : ISaveableFile
    {
        void Change(UndoAction actions);
    }

    public abstract class SaveableFile : Disposable, ISaveableFileBase, IWritable
    {
        private UpToDateFile m_upToDateFile;
        private Action<Stream> m_saveTo;
        private UpToDateFile.BackEnd m_backEnd;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialContent">Represents the current contents of the file. Reference is not held. A copy is made.</param>
        /// <param name="path"></param>
        /// <param name="saveTo"></param>
        protected SaveableFile(MemoryStream initialContent, FileInfo path, Action<Stream> saveTo, UpToDateFile.BackEnd backEnd)
        {
            m_upToDateFile = new UpToDateFile(initialContent, path, saveTo, backEnd);
            m_upToDateFile.FileChanged += () => FileModifiedExternally.Execute();
            m_upToDateFile.FileDeleted += () => FileDeletedExternally.Execute();
            m_saveTo = saveTo;
            m_backEnd = backEnd;
        }

        public FileInfo File => m_upToDateFile.File;

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
                        throw new InvalidOperationException();
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
            using (MemoryStream m = new MemoryStream())
            {
                m_upToDateFile = new UpToDateFile(m, path, m_saveTo, m_backEnd);
            }
            m_upToDateFile.FileChanged += () => FileModifiedExternally.Execute();
            m_upToDateFile.FileDeleted += () => FileDeletedExternally.Execute();
            Save();
            Moved.Execute(new Changed<FileInfo>(oldPath, File));
        }

        protected abstract void Saved();

        public string Name => File.Name;

        public event Action<Changed<FileInfo>> Moved;

        public bool Exists => true;

        public bool Move(FileInfo newPath, Func<bool> replace)
        {
            var oldFile = File;
            if (newPath.Exists)
                if (replace())
                    newPath.Delete();
                else
                    return false;
            var stream = m_upToDateFile.Migrate();
            newPath.Directory.EnsureExists(); //This can fail (returning false) if it does then we subsequently get a (hopefully) useful exception from File.Move
            System.IO.File.Move(File.FullName, newPath.FullName);
            m_upToDateFile.Dispose();
            m_upToDateFile = new UpToDateFile(stream, newPath, m_saveTo, m_backEnd);
            m_upToDateFile.FileChanged += () => FileModifiedExternally.Execute();
            m_upToDateFile.FileDeleted += () => FileDeletedExternally.Execute();
            Moved.Execute(new Changed<FileInfo>(oldFile, File));
            return true;
        }

        public void GotMoved(FileInfo newPath)
        {
            var oldFile = File;
            m_upToDateFile = new UpToDateFile(m_upToDateFile.Migrate(), newPath, m_saveTo, m_backEnd);
            m_upToDateFile.FileChanged += () => FileModifiedExternally.Execute();
            m_upToDateFile.FileDeleted += () => FileDeletedExternally.Execute();
            Moved.Execute(new Changed<FileInfo>(oldFile, File));
        }

        public event Action FileModifiedExternally;

        public event Action FileDeletedExternally;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_upToDateFile.Dispose();
            }
        }

        public IWritable Writable => this;
    }

    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Disposable behavior inherited from SaveableFile")]
    public class SaveableFileUndoable : SaveableFile, ISaveableFileUndoable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialContent">Represents the current contents of the file. Reference is not held. A copy is made.</param>
        /// <param name="path"></param>
        /// <param name="saveTo"></param>
        public SaveableFileUndoable(MemoryStream initialContent, FileInfo path, Action<Stream> saveTo, UpToDateFile.BackEnd backEnd)
            : base(initialContent, path, saveTo, backEnd)
        {
            FileModifiedExternally += () => m_undoQueue.NeverSaved();
            FileDeletedExternally += () => m_undoQueue.NeverSaved();
            m_undoQueue = new UndoQueue(path.Name);
        }

        readonly UndoQueue m_undoQueue;
        public IUndoQueue UndoQueue => m_undoQueue;

        public override bool Changed => m_undoQueue.Modified;

        public void ChangeNoUndo()
        {
            m_undoQueue.NeverSaved();
        }

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

    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Disposable behavior inherited from SaveableFile")]
    public class SaveableFileExternalChangedSource : SaveableFile, ISaveableFile
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialContent">Represents the current contents of the file. Reference is not held. A copy is made.</param>
        /// <param name="path"></param>
        /// <param name="saveTo"></param>
        /// <param name="changed"></param>
        /// <param name="saved"></param>
        public SaveableFileExternalChangedSource(MemoryStream initialContent, FileInfo path, Action<Stream> saveTo, Func<bool> changed, Action saved, UpToDateFile.BackEnd backEnd)
            : base(initialContent, path, saveTo, backEnd)
        {
            m_changed = changed;
            m_saved = saved;
            m_lastChanged = false;
        }

        public IUndoQueue UndoQueue => NoUndoQueue.Instance;

        private Func<bool> m_changed;
        private Action m_saved;
        private bool m_lastChanged;

        public void Change()
        {
            if (!m_lastChanged)
            {
                m_lastChanged = m_changed();
                SaveStateChanged.Execute();
            }
            Modified.Execute();
        }

        public override bool Changed => m_changed();

        protected override void Saved()
        {
            m_saved();
            m_lastChanged = m_changed();
            SaveStateChanged.Execute();
        }

        public event Action SaveStateChanged;
        public event Action Modified;
    }

    [SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "Disposable behavior inherited from SaveableFile")]
    public class SaveableFileNotUndoable : SaveableFile, ISaveableFile
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialContent">Represents the current contents of the file. Reference is not held. A copy is made.</param>
        /// <param name="path"></param>
        /// <param name="saveTo"></param>
        public SaveableFileNotUndoable(MemoryStream initialContent, FileInfo path, Action<Stream> saveTo, UpToDateFile.BackEnd backEnd)
            : base(initialContent, path, saveTo, backEnd)
        {
        }

        public IUndoQueue UndoQueue => NoUndoQueue.Instance;

        private bool m_changed = false;

        public void Change()
        {
            if (!m_changed)
            {
                m_changed = true;
                SaveStateChanged.Execute();
            }
            Modified.Execute();
        }

        public override bool Changed => m_changed;

        protected override void Saved()
        {
            m_changed = false;
            SaveStateChanged.Execute();
        }

        public event Action SaveStateChanged;
        public event Action Modified;
    }

    public class ReadOnlyFileUnmonitored : Disposable, ISaveableFile
    {
        public ReadOnlyFileUnmonitored(FileInfo path)
        {
            File = path;
        }

        public IUndoQueue UndoQueue => NoUndoQueue.Instance;

        public event Action<Changed<FileInfo>> Moved;

        public event Action Modified { add { } remove { } }

        public event Action SaveStateChanged { add { } remove { } }

        public FileInfo File
        {
            get;
            private set;
        }

        public bool Move(FileInfo newPath, Func<bool> replace)
        {
            var oldFile = File;
            if (newPath.Exists)
                if (replace())
                    newPath.Delete();
                else
                    return false;

            System.IO.File.Move(oldFile.FullName, newPath.FullName);
            File = newPath;
            Moved.Execute(new Changed<FileInfo>(oldFile, File));
            return true;
        }

        public void GotMoved(FileInfo newPath)
        {
            var oldFile = File;
            File = newPath;
            Moved.Execute(new Changed<FileInfo>(oldFile, File));
        }

        public bool CanClose()
        {
            return true;
        }

        public bool Exists => true;

        public IWritable Writable => null;

        public event Action FileModifiedExternally { add { } remove { } }

        public event Action FileDeletedExternally { add { } remove { } }

        protected override void Dispose(bool disposing)
        {
        }
    }

    public class ReadOnlyFile : Disposable, ISaveableFile
    {
        private UpToDateFile m_upToDateFile;
        private UpToDateFile.BackEnd m_backEnd;

        public ReadOnlyFile(MemoryStream initialContent, FileInfo path, UpToDateFile.BackEnd backEnd)
        {
            m_upToDateFile = new UpToDateFile(initialContent, path, s => { }, backEnd);
            m_backEnd = backEnd;
            m_upToDateFile.FileChanged += () => FileModifiedExternally.Execute();
            m_upToDateFile.FileDeleted += () => FileDeletedExternally.Execute();
        }

        public FileInfo File => m_upToDateFile.File;

        public bool Move(FileInfo newPath, Func<bool> replace)
        {
            var oldFile = File;
            if (newPath.Exists)
                if (replace())
                    newPath.Delete();
                else
                    return false;

            var stream = m_upToDateFile.Migrate();
            m_upToDateFile.Dispose();
            System.IO.File.Move(oldFile.FullName, newPath.FullName);
            m_upToDateFile = new UpToDateFile(stream, newPath, s => { }, m_backEnd);
            m_upToDateFile.FileChanged += () => FileModifiedExternally.Execute();
            m_upToDateFile.FileDeleted += () => FileDeletedExternally.Execute();
            Moved.Execute(new Changed<FileInfo>(oldFile, File));
            return true;
        }

        public void GotMoved(FileInfo newPath)
        {
            var oldFile = File;
            m_upToDateFile = new UpToDateFile(m_upToDateFile.Migrate(), newPath, s => { }, m_backEnd);
            m_upToDateFile.FileChanged += () => FileModifiedExternally.Execute();
            m_upToDateFile.FileDeleted += () => FileDeletedExternally.Execute();
            Moved.Execute(new Changed<FileInfo>(oldFile, File));
        }

        public bool CanClose()
        {
            return true;
        }

        public bool Exists => true;

        public event Action<Changed<FileInfo>> Moved;
        public event Action Modified { add { } remove { } } //Can't be modified

        public IUndoQueue UndoQueue => NoUndoQueue.Instance;

        public event Action SaveStateChanged { add { } remove { } } //Can't be modified/saved

        public event Action FileModifiedExternally;

        public event Action FileDeletedExternally;

        protected override void Dispose(bool disposing)
        {
            m_upToDateFile.Dispose();
        }

        public IWritable Writable => null;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace Utilities
{
    public class UpToDateFile : Disposable
    {
        /// <summary>
        /// Access to this class is threadsafe (other than constructor and disposer)
        /// </summary>
        public class Backend : Disposable
        {
            HashSet<UpToDateFile> m_deleted = new HashSet<UpToDateFile>();
            HashSet<UpToDateFile> m_modified = new HashSet<UpToDateFile>();

            private ManualResetEventSlim m_abort;
            private ManualResetEventSlim m_aborted;
            private AutoResetEvent m_event;
            private Thread m_worker;

            public WaitHandle AbortHandle { get { return m_abort.WaitHandle; } }

            public Backend()
            {
                m_abort = new ManualResetEventSlim(false);
                m_aborted = new ManualResetEventSlim(false);
                m_event = new AutoResetEvent(false);
                m_worker = new Thread(UpdateThread);
                m_worker.IsBackground = true;
                m_worker.Start();
            }

            private void UpdateThread()
            {
                try
                {
                    while (true)
                    {
                        WaitHandle[] handles = new WaitHandle[2];
                        handles[0] = m_abort.WaitHandle;
                        handles[1] = m_event;

                        int index = WaitHandle.WaitAny(handles);

                        if (index == 0) //Abort signal was sent
                        {
                            return;
                        }
                        else if (index == 1) //A file was modified or deleted
                        {

                            lock (m_deleted)
                            {
                                if (m_deleted.Any())
                                {
                                    var x = m_deleted.First();
                                    x.NotifyFileDeleted();
                                    lock (m_modified)
                                    {
                                        if (m_modified.Contains(x))
                                            m_modified.Remove(x);
                                    }
                                    m_deleted.Remove(x);
                                }
                                else
                                {
                                    lock (m_modified)
                                    {
                                        if (m_modified.Any())
                                        {
                                            var x = m_modified.First();
                                            var hasChanged = x.HasChanged();
                                            if (hasChanged.HasValue)
                                            {
                                                if (hasChanged.Value)
                                                    x.NotifyFileModified();
                                                m_modified.Remove(x);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    m_aborted.Set();
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    m_event.Reset();
                    m_abort.Set();
                    m_aborted.Wait(1000); //Wait a second. If we were unlucky and in the middle of a stream comparison it could take a while.
                                          //m_watcher.Dispose();
                                          //m_reopen.Dispose();
                                          //if (m_onDisk != null)
                                          //m_onDisk.Dispose();
                    m_abort.Dispose();
                    m_aborted.Dispose();
                    m_event.Dispose();
                }
            }

            internal void Abort(UpToDateFile upToDateFile)
            {
                lock (m_modified)
                {
                    m_modified.Remove(upToDateFile);
                }
                lock (m_deleted)
                {
                    m_deleted.Remove(upToDateFile);
                }
            }

            internal void FileChanged(UpToDateFile upToDateFile)
            {
                lock (m_modified)
                {
                    m_modified.Add(upToDateFile);
                }
                m_event.Set();
            }

            internal void FileDeleted(UpToDateFile upToDateFile)
            {
                lock (m_deleted)
                {
                    m_deleted.Add(upToDateFile);
                }
                m_event.Set();
            }
        }


        public FileInfo File { get { return m_file; } }

        private FileSystemWatcher m_watcher;
        private FileInfo m_file;
        private MemoryStream m_onDisk;
        private object m_onDiskAccess = new object();
        private Action<Stream> m_saveTo;
        public event Action FileChanged; //A change was made to the file externally
        public event Action FileDeleted; //The file was deleted
        private Backend m_backend;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lastSavedOrLoaded">Represents the current contents of the file. Reference is not held. A copy is made.</param>
        /// <param name="file"></param>
        /// <param name="saveTo"></param>
        public UpToDateFile(MemoryStream lastSavedOrLoaded, FileInfo file, Action<Stream> saveTo, Backend backend)
        {
            m_file = file;
            m_saveTo = saveTo;
            m_backend = backend;

            //Make a copy of the input data to make sure we're the only ones that can access it.
            m_onDisk = new MemoryStream((int)lastSavedOrLoaded.Length);
            lastSavedOrLoaded.CopyTo(m_onDisk);

            m_watcher = new FileSystemWatcher();
            m_watcher.Path = m_file.Directory.FullName;
            m_watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastAccess;
            m_watcher.Filter = m_file.Name;
            m_watcher.Changed += m_watcher_Changed;
            m_watcher.Deleted += m_watcher_Changed;
            m_watcher.Renamed += m_watcher_Changed;
            m_watcher.EnableRaisingEvents = true;
        }

        void m_watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed)
                m_backend.FileChanged(this);
            else if (e.ChangeType == WatcherChangeTypes.Deleted)
                m_backend.FileDeleted(this);
        }

        void NotifyFileDeleted()
        {
            FileDeleted.Execute();
        }

        void NotifyFileModified()
        {
            ThreadPool.QueueUserWorkItem(a => FileChanged.Execute()); //Queue the changes on a thread so this thread can be aborted due to a dispose triggered from the callback
        }

        bool? HasChanged()
        {
            try
            {
                using (var stream = Util.LoadFileStream(m_file, FileMode.Open, FileAccess.Read, 0))
                {
                    int length = (int)stream.Length;
                    if (length > 100e6)
                        MessageBox.Show("Attempting to load more than 100MB from a file. This is probably why we get the out of memory exception");
                    lock (m_onDiskAccess)
                    {
                        return FileSystem.ChangeIfDifferent(ref m_onDisk, stream, m_backend.AbortHandle);
                    }
                }
            }
            catch (MyFileLoadException)
            {
            }
            return null;
        }

        public void Save()
        {
            using (FileStream file = Util.LoadFileStream(m_file, FileMode.OpenOrCreate, FileAccess.Write, 10))
            {
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_watcher.Dispose();
                if (m_onDisk != null)
                    m_onDisk.Dispose();
            }
        }

        public MemoryStream Migrate()
        {
            m_watcher.Dispose();
            m_backend.Abort(this);
            MemoryStream temp = m_onDisk;
            m_onDisk = null;
            this.Dispose();
            return temp;
        }
    }

}

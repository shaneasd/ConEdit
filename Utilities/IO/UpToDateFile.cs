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
            m_saveTo = saveTo;

            //Make a copy of the input data to make sure we're the only ones that can access it.
            m_onDisk = new MemoryStream((int)lastSavedOrLoaded.Length);
            lastSavedOrLoaded.CopyTo(m_onDisk);

            m_abort = new ManualResetEventSlim(false);
            m_reopen = new AutoResetEvent(false);
            m_deleted = new ManualResetEventSlim(false);
            m_aborted = new ManualResetEventSlim(false);
            m_worker = new Thread(UpdateThread);
            m_worker.IsBackground = true;
            m_worker.Start();
            m_watcher = new FileSystemWatcher();
            m_watcher.Path = m_file.Directory.FullName;
            m_watcher.NotifyFilter = NotifyFilters.LastWrite;
            m_watcher.Filter = m_file.Name;
            m_watcher.Changed += m_watcher_Changed;
            m_watcher.Deleted += m_watcher_Changed;
            m_watcher.Renamed += m_watcher_Changed;
            m_watcher.EnableRaisingEvents = true;
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
                    WaitHandle.WaitAny(new WaitHandle[] { m_abort.WaitHandle, m_deleted.WaitHandle, m_reopen });
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
                            //Essentially wait 1000ms before trying the file but if we get terminated in the mean time just abort
                            if (m_abort.Wait(1000))
                                return;

                            try
                            {
                                using (var stream = Util.LoadFileStream(m_file, FileMode.Open, FileAccess.Read, 0))
                                {
                                    int length = (int)stream.Length;
                                    if (length > 100e6)
                                        MessageBox.Show("Attempting to load more than 100MB from a file. This is probably why we get the out of memory exception");
                                    lock (m_onDiskAccess)
                                    {
                                        fileChanged = FileSystem.ChangeIfDifferent(ref m_onDisk, stream, m_abort.WaitHandle);

                                        //TODO: Remove this. It's purely for debugging
                                        if (fileChanged)
                                        {
                                            //FileSystem.ChangeIfDifferent(ref m_onDisk, stream, m_abort.WaitHandle);
                                        }
                                        break;
                                    }
                                }
                            }
                            catch (MyFileLoadException)
                            {
                            }
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
            using (FileStream file = Util.LoadFileStream(m_file, FileMode.OpenOrCreate, FileAccess.Write, 10))
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_abort.Set();
                m_aborted.Wait(1000); //Wait a second. If we were unlucky and in the middle of a stream comparison it could take a while.
                m_watcher.Dispose();
                m_reopen.Dispose();
                if (m_onDisk != null)
                    m_onDisk.Dispose();
                m_abort.Dispose();
                m_aborted.Dispose();
                m_deleted.Dispose();
            }
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

}

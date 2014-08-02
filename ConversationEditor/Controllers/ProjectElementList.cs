using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.IO;
using System.Windows.Forms;

namespace ConversationEditor
{
    public interface IInProject
    {
        void Removed();
    }

    public enum Change { Add, Remove, Move }

    public interface IProjectElementList<out TReal, TInterface> : IEnumerable<TInterface>
        where TReal : TInterface
    {
        IEnumerable<TInterface> Load(IEnumerable<FileInfo> fileInfos);
        void Reload();
        TReal New(DirectoryInfo path);
        void Remove(TInterface element, bool force);
        void Delete(TInterface element);
        bool FileLocationOk(string path);

        event Action<TInterface> Added;
        event Action<TInterface> Removed;
        event Action<TInterface, TInterface> Reloaded;
        event Action GotChanged;
    }

    public class DummyProjectElementList<TReal, TInterface> : IProjectElementList<TReal, TInterface> where TReal : TInterface
    {
        public static readonly DummyProjectElementList<TReal, TInterface> Instance = new DummyProjectElementList<TReal, TInterface>();

        public IEnumerable<TInterface> Load(IEnumerable<FileInfo> fileInfos)
        {
            throw new NotImplementedException();
        }

        public void Reload()
        {
            throw new NotImplementedException();
        }

        public TReal New(DirectoryInfo path)
        {
            throw new NotImplementedException();
        }

        public void Remove(TInterface element, bool force)
        {
            throw new NotImplementedException();
        }

        public void Delete(TInterface element)
        {
            throw new NotImplementedException();
        }

        public bool FileLocationOk(string path)
        {
            throw new NotImplementedException();
        }

        public event Action<TInterface> Added { add { } remove { } }

        public event Action<TInterface> Removed { add { } remove { } }

        public event Action<TInterface, TInterface> Reloaded { add { } remove { } }

        public event Action GotChanged { add { } remove { } }

        public IEnumerator<TInterface> GetEnumerator()
        {
            return Enumerable.Empty<TInterface>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Enumerable.Empty<TInterface>().GetEnumerator();
        }
    }

    public static class ProjectElementList
    {
        public class Error
        {
            public readonly string Message;
            public Error(string message)
            {
                Message = message;
            }
        }
    }

    public class ProjectElementList<TReal, TMissing, TInterface> : IProjectElementList<TReal, TInterface>
        where TReal : TInterface
        where TMissing : TInterface
        where TInterface : class, ISaveableFileProvider, IInProject
    {
        public CallbackList<TInterface> m_data;
        private Func<IEnumerable<FileInfo>, IEnumerable<Or<TReal, TMissing>>> m_loader;
        private Func<FileInfo, TMissing> m_makeMissing;
        private Func<DirectoryInfo, TReal> m_makeEmpty;
        private Func<string, bool> m_fileLocationOk;

        public event Action<TInterface> Added;
        public event Action<TInterface> Removed;
        public event Action<TInterface, TInterface> Reloaded;
        public event Action GotChanged;

        public bool FileLocationOk(string path) { return m_fileLocationOk(path); }

        static Func<IEnumerable<FileInfo>, IEnumerable<Or<TReal, TMissing>>> MyLoader(Func<IEnumerable<FileInfo>, IEnumerable<TReal>> loader, Func<FileInfo, TMissing> makeMissing)
        {
            return files =>
                {
                    List<Or<TReal, TMissing>> result = new List<Or<TReal, TMissing>>();
                    foreach (var file in files)
                    {
                        if (file.Exists)
                        {
                            try
                            {
                                var doc = loader(file.Only()).Single();
                                result.Add(doc);
                            }
                            catch (FileLoadException e)
                            {
                                Console.Out.WriteLine(e.Message);
                                Console.Out.WriteLine(e.StackTrace);
                                Console.Out.WriteLine(e.InnerException.Message);
                                Console.Out.WriteLine(e.InnerException.StackTrace);
                                MessageBox.Show("File: " + file.Name + " exists but could not be accessed");
                                var doc = makeMissing(file);
                                result.Add(doc);
                            }
                        }
                        else
                        {
                            var doc = makeMissing(file);
                            result.Add(doc);
                        }
                    }
                    return result;
                };
        }

        public ProjectElementList(Func<string, bool> fileLocationOk, Func<IEnumerable<FileInfo>, IEnumerable<TReal>> loader, Func<DirectoryInfo, TReal> makeEmpty, Func<FileInfo, TMissing> makeMissing)
            : this(fileLocationOk, MyLoader(loader, makeMissing), makeEmpty, makeMissing)
        {
        }

        public ProjectElementList(Func<string, bool> fileLocationOk, Func<IEnumerable<FileInfo>, IEnumerable<Or<TReal, TMissing>>> loader, Func<DirectoryInfo, TReal> makeEmpty, Func<FileInfo, TMissing> makeMissing)
        {
            m_data = new CallbackList<TInterface>();
            m_data.Removing += (element) => { element.Removed(); };
            m_data.Clearing += () => { m_data.ForAll(element => { element.Removed(); }); };
            m_loader = loader;
            m_makeEmpty = makeEmpty;
            m_makeMissing = makeMissing;
            m_fileLocationOk = fileLocationOk;

            Added += a => GotChanged.Execute();
            Removed += a => GotChanged.Execute();
        }

        public IEnumerator<TInterface> GetEnumerator()
        {
            return m_data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<TInterface> Load(IEnumerable<FileInfo> fileInfos)
        {
            List<Tuple<FileInfo, TInterface>> toLoad = new List<Tuple<FileInfo, TInterface>>();
            foreach (var fileInfo in fileInfos)
            {
                if (!FileLocationOk(fileInfo.FullName))
                    throw new Exception("Attempting to load file that is not in a subfolder of the project's parent folder");

                var existing = m_data.FirstOrDefault(e => e.File.File.FullName == fileInfo.FullName);
                if (existing != null)
                {
                    try
                    {
                        if (existing.File.CanClose()) //TODO: If the file is modified and you try to load the same file over the top what happens? The message is probably unintuitive
                        {
                            m_data.Remove(existing); //TODO: Removing a domain file should alter the domain
                            toLoad.Add(Tuple.Create(fileInfo, existing));
                        }
                        else
                        {
                            //Don't try to load it
                        }
                    }
                    catch (FileLoadException e)
                    {
                        Console.Out.WriteLine(e.Message);
                        Console.Out.WriteLine(e.StackTrace);
                        Console.Out.WriteLine(e.InnerException.Message);
                        Console.Out.WriteLine(e.InnerException.StackTrace);
                        MessageBox.Show("Failed to access " + existing.File.File.FullName + " for saving");
                        //Don't try to load it
                    }
                }
                else
                {
                    toLoad.Add(new Tuple<FileInfo, TInterface>(fileInfo, null));
                }
            }

            List<TInterface> result = m_loader(toLoad.Select(t => t.Item1)).Select(o => o.Transformed<TInterface>(a => a, a => a)).ToList();

            for (int i = 0; i < result.Count; i++)
            {
                var conversation = result[i];
                var existing = toLoad[i].Item2;
                m_data.Add(conversation);
                if (existing != null)
                    Reloaded.Execute(existing, conversation);
                else
                    Added.Execute(conversation);
            }

            return this;
        }

        public void Reload()
        {
            List<Tuple<FileInfo, TInterface>> toLoad = new List<Tuple<FileInfo, TInterface>>();
            foreach (var doc in m_data)
            {
                toLoad.Add(Tuple.Create(doc.File.File, doc));
            }

            List<TInterface> result = m_loader(toLoad.Select(t => t.Item1)).Select(o => o.Transformed<TInterface>(a => a, a => a)).ToList();
            for (int i = 0; i < result.Count; i++)
            {
                var conversation = result[i];
                var existing = toLoad[i].Item2;
                m_data.Add(conversation);
                Reloaded.Execute(existing, conversation);
                m_data[i] = conversation;
            }
        }

        public TReal New(DirectoryInfo path)
        {
            TReal conversation = m_makeEmpty(path);
            m_data.Add(conversation);
            Added.Execute(conversation);
            return conversation;
        }

        public void Remove(TInterface element, bool force)
        {
            try
            {
                if (force || element.File.CanClose())
                {
                    m_data.Remove(element);
                    Removed.Execute(element);
                    element.Dispose();
                }
            }
            catch (FileLoadException e)
            {
                Console.Out.WriteLine(e.Message);
                Console.Out.WriteLine(e.StackTrace);
                Console.Out.WriteLine(e.InnerException.Message);
                Console.Out.WriteLine(e.InnerException.StackTrace);
                MessageBox.Show("Failed to access " + element.File.File.FullName + " for saving");
            }
        }

        public void Delete(TInterface element)
        {
            try
            {
                element.File.File.Delete();
            }
            catch (System.IO.IOException)
            {
                MessageBox.Show("Failed to delete file");
            }
            catch (System.Security.SecurityException)
            {
                MessageBox.Show("Failed to delete file");
            }
            m_data.Remove(element);
            Removed(element);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;

namespace ConversationEditor
{
    internal class ProjectElementList<TReal, TMissing, TInterface> : IProjectElementList<TReal, TInterface>
        where TReal : TInterface
        where TMissing : TInterface
        where TInterface : class, ISaveableFileProvider, IInProject
    {
        public CallbackDictionary<string, TInterface> m_data;
        private Func<IEnumerable<FileInfo>, IEnumerable<Either<TReal, TMissing>>> m_loader;
        private Func<DirectoryInfo, TReal> m_makeEmpty;
        private Func<string, bool> m_fileLocationOk;
        private SuppressibleAction m_suppressibleGotChanged;

        public event Action<TInterface> Added;
        public event Action<TInterface> Removed;
        public event Action<TInterface, TInterface> Reloaded;
        public event Action GotChanged;

        public ProjectElementList(Func<string, bool> fileLocationOk, Func<IEnumerable<FileInfo>, IEnumerable<TReal>> loader, Func<DirectoryInfo, TReal> makeEmpty, Func<FileInfo, TMissing> makeMissing)
            : this(fileLocationOk, MyLoader(loader, makeMissing), makeEmpty)
        {
        }

        public ProjectElementList(Func<string, bool> fileLocationOk, Func<IEnumerable<FileInfo>, IEnumerable<Either<TReal, TMissing>>> loader, Func<DirectoryInfo, TReal> makeEmpty)
        {
            m_data = new CallbackDictionary<string, TInterface>();
            m_data.Removing += (key, element) => { element.Removed(); };
            m_data.Clearing += () => { m_data.Values.ForAll(element => { element.Removed(); }); };
            m_loader = loader;
            m_makeEmpty = makeEmpty;
            m_fileLocationOk = fileLocationOk;

            m_suppressibleGotChanged = new SuppressibleAction(() => { GotChanged.Execute(); });
            Added += a => m_suppressibleGotChanged.TryExecute();
            Removed += a => m_suppressibleGotChanged.TryExecute();
        }

        public bool FileLocationOk(string path)
        {
            return m_fileLocationOk(path);
        }

        static Func<IEnumerable<FileInfo>, IEnumerable<Either<TReal, TMissing>>> MyLoader(Func<IEnumerable<FileInfo>, IEnumerable<TReal>> loader, Func<FileInfo, TMissing> makeMissing)
        {
            return files =>
                {
                    List<Either<TReal, TMissing>> result = new List<Either<TReal, TMissing>>();
                    foreach (var file in files)
                    {
                        if (file.Exists)
                        {
                            try
                            {
                                var doc = loader(file.Only()).Single();
                                result.Add(doc);
                            }
                            catch (MyFileLoadException e)
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

        public IEnumerator<TInterface> GetEnumerator()
        {
            return m_data.Values.GetEnumerator();
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
                    throw new InvalidOperationException("Attempting to load file that is not in a subfolder of the project's parent folder");

                //var existing = m_data.FirstOrDefault(e => e.File.File.FullName == fileInfo.FullName);
                var existing = m_data.ContainsKey(fileInfo.FullName) ? m_data[fileInfo.FullName] : null;
                if (existing != null)
                {
                    try
                    {
                        if (existing.File.CanClose())
                        {
                            //Ignore the fact that the new file might be missing stuff the conversation needs
                            m_data.Remove(fileInfo.FullName); //Callback on the list informs the domain file it has been removed thereby triggering domain update
                            toLoad.Add(Tuple.Create(fileInfo, existing));
                        }
                        else
                        {
                            //Don't try to load it
                        }
                    }
                    catch (MyFileLoadException e)
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

            using (m_suppressibleGotChanged.SuppressCallback())
            {
                for (int i = 0; i < result.Count; i++)
                {
                    var conversation = result[i];
                    var existing = toLoad[i].Item2;
                    m_data[conversation.File.File.FullName] = conversation;
                    if (existing != null)
                        Reloaded.Execute(existing, conversation);
                    else
                        Added.Execute(conversation);
                }
            }

            return this;
        }

        public void Reload()
        {
            List<Tuple<FileInfo, TInterface>> toLoad = new List<Tuple<FileInfo, TInterface>>();
            foreach (var doc in m_data.Values)
            {
                toLoad.Add(Tuple.Create(doc.File.File, doc));
            }

            List<TInterface> result = m_loader(toLoad.Select(t => t.Item1)).Select(o => o.Transformed<TInterface>(a => a, a => a)).ToList();
            for (int i = 0; i < result.Count; i++)
            {
                var conversation = result[i];
                var existing = toLoad[i].Item2;
                m_data[conversation.File.File.FullName] = conversation;
                Reloaded.Execute(existing, conversation);
            }

            foreach (var dispose in toLoad.Select(v => v.Item2))
            {
                dispose.Dispose();
            }
        }

        public TReal New(DirectoryInfo path)
        {
            TReal conversation = m_makeEmpty(path);
            m_data.Add(conversation.File.File.FullName, conversation);
            Added.Execute(conversation);
            return conversation;
        }

        public static bool PromptUsedAudioRemoved()
        {
            var result = MessageBox.Show("This audio is currently referenced by nodes in loaded conversations. " +
                                         "It will be automatically reloaded next time the project is loaded unless those references are removed", "Ok to remove file?", MessageBoxButtons.OKCancel);
            return result == DialogResult.OK;
        }

        public void Remove(TInterface element, bool force)
        {
            if (force || element.File.CanClose())
            {
                Func<bool> prompt = element is IDomainFile ? (Func<bool>)GraphFile.PromptFileRemoved : (Func<bool>)PromptUsedAudioRemoved;
                if (force || element.CanRemove(prompt))
                {
                    m_data.Remove(element.File.File.FullName);
                    Removed.Execute(element);
                    element.Dispose();
                }
            }
        }

        public void Delete(TInterface element)
        {
            Func<bool> prompt = element is IDomainFile ? (Func<bool>)GraphFile.PromptFileRemoved : (Func<bool>)PromptUsedAudioRemoved;
            if (element.CanRemove(prompt))
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
                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("Failed to delete file");
                }
                m_data.Remove(element.File.File.FullName);
                Removed(element);
            }
        }

        internal void Rename(string from, string to)
        {
            if (m_data.ContainsKey(from))
            {
                var item = m_data[from];
                m_data.Remove(from);
                m_data.Add(to, item);
            }
        }
    }
}

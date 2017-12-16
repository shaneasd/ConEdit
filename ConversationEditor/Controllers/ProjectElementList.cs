using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using Conversation;

namespace ConversationEditor
{
    internal class ProjectElementList<TInterface> : IProjectElementList<TInterface>
        where TInterface : class, ISaveableFileProvider, IInProject
    {
        public CallbackDictionary<Id<FileInProject>, TInterface> m_data;
        private Func<IEnumerable<Tuple<Id<FileInProject>, DocumentPath>>, IEnumerable<TInterface>> m_loader;
        private Func<DirectoryInfo, TInterface> m_makeEmpty;
        private Func<string, bool> m_fileLocationOk;
        private GetFilePath m_getFilePath;
        private SuppressibleAction m_suppressibleGotChanged;

        public event Action<TInterface> Added;
        public event Action<TInterface> Removed;
        public event Action<TInterface, TInterface> Reloaded;
        public event Action GotChanged;

        public ProjectElementList(GetFilePath getFilePath, Func<string, bool> fileLocationOk, Func<IEnumerable<Tuple<Id<FileInProject>, DocumentPath>>, IEnumerable<TInterface>> loader, Func<DirectoryInfo, TInterface> makeEmpty, Func<Id<FileInProject>, TInterface> makeMissing)
            : this(getFilePath, fileLocationOk, MyLoader(getFilePath, loader, makeMissing), makeEmpty)
        {
        }

        public ProjectElementList(GetFilePath getFilePath, Func<string, bool> fileLocationOk, Func<IEnumerable<Tuple<Id<FileInProject>, DocumentPath>>, IEnumerable<TInterface>> loader, Func<DirectoryInfo, TInterface> makeEmpty)
        {
            m_getFilePath = getFilePath;
            m_data = new CallbackDictionary<Id<FileInProject>, TInterface>();
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

        static Func<IEnumerable<Tuple<Id<FileInProject>, DocumentPath>>, IEnumerable<TInterface>> MyLoader(GetFilePath getFilePath, Func<IEnumerable<Tuple<Id<FileInProject>, DocumentPath>>, IEnumerable<TInterface>> loader, Func<Id<FileInProject>, TInterface> makeMissing)
        {
            return files =>
                {
                    List<TInterface> result = new List<TInterface>();
                    foreach (var file in files)
                    {
                        if (file.Item2.Exists)
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
                                MessageBox.Show("File: " + file.Item2.AbsolutePath + " could not be accessed");
                                var doc = makeMissing(file.Item1);
                                result.Add(doc);
                            }
                        }
                        else
                        {
                            var doc = makeMissing(file.Item1);
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

        public IEnumerable<TInterface> Load(IEnumerable<DocumentPath> paths)
        {
            IEnumerable<Tuple<Id<FileInProject>, DocumentPath>> toLoad = paths.Select(path => Tuple.Create((Id<FileInProject>.New()), path)).ToList();
            return Load(toLoad);
        }

        public IEnumerable<TInterface> Load(IEnumerable<Id<FileInProject>> fileIds)
        {
            IEnumerable<Tuple<Id<FileInProject>, DocumentPath>> withFileIdsAndInfos = fileIds.Select(f => Tuple.Create(f, m_getFilePath(f)));

            return Load(withFileIdsAndInfos);
        }

        private IEnumerable<TInterface> Load(IEnumerable<Tuple<Id<FileInProject>, DocumentPath>> withFileIdsAndInfos)
        {
            List<Tuple<Id<FileInProject>, DocumentPath, TInterface>> toLoad = new List<Tuple<Id<FileInProject>, DocumentPath, TInterface>>();
            foreach (var fileInfo in withFileIdsAndInfos)
            {
                TryAddToLoadList(toLoad, fileInfo.Item1, fileInfo.Item2);
            }

            List<TInterface> result = m_loader(toLoad.Select(t => Tuple.Create(t.Item1, t.Item2))).ToList();

            using (m_suppressibleGotChanged.SuppressCallback())
            {
                for (int i = 0; i < result.Count; i++)
                {
                    var conversation = result[i];
                    TInterface existing = toLoad[i].Item3;
                    m_data[conversation.Id] = conversation;
                    if (existing != null)
                        Reloaded.Execute(existing, conversation);
                    else
                        Added.Execute(conversation);
                }
            }

            return this;
        }

        private void TryAddToLoadList(List<Tuple<Id<FileInProject>, DocumentPath, TInterface>> toLoad, Id<FileInProject> fileId, DocumentPath path)
        {
            if (!FileLocationOk(path.AbsolutePath))
                throw new InvalidOperationException("Attempting to load file that is not in a subfolder of the project's parent folder");

            //var existing = m_data.FirstOrDefault(e => e.File.File.FullName == fileInfo.FullName);
            var existing = m_data.ContainsKey(fileId) ? m_data[fileId] : null;
            if (existing != null)
            {
                try
                {
                    if (existing.File.CanClose())
                    {
                        //Ignore the fact that the new file might be missing stuff the conversation needs
                        m_data.Remove(fileId); //Callback on the list informs the domain file it has been removed thereby triggering domain update
                        toLoad.Add(Tuple.Create(fileId, path, existing));
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
                toLoad.Add(new Tuple<Id<FileInProject>, DocumentPath, TInterface>(fileId, path, null));
            }
        }

        public void Reload()
        {
            List<Tuple<Id<FileInProject>, DocumentPath, TInterface>> toLoad = new List<Tuple<Id<FileInProject>, DocumentPath, TInterface>>();
            foreach (var doc in m_data.Values)
            {
                toLoad.Add(Tuple.Create(doc.Id, m_getFilePath(doc.Id), doc));
            }

            List<TInterface> result = m_loader(toLoad.Select(t => Tuple.Create(t.Item1, t.Item2))).ToList();
            for (int i = 0; i < result.Count; i++)
            {
                var conversation = result[i];
                TInterface existing = toLoad[i].Item3;
                m_data[conversation.Id] = conversation;
                Reloaded.Execute(existing, conversation);
            }

            foreach (TInterface dispose in toLoad.Select(v => v.Item3))
            {
                dispose.Dispose();
            }
        }

        public TInterface New(DirectoryInfo path)
        {
            TInterface conversation = m_makeEmpty(path);
            m_data.Add(conversation.Id, conversation);
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
                    m_data.Remove(element.Id);
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
                bool success = m_data.Remove(element.Id);
                if (!success)
                    throw new InvalidOperationException("Failed to remove item from ProjectElementList");
                Removed(element);
            }
        }
    }
}

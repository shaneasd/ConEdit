using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Utilities;
using Conversation;
using System.Xml.Linq;
using System.Xml;
using Conversation.Serialization;
using System.Windows;

namespace ConversationEditor
{
    public class LocalizationFile : Disposable, ILocalizationFile
    {
        public delegate bool ShouldSaveQuery(Id<LocalizedText> guid);
        private LocalizerData m_data;
        SaveableFileExternalChangedSource m_file;
        HashSet<object> m_changesLastSave = new HashSet<object>();
        HashSet<object> m_currentChanges = new HashSet<object>();
        private UpToDateFile.BackEnd m_backend;

        public Id<FileInProject> Id { get; }

        private bool Changed()
        {
            return !m_changesLastSave.SetEquals(m_currentChanges);
        }

        private void Saved()
        {
            m_changesLastSave.Clear();
            m_changesLastSave.UnionWith(m_currentChanges);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialData">Represents the current contents of the file. Reference is not held. A copy is made.</param>
        /// <param name="path"></param>
        /// <param name="data"></param>
        /// <param name="serializer"></param>
        private LocalizationFile(Id<FileInProject> id, MemoryStream initialData, DocumentPath path, LocalizerData data, ISerializer<LocalizerData> serializer, UpToDateFile.BackEnd backend)
        {
            Id = id;
            m_backend = backend;
            m_file = new SaveableFileExternalChangedSource(initialData, path.FileInfo, s => { serializer.Write(m_data, s); }, Changed, Saved, backend);
            m_data = data;
        }

        public ISaveableFile File { get { return m_file; } }

        internal static LocalizationFile MakeNew(DirectoryInfo directory, Func<string, Id<FileInProject>, ISerializer<LocalizerData>> serializer, Func<FileInfo, bool> pathOk, UpToDateFile.BackEnd backend, DirectoryInfo origin)
        {
            //Create a stream under an available filename
            FileInfo path = null;
            for (int i = 0; path == null; i++)
            {
                path = new FileInfo(directory.FullName + Path.DirectorySeparatorChar + "New Localization " + i + ".loc");
                if (!pathOk(path))
                    path = null;
            }

            LocalizerData data = new LocalizerData();
            using (var file = Util.LoadFileStream(path, FileMode.CreateNew, FileAccess.Write))
            {
            }
            using (var mem = new MemoryStream())
            {
                var id = Id<FileInProject>.New();
                LocalizationFile result = new LocalizationFile(id, mem, DocumentPath.FromPath(path, origin), data, serializer(path.FullName, id), backend); //Make a new localization file for an existing project
                result.File.Writable.Save();
                return result;
            }
        }

        internal static ILocalizationFile Load(DocumentPath path, Id<FileInProject> id, ISerializer<LocalizerData> serializer, UpToDateFile.BackEnd backend)
        {
            if (path.Exists)
            {
                try
                {
                    LocalizerData data;
                    using (FileStream file = Util.LoadFileStream(path.AbsolutePath, FileMode.Open, FileAccess.Read))
                    {
                        using (MemoryStream m = new MemoryStream((int)file.Length))
                        {
                            file.CopyTo(m);
                            m.Position = 0;
                            XmlLocalization.Deserializer d = new XmlLocalization.Deserializer();
                            data = d.Read(m);
                            m.Position = 0;
                            LocalizationFile result = new LocalizationFile(id, m, path, data, serializer, backend);
                            return result;
                        }
                    }
                }
                catch (MyFileLoadException e)
                {
                    Console.Out.WriteLine(e.Message);
                    Console.Out.WriteLine(e.StackTrace);
                    Console.Out.WriteLine(e.InnerException.Message);
                    Console.Out.WriteLine(e.InnerException.StackTrace);
                    MessageBox.Show("File: " + path.AbsolutePath + " could not be accessed");
                    return new MissingLocalizationFile(id, path);
                }
            }
            else
            {
                return new MissingLocalizationFile(id, path);
            }
        }

        public bool CanRemove(Func<bool> prompt)
        {
            //Project does not require there to be a localization file
            return true;
        }

        public void Removed()
        {
            //Do nothing
        }

        public string Localize(Id<LocalizedText> id)
        {
            if (m_data.LocalizationExists(id))
                return m_data.GetLocalized(id).Text;
            else
                return null;
        }

        public DateTime LocalizationTime(Id<LocalizedText> id)
        {
            if (m_data.LocalizationExists(id))
                return m_data.GetLocalized(id).Localized;
            else
                return DateTime.MinValue;
        }

        public SimpleUndoPair SetLocalizationAction(Id<LocalizedText> guid, string p)
        {
            Either<LocalizationElement, Null> oldValue = Either.Create(m_data.LocalizationExists(guid), () => m_data.GetLocalized(guid), Null.Func);
            return InnerSetLocalizationAction(guid, new LocalizationElement(DateTime.Now, p), oldValue);
        }

        private SimpleUndoPair InnerSetLocalizationAction(Id<LocalizedText> guid, Either<LocalizationElement, Null> newValue, Either<LocalizationElement, Null> oldValue)
        {
            object change = new object();
            return new SimpleUndoPair
            {
                Redo = () =>
                {
                    newValue.Do(a => m_data.SetLocalized(guid, a), b => m_data.ClearLocaliation(guid));
                    m_currentChanges.Add(change);
                    m_file.Change();
                },
                Undo = () =>
                {
                    oldValue.Do(a => m_data.SetLocalized(guid, a), b => m_data.ClearLocaliation(guid));
                    m_currentChanges.Remove(change);
                    m_file.Change();
                },
            };
        }

        public SimpleUndoPair ClearLocalizationAction(Id<LocalizedText> guid)
        {
            Either<LocalizationElement, Null> oldValue = Either.Create(m_data.LocalizationExists(guid), () => m_data.GetLocalized(guid), Null.Func);
            return InnerSetLocalizationAction(guid, Null.Func(), oldValue);
        }

        public IEnumerable<Id<LocalizedText>> ExistingLocalizations { get { return m_data.AllLocalizations.Select(kvp => kvp.Key); } }

        public SimpleUndoPair DuplicateAction(Id<LocalizedText> guid, Id<LocalizedText> result)
        {
            object change = new object();
            LocalizationElement? value = m_data.LocalizationExists(guid) ? m_data.GetLocalized(guid) : (LocalizationElement?)null;

            return new SimpleUndoPair
            {
                Redo = () =>
                {
                    if (value.HasValue)
                    {
                        m_data.SetLocalized(result, value.Value);
                        m_currentChanges.Add(change);
                        m_file.Change();
                    }
                },
                Undo = () =>
                {
                    m_data.ClearLocaliation(result);
                    m_currentChanges.Add(change);
                    m_file.Change();
                },
            };
        }

        public event Action FileModifiedExternally
        {
            add { (this as ISaveableFileProvider).File.FileModifiedExternally += value; }
            remove { (this as ISaveableFileProvider).File.FileModifiedExternally -= value; }
        }

        public event Action FileDeletedExternally
        {
            add { (this as ISaveableFileProvider).File.FileDeletedExternally += value; }
            remove { (this as ISaveableFileProvider).File.FileDeletedExternally -= value; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_file.Dispose();
            }
        }

        public void ImportInto(string[] fileNames, DirectoryInfo origin)
        {
            var localizations = fileNames.Select(path => new { Path = path, Localization = Load(DocumentPath.FromPath(path, origin), Id<FileInProject>.New(), null, m_backend) });

            string message = "";

            var keys = m_data.AllLocalizations.Select(kvp => kvp.Key);
            foreach (var localization in localizations)
            {
                foreach (var key in localization.Localization.ExistingLocalizations)
                {
                    if (keys.Contains(key))
                        message += "Trying to import localization for " + key.Serialized() + " from " + localization.Path + " which already exists in the destination\n";
                }
            }

            Dictionary<Id<LocalizedText>, string> paths = new Dictionary<Id<LocalizedText>, string>();
            foreach (var loc in localizations)
            {
                foreach (var key in loc.Localization.ExistingLocalizations)
                {
                    if (paths.ContainsKey(key))
                        message += "Trying to import localization for " + key.Serialized() + " from " + loc.Path + " and from " + paths[key] + "\n";
                    else
                        paths[key] = loc.Path;
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                MessageBox.Show(message);
            }
            else
            {
                foreach (var loc in localizations)
                {
                    foreach (var key in loc.Localization.ExistingLocalizations)
                    {
                        SetLocalizationAction(key, loc.Localization.Localize(key)).Redo();
                    }
                }
            }
        }

        public bool IsValid
        {
            get { return true; }
        }
    }
}

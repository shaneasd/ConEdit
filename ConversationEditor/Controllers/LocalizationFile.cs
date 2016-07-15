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

namespace ConversationEditor
{
    internal class LocalizationFile : Disposable, ILocalizationFile
    {
        public delegate bool ShouldSaveQuery(Id<LocalizedText> guid);
        private LocalizerData m_data;
        SaveableFileExternalChangedSource m_file;
        HashSet<object> m_changesLastSave = new HashSet<object>();
        HashSet<object> m_currentChanges = new HashSet<object>();

        private bool Changed()
        {
            return !m_changesLastSave.SetEquals(m_currentChanges);
        }

        private void Saved()
        {
            m_changesLastSave.Clear();
            m_changesLastSave.UnionWith(m_currentChanges);
        }

        private LocalizationFile(MemoryStream initialData, FileInfo path, LocalizerData data, ISerializer<LocalizerData> serializer)
        {
            m_file = new SaveableFileExternalChangedSource(initialData, path, s => { serializer.Write(m_data, s); }, Changed, Saved);
            m_data = data;
        }

        public ISaveableFile File { get { return m_file; } }

        internal static LocalizationFile MakeNew(DirectoryInfo directory, Func<string, ISerializer<LocalizerData>> serializer, Func<FileInfo, bool> pathOk)
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
                file.Close();
            }
            LocalizationFile result = new LocalizationFile(new MemoryStream(), path, data, serializer(path.FullName)); //Make a new localization file for an existing project
            result.File.Writable.Save();
            return result;
        }

        internal static LocalizationFile Load(FileInfo path, ISerializer<LocalizerData> serializer)
        {
            LocalizerData data;
            using (FileStream file = Util.LoadFileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (MemoryStream m = new MemoryStream((int)file.Length))
                {
                    file.CopyTo(m);
                    m.Position = 0;
                    XmlLocalization.Deserializer d = new XmlLocalization.Deserializer();
                    data = d.Read(m);
                    m.Position = 0;
                    LocalizationFile result = new LocalizationFile(m, path, data, serializer);
                    return result;
                }
            }
        }

        public bool Contains(Id<LocalizedText> guid)
        {
            return m_data.CanLocalize(guid);
        }

        public bool CanRemove(Func<bool> prompt)
        {
            //Project does not require there to be a localization file
            return true;
        }

        void IInProject.Removed()
        {
            //Do nothing
        }

        public string Localize(Id<LocalizedText> guid)
        {
            if (m_data.CanLocalize(guid))
                return m_data.GetLocalized(guid).Text;
            else
                return null;
        }

        public SimpleUndoPair SetLocalizationAction(Id<LocalizedText> guid, string p)
        {
            Either<LocalizationElement, Null> oldValue = Either.Create(m_data.CanLocalize(guid), () => m_data.GetLocalized(guid), Null.Func);
            object change = new object();
            return new SimpleUndoPair
            {
                Redo = () =>
                {
                    m_data.SetLocalized(guid, new LocalizationElement(DateTime.Now, p));
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

        public SimpleUndoPair DuplicateAction(Id<LocalizedText> guid, Id<LocalizedText> result)
        {
            object change = new object();
            return new SimpleUndoPair
            {
                Redo = () =>
                {
                    if (m_data.CanLocalize(guid))
                    {
                        m_data.SetLocalized(result, m_data.GetLocalized(guid));
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

        public bool IsValid
        {
            get { return true; }
        }
    }
}

﻿using System;
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
    public interface ILocalizationFile : IInProject, ISaveableFileProvider
    {
        string Localize(ID<LocalizedText> id);
        SimpleUndoPair SetLocalizationAction(ID<LocalizedText> guid, string p);
    }

    public class LocalizationFile : ILocalizationFile
    {
        public delegate bool ShouldSaveQuery(ID<LocalizedText> guid);
        private LocalizerData m_data;

        private LocalizationFile(FileInfo path, LocalizerData data, ISerializer<LocalizerData> serializer)
        {
            m_file = new SaveableFileNotUndoable(path, s => { serializer.Write(m_data, s); });
            m_data = data;
        }

        SaveableFileNotUndoable m_file;
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
            Util.LoadFileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None).Close();
            LocalizationFile result = new LocalizationFile(path, data, serializer(path.FullName)); //Make a new localization file for an existing project
            result.File.Save();
            return result;
        }

        internal static LocalizationFile Load(FileInfo path, ISerializer<LocalizerData> serializer)
        {
            LocalizerData data;
            using (FileStream file = Util.LoadFileStream(path, FileMode.Open, FileAccess.Read))
            {
                XmlLocalization.Deserializer d = new XmlLocalization.Deserializer();
                data = d.Read(file);
            }
            LocalizationFile result = new LocalizationFile(path, data, serializer);
            return result;
        }

        public bool Contains(ID<LocalizedText> guid)
        {
            return m_data.m_data.ContainsKey(guid);
        }

        public void Removed()
        {
            //Doesn't care
        }

        public string Localize(ID<LocalizedText> guid)
        {
            if (m_data.m_data.ContainsKey(guid))
                return m_data.m_data[guid].Text;
            else
                return null;
        }

        public SimpleUndoPair SetLocalizationAction(ID<LocalizedText> guid, string p)
        {
            Or<LocalizationElement, Null> oldValue = Or.Create(m_data.m_data.ContainsKey(guid), () => m_data.m_data[guid], Null.Func);
            return new SimpleUndoPair
            {
                Redo = () => { m_data.m_data[guid] = new LocalizationElement(DateTime.Now, p); m_file.Change(); }, //TODO: Should I bother making the file change undoable?
                Undo = () => { oldValue.Do(a => m_data.m_data[guid] = a, b => m_data.m_data.Remove(guid)); m_file.Change(); },
            };
        }

        public SimpleUndoPair DuplicateAction(ID<LocalizedText> guid, ID<LocalizedText> result)
        {
            return new SimpleUndoPair
            {
                Redo = () =>
                {
                    if (m_data.m_data.ContainsKey(guid))
                    {
                        m_data.m_data[result] = m_data.m_data[guid];
                        m_file.Change();
                    }
                },
                Undo = () =>
                {
                    m_data.m_data.Remove(result);
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

        public void Dispose()
        {
            File.Dispose();
        }
    }
}

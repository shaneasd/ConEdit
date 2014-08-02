using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using ConversationEditor;
using System.IO;
using Utilities;
using Conversation.Serialization;

namespace ConversationEditor
{
    public class LocalizationEngine
    {
        ProjectElementList<LocalizationFile, MissingLocalizationFile, ILocalizationFile> m_localizers;
        ILocalizationFile m_currentLocalization = null;

        private Func<HashSet<ID<LocalizedText>>> m_usedGuids;
        public readonly Func<string, bool> ShouldClean;
        public readonly Func<string, bool> ShouldExpand;

        /// <param name="usedGuids"></param>
        /// <param name="shouldClean"></param>
        /// <param name="shouldExpand"></param>
        /// <param name="pathOk">Path is an acceptable filename for a new localization file</param>
        /// <param name="fileLocationOk">Path is an acceptable location from which to import an existing localization file</param>
        public LocalizationEngine(Func<HashSet<ID<LocalizedText>>> usedGuids, Func<string, bool> shouldClean, Func<string, bool> shouldExpand, Func<FileInfo, bool> pathOk, Func<string, bool> fileLocationOk)
        {
            Func<IEnumerable<FileInfo>, IEnumerable<LocalizationFile>> load = files => files.Select(file => LocalizationFile.Load(file, MakeSerializer(file.Name)));
            Func<DirectoryInfo, LocalizationFile> makeEmpty = path => LocalizationFile.MakeNew(path, MakeSerializer, pathOk);
            Func<FileInfo, MissingLocalizationFile> makeMissing = file => new MissingLocalizationFile(file);
            m_localizers = new ProjectElementList<LocalizationFile, MissingLocalizationFile, ILocalizationFile>(fileLocationOk, load, makeEmpty, makeMissing);
            m_usedGuids = usedGuids;
            ShouldClean = shouldClean;
            ShouldExpand = shouldExpand;
        }

        public IProjectElementList<LocalizationFile, ILocalizationFile> Localizers { get { return m_localizers; } }

        internal ISerializer<LocalizerData> MakeSerializer(string file)
        {
            return new XmlLocalization.Serializer(GuidUsed, m_usedGuids, ShouldClean, ShouldExpand, Localize, file);
        }

        public string Localize(ID<LocalizedText> guid)
        {
            return m_currentLocalization.Localize(guid);
        }

        internal Tuple<ID<LocalizedText>, SimpleUndoPair> DuplicateActions(ID<LocalizedText> guid)
        {
            ID<LocalizedText> result = new ID<LocalizedText>();
            List<SimpleUndoPair> actions = new List<SimpleUndoPair>();
            foreach (var loc in Localizers.OfType<LocalizationFile>())
            {
                actions.Add(loc.DuplicateAction(guid, result));
            }
            return Tuple.Create(result, new SimpleUndoPair { Redo = ()=>actions.ForEach(a=>a.Redo()), Undo = ()=> actions.ForEach(a=>a.Undo())});
        }

        internal SimpleUndoPair SetLocalizationAction(ID<LocalizedText> guid, string value)
        {
            return m_currentLocalization.SetLocalizationAction(guid, value);
        }

        public IEnumerable<ID<LocalizedText>> UsedGuids
        {
            get
            {
                return m_usedGuids();
            }
        }

        public bool GuidUsed(ID<LocalizedText> guid)
        {
            return m_usedGuids().Contains(guid);
        }

        internal void SelectLocalizer(ILocalizationFile element)
        {
            m_currentLocalization = element;
        }
    }
}

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
    public class LocalizationEngine : ILocalizationEngine
    {
        public const string MISSING_LOCALIZATION = "Missing Localization";

        ProjectElementList<LocalizationFile, MissingLocalizationFile, ILocalizationFile> m_localizers;
        ILocalizationContext m_context;

        private Func<HashSet<Id<LocalizedText>>> m_usedGuids;
        public readonly Func<string, bool> ShouldClean;
        public readonly Func<string, bool> ShouldExpand;

        /// <param name="context">Context used when localizing to reference current localization</param>
        /// <param name="usedGuids"></param>
        /// <param name="shouldClean"></param>
        /// <param name="shouldExpand"></param>
        /// <param name="pathOk">Path is an acceptable filename for a new localization file</param>
        /// <param name="fileLocationOk">Path is an acceptable location from which to import an existing localization file</param>
        public LocalizationEngine(ILocalizationContext context, Func<HashSet<Id<LocalizedText>>> usedGuids, Func<string, bool> shouldClean, Func<string, bool> shouldExpand, Func<FileInfo, bool> pathOk, Func<string, bool> fileLocationOk, UpToDateFile.Backend backend)
        {
            m_context = context;
            m_usedGuids = usedGuids;
            ShouldClean = shouldClean;
            ShouldExpand = shouldExpand;

            Func<IEnumerable<FileInfo>, IEnumerable<Either<LocalizationFile, MissingLocalizationFile>>> load = files =>
            {
                return files.Select(file => LocalizationFile.Load(file, MakeSerializer(file.Name), backend));
                //return ParallelEnumerable.Select(files.AsParallel(), file => LocalizationFile.Load(file, MakeSerializer(file.Name), backend));
            };
            Func<DirectoryInfo, LocalizationFile> makeEmpty = path => LocalizationFile.MakeNew(path, MakeSerializer, pathOk, backend);
            m_localizers = new ProjectElementList<LocalizationFile, MissingLocalizationFile, ILocalizationFile>(fileLocationOk.BottleNeck(), load, makeEmpty);
            m_localizers.GotChanged += () =>
            {
                //If there is only one localizer then it must be the current localizer
                if (m_localizers.Count() == 1)
                    m_context.CurrentLocalization.Value = m_localizers.First();
                //If the current localizer is removed from the list of available localizers then select a replacement
                if (!m_localizers.Contains(m_context.CurrentLocalization.Value))
                    m_context.CurrentLocalization.Value = Project.SelectNewLocalizer(m_localizers);
            };
        }

        public IProjectElementList<LocalizationFile, ILocalizationFile> Localizers { get { return m_localizers; } }

        internal ISerializer<LocalizerData> MakeSerializer(string file)
        {
            Func<XmlLocalization.Context> context = () =>
             {
                 var used = m_usedGuids();
                 return new XmlLocalization.Context(used.Contains, used);
             };
            return new XmlLocalization.Serializer(context, ShouldClean, ShouldExpand, Localize, file);
        }

        public string Localize(Id<LocalizedText> guid)
        {
            if (guid != null)
                return m_context.CurrentLocalization.Value.Localize(guid) ?? MISSING_LOCALIZATION;
            else
                return MISSING_LOCALIZATION;
        }

        public Tuple<Id<LocalizedText>, SimpleUndoPair> DuplicateActions(Id<LocalizedText> guid)
        {
            Id<LocalizedText> result = Id<LocalizedText>.New();
            List<SimpleUndoPair> actions = new List<SimpleUndoPair>();
            foreach (var loc in Localizers.OfType<LocalizationFile>())
            {
                actions.Add(loc.DuplicateAction(guid, result));
            }
            return Tuple.Create(result, new SimpleUndoPair { Redo = () => actions.ForEach(a => a.Redo()), Undo = () => actions.ForEach(a => a.Undo()) });
        }

        public SimpleUndoPair SetLocalizationAction(Id<LocalizedText> guid, string value)
        {
            return m_context.CurrentLocalization.Value.SetLocalizationAction(guid, value);
        }

        public SimpleUndoPair ClearLocalizationAction(Id<LocalizedText> guid)
        {
            List<SimpleUndoPair> actions = new List<SimpleUndoPair>();
            foreach (var loc in Localizers.OfType<LocalizationFile>())
            {
                actions.Add(loc.ClearLocalizationAction(guid));
            }
            return new SimpleUndoPair { Redo = () => actions.ForEach(a => a.Redo()), Undo = () => actions.ForEach(a => a.Undo()) };
        }

        public bool CanLocalize
        {
            get { return m_localizers.Any(); } //Assume that if localizers exist then one is selected
        }

        internal void Rename(string from, string to)
        {
            m_localizers.Rename(from, to);
        }
    }
}

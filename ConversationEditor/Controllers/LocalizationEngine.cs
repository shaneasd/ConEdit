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
        public const string MissingLocalizationString = "Missing Localization";

        ProjectElementList<ILocalizationFile> m_localizers;
        HashSet<Project.TData.LocalizerSetData> m_localizationSets;
        ILocalizationContext m_context;

        private Func<HashSet<Id<LocalizedText>>> m_usedGuids;
        public Func<string, bool> ShouldClean { get; }

        /// <param name="context">Context used when localizing to reference current localization</param>
        /// <param name="usedGuids"></param>
        /// <param name="shouldClean"></param>
        /// <param name="shouldExpand"></param>
        /// <param name="pathOk">Path is an acceptable filename for a new localization file</param>
        /// <param name="fileLocationOk">Path is an acceptable location from which to import an existing localization file</param>
        public LocalizationEngine(GetFilePath getFilePath, IEnumerable<Project.TData.LocalizerSetData> sets, ILocalizationContext context, Func<HashSet<Id<LocalizedText>>> usedGuids, Func<string, bool> shouldClean, Func<FileInfo, bool> pathOk, Func<string, bool> fileLocationOk, UpToDateFile.BackEnd backend, DirectoryInfo origin)
        {
            m_context = context;
            m_usedGuids = usedGuids;
            ShouldClean = shouldClean;

            Func<IEnumerable<Tuple<Id<FileInProject>, DocumentPath>>, IEnumerable<ILocalizationFile>> load = files =>
            {
                //return files.Select(file => LocalizationFile.Load(file, MakeSerializer(file.Name), backend));
                var filesAndSerializer = files.Select(f => new { Id = f.Item1, Path = f.Item2, Serializer = MakeSerializer(f.Item2.AbsolutePath, f.Item1) }).ToList();
                return ParallelEnumerable.Select(filesAndSerializer.AsParallel(), fs => LocalizationFile.Load(fs.Path, fs.Id, fs.Serializer, backend));
            };
            Func<DirectoryInfo, LocalizationFile> makeEmpty = path => LocalizationFile.MakeNew(path, MakeSerializer, pathOk, backend, origin);
            m_localizers = new ProjectElementList<ILocalizationFile>(getFilePath, fileLocationOk.Bottleneck(), load, makeEmpty);
            m_localizationSets = sets.ToHashSet();
        }

        public IProjectElementList<ILocalizationFile> Localizers { get { return m_localizers; } }

        public HashSet<Project.TData.LocalizerSetData> LocalizationSets { get { return m_localizationSets; } }

        internal ISerializer<LocalizerData> MakeSerializer(string file, Id<FileInProject> id)
        {
            Func<XmlLocalization.Context> context = () =>
             {
                 var used = m_usedGuids();
                 return new XmlLocalization.Context(used.Contains, used);
             };
            return new XmlLocalization.Serializer(context, ShouldClean, textId => Localize(Id<LocalizedStringType>.FromGuid(id.Guid), textId), file);
        }

        public string Localize(Id<LocalizedStringType> type, Id<LocalizedText> guid)
        {
            if (type != null && guid != null && m_context.CurrentLocalization.Value != null)
                return Lookup(m_context.CurrentLocalization.Value.Sources[type])?.Localize(guid) ?? MissingLocalizationString;
            else
                return MissingLocalizationString;
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

        public SimpleUndoPair SetLocalizationAction(Id<LocalizedStringType> type, Id<LocalizedText> guid, string value)
        {
            return Lookup(type).SetLocalizationAction(guid, value);
        }

        public SimpleUndoPair ClearLocalizationAction(Id<LocalizedStringType> type, Id<LocalizedText> guid)
        {
            List<SimpleUndoPair> actions = new List<SimpleUndoPair>();
            foreach (var loc in Localizers.OfType<LocalizationFile>())
            {
                actions.Add(loc.ClearLocalizationAction(guid));
            }
            return new SimpleUndoPair { Redo = () => actions.ForEach(a => a.Redo()), Undo = () => actions.ForEach(a => a.Undo()) };
        }

        public bool CanLocalize(Id<LocalizedStringType> localizedStringType)
        {
            var localizer = Lookup(localizedStringType);
            return localizer != null;
        }

        public IEnumerable<Tuple<Id<LocalizedStringType>, Id<LocalizedText>>> ExistingLocalizations
        {
            get
            {
                foreach (var fileId in m_context.CurrentLocalization.Value.Sources.Values)
                {
                    var file = Lookup(fileId);
                    if (file != null)
                        foreach (var textId in file.ExistingLocalizations)
                            yield return Tuple.Create(Id<LocalizedStringType>.ConvertFrom(fileId), textId);
                }
            }
        }

        //TODO: LOC: This may need to be faster
        private ILocalizationFile Lookup(Id<FileInProject> fileId)
        {
            return m_localizers.FirstOrDefault(x => x.Id == fileId);
        }

        private ILocalizationFile Lookup(Id<LocalizedStringType> type)
        {
            Id<FileInProject> fileId;
            if (m_context.CurrentLocalization.Value.Sources.TryGetValue(type, out fileId))
                return Lookup(fileId);
            else
                return null;
        }

        /// <summary>
        /// Will the current localizer set localize to real user specified data?
        /// </summary>
        public bool IsValid
        {
            get
            {
                return m_context.CurrentLocalization.Value.Sources.Values.All(fileId =>
                {
                    var file = Lookup(fileId);
                    return file != null && file.IsValid;
                });
            }
        }

        internal DateTime LocalizationTime(Id<LocalizedStringType> type, Id<LocalizedText> id)
        {
            var file = Lookup(type);
            return file?.LocalizationTime(id) ?? DateTime.MinValue;
        }

        internal void UpdateLocalizationSets(Project.TData.LocalizerSetData[] sets)
        {
            var currentSelectionId = m_context.CurrentLocalization.Value?.Id;
            m_localizationSets = sets.ToHashSet();
            m_context.CurrentLocalization.Value = m_localizationSets.FirstOrDefault(s => s.Id == currentSelectionId)
                                                  ?? m_localizationSets.FirstOrDefault()
                                                  ?? Project.TData.LocalizerSetData.Empty;
            LocalizationSetsChanged.Execute();
        }

        public event Action LocalizationSetsChanged;
    }
}

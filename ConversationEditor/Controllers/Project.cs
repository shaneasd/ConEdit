using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Utilities;
using Conversation;
using Conversation.Serialization;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGui>;
    //using TData = Tuple<IEnumerable<string>, IEnumerable<string>, IEnumerable<string>>;
    using TConversationData = XmlGraphData<NodeUIData, ConversationEditorData>;
    using TDomainData = XmlGraphData<NodeUIData, ConversationEditorData>;
    using TConversationSerializerDeserializer = ISerializerDeserializer<Tuple<IEnumerable<ConversationNode<INodeGui>>, ConversationEditorData>>;
    using TDomainSerializerDeserializer = ISerializerDeserializer<Tuple<IEnumerable<ConversationNode<INodeGui>>, ConversationEditorData>>;
    using ConversationSerializerDeserializerFactory = Func<IDataSource, ISerializerDeserializer<XmlGraphData<NodeUIData, ConversationEditorData>>>;
    using DomainSerializerDeserializerFactory = Func<IDataSource, DomainSerializerDeserializer>;
    using System.Diagnostics;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [Serializable]
    public class ProjectCreationException : ApplicationException
    {
        public ProjectCreationException() : base()
        {
        }

        public ProjectCreationException(string message) : base(message)
        {
        }

        public ProjectCreationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ProjectCreationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class Project : Disposable, IProject, ISaveableFileProvider
    {
        public class TData
        {
            public struct FileIdAndPath
            {
                public Id<FileInProject> Id { get; }
                public DocumentPath Path { get; }
                public FileIdAndPath(Id<FileInProject> id, DocumentPath path)
                {
                    Id = id;
                    Path = path;
                }
            }

            public class LocalizerSetData
            {
                public LocalizerSetData(Id<LocalizerSetData> id, string name, IReadOnlyDictionary<Id<LocalizedStringType>, Id<FileInProject>> sources)
                {
                    Id = id;
                    Name = name;
                    Sources = sources;
                }
                public Id<LocalizerSetData> Id { get; }
                public string Name { get; }
                public IReadOnlyDictionary<Id<LocalizedStringType>, Id<FileInProject>> Sources { get; }

                public static LocalizerSetData Empty { get; } = new LocalizerSetData(Id<LocalizerSetData>.FromGuid(Guid.Empty), "Empty", new Dictionary<Id<LocalizedStringType>, Id<FileInProject>>());
            }

            public TData(IEnumerable<FileIdAndPath> conversations, IEnumerable<FileIdAndPath> domains, IEnumerable<FileIdAndPath> localizations, IEnumerable<FileIdAndPath> audios, IEnumerable<LocalizerSetData> localizationSets)
            {
                Conversations = conversations;
                Domains = domains;
                Localizations = localizations;
                LocalizationSets = localizationSets;
                Audios = audios;
            }

            public IEnumerable<FileIdAndPath> Conversations { get; }
            public IEnumerable<FileIdAndPath> Domains { get; }
            public IEnumerable<FileIdAndPath> Localizations { get; }
            public IEnumerable<LocalizerSetData> LocalizationSets { get; }
            public IEnumerable<FileIdAndPath> Audios { get; }
        }

        public class TConfig
        {
            public TConfig(Id<FileInProject> lastEdited, Id<Project.TData.LocalizerSetData> lastLocalization)
            {
                LastEdited = lastEdited;
                LastLocalization = lastLocalization;
            }
            public Id<FileInProject> LastEdited { get; }
            public Id<Project.TData.LocalizerSetData> LastLocalization { get; }
        }

        public ISerializer<TConversationData> ConversationSerializer { get; }
        public INodeFactory ConversationNodeFactory { get; }
        public INodeFactory DomainNodeFactory { get; }

        ISerializer<TData> m_serializer;
        ConversationSerializerDeserializerFactory m_conversationSerializerFactory;
        ISerializer<TDomainData> m_domainSerializer;
        private ProjectElementList<IConversationFile> m_conversations; //TODO: Could these be IProjectElementLists?
        private ProjectElementList<IDomainFile> m_domainFiles;
        private DomainDomain m_domainDataSource;
        private ConversationDataSource m_conversationDataSource;
        SaveableFileNotUndoable m_file;
        UpToDateFile.BackEnd m_upToDateFileBackend;

        public static Project CreateEmpty(ILocalizationContext context, FileInfo path, INodeFactory conversationNodeFactory, INodeFactory domainNodeFactory, ISerializer<TData> serializer, ISerializer<TConversationData> conversationSerializer, ConversationSerializerDeserializerFactory conversationSerializerDeserializer, ISerializer<TDomainData> domainSerializer, PluginsConfig pluginsConfig, Func<IAudioProviderCustomization> audioCustomization, UpToDateFile.BackEnd backEnd)
        {
            using (MemoryStream m = new MemoryStream())
            {
                //Create the new conversation file stream, fill with essential content and close
                FileInfo conversationFile = ConversationFile.GetAvailableConversationPath(path.Directory, Enumerable.Empty<ISaveableFileProvider>());
                using (FileStream conversationStream = Util.LoadFileStream(conversationFile, FileMode.CreateNew, FileAccess.Write))
                {
                    conversationSerializer.Write(SerializationUtils.MakeConversationData(Enumerable.Empty<ConversationNode>(), new ConversationEditorData()), conversationStream);
                }

                //Create the new project
                GetFilePath getFilePath = null; //Should never need this as all the FileId lists are empty
                Write(getFilePath, Enumerable.Empty<Id<FileInProject>>(), Enumerable.Empty<Id<FileInProject>>(), Enumerable.Empty<Id<FileInProject>>(), Enumerable.Empty<Id<FileInProject>>(), Enumerable.Empty<TData.LocalizerSetData>(), m, serializer);
                using (FileStream projectfile = Util.LoadFileStream(path, FileMode.Create, FileAccess.Write))
                {
                    m.Position = 0;
                    m.CopyTo(projectfile);
                    m.Position = 0;
                }

                TData data = new TData(Enumerable.Empty<TData.FileIdAndPath>(), Enumerable.Empty<TData.FileIdAndPath>(), Enumerable.Empty<TData.FileIdAndPath>(), Enumerable.Empty<TData.FileIdAndPath>(), Enumerable.Empty<TData.LocalizerSetData>());

                Project result = new Project(context, data, conversationNodeFactory, domainNodeFactory, m, path, serializer, conversationSerializer, conversationSerializerDeserializer, domainSerializer, pluginsConfig, audioCustomization, backEnd);
                return result;
            }
        }

        public HashSet<Id<LocalizedText>> UsedLocalizations()
        {
            return new HashSet<Id<LocalizedText>>(m_conversations.SelectMany(f => f.Nodes).SelectMany(f => f.Data.Parameters).OfType<LocalizedStringParameter>().Where(p => !p.Corrupted).Select(p => p.Value));
        }

        public bool ShouldContract(string file)
        {
            var result = MessageBox.Show(file + " contains elements not required by the current set of conversations. Remove these elements?", "Contract localization?", MessageBoxButtons.YesNo);
            switch (result)
            {
                case DialogResult.No:
                    return false;
                case DialogResult.Yes:
                    return true;
                default:
                    throw new InternalLogicException("Unexpected state fv8fbf");
            }
        }

        bool pathOk(FileInfo f)
        {
            return !f.Exists && !Elements.Any(s => s.File.File.FullName == f.FullName);
        }

        private bool m_conversationDatasourceModified = false;
        private void ConversationDatasourceModified()
        {
            m_conversationDatasourceModified = true;
        }
        public bool ReloadConversationDatasourceIfRequired()
        {
            if (m_conversationDatasourceModified)
            {
                m_conversationDatasourceModified = false;
                m_conversationDataSource = new ConversationDataSource(m_domainFiles.Select(f => f.Data));
                m_conversations.Reload(); //Reload all conversations
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="data"></param>
        /// <param name="conversationNodeFactory"></param>
        /// <param name="domainNodeFactory"></param>
        /// <param name="initialContent">Represents the current contents of the file. Reference is not held. A copy is made.</param>
        /// <param name="projectFile"></param>
        /// <param name="serializer"></param>
        /// <param name="conversationSerializer"></param>
        /// <param name="conversationSerializerDeserializerFactory"></param>
        /// <param name="domainSerializer"></param>
        /// <param name="pluginsConfig"></param>
        /// <param name="audioCustomization"></param>
        public Project(ILocalizationContext context, TData data, INodeFactory conversationNodeFactory, INodeFactory domainNodeFactory, MemoryStream initialData, FileInfo projectFile, ISerializer<TData> serializer, ISerializer<TConversationData> conversationSerializer, ConversationSerializerDeserializerFactory conversationSerializerDeserializerFactory, ISerializer<TDomainData> domainSerializer, PluginsConfig pluginsConfig, Func<IAudioProviderCustomization> audioCustomization, UpToDateFile.BackEnd backend)
        {
            AssertUniqueFileIds(data);
            AssertUniqueLocalizationSetNames(data);
            AssertUniqueFilePaths(data);

            m_upToDateFileBackend = backend;
            Action<Stream> saveTo = stream =>
            {
                Write(GetFilePath, Conversations.Select(x => (x.Id)), LocalizationFiles.Select(x => (x.Id)),
                      DomainFiles.Select(x => (x.Id)), AudioFiles.Select(x => (x.Id)), Localizer.LocalizationSets, stream, m_serializer);
            };
            m_file = new SaveableFileNotUndoable(initialData, projectFile, saveTo, backend);
            ConversationNodeFactory = conversationNodeFactory;
            DomainNodeFactory = domainNodeFactory;
            m_serializer = serializer;
            ConversationSerializer = conversationSerializer;
            m_conversationSerializerFactory = conversationSerializerDeserializerFactory;
            m_domainSerializer = domainSerializer;

            m_filePaths = data.Conversations.Concat(data.Localizations).Concat(data.Domains).Concat(data.Audios).ToDictionary(f => f.Id, f => f.Path);

            IEnumerable<Id<FileInProject>> conversationIds = data.Conversations.Select(f => f.Id);
            IEnumerable<Id<FileInProject>> localizerIds = data.Localizations.Select(f => f.Id);
            IEnumerable<Id<FileInProject>> domainIds = data.Domains.Select(f => f.Id);
            IEnumerable<Id<FileInProject>> audioIds = data.Audios.Select(f => f.Id);

            m_audioProvider = new AudioProvider(new FileInfo(projectFile.FullName), GetFilePath, s => CheckFolder(s, Origin), this, audioCustomization());
            using (m_audioProvider.SuppressUpdates())
            {
                {
                    m_audioProvider.AudioFiles.Load(audioIds);
                }

                {
                    //Dictionary<IDynamicEnumParameter, DynamicEnumParameter.Source> domainEnumSource = new Dictionary<IDynamicEnumParameter, DynamicEnumParameter.Source>();
                    Func<IDynamicEnumParameter, object, DynamicEnumParameter.Source> getDomainEnumSource = (k, o) =>
                    {
                        return null; //Nothing should need a source (but the system will ask anyway)
                                     //if (!domainEnumSource.ContainsKey(k))
                                     //domainEnumSource[k] = new DynamicEnumParameter.Source();
                                     //return domainEnumSource[k];
                    };

                    m_domainDataSource = new DomainDomain(pluginsConfig);
                    Func<IEnumerable<Tuple<Id<FileInProject>, DocumentPath>>, IEnumerable<IDomainFile>> loader = paths =>
                    {
                        var result = DomainFile.Load(paths, m_domainDataSource, document => DomainSerializerDeserializer.Make(m_domainDataSource), DomainNodeFactory, () => DomainUsage, getDomainEnumSource, backend).Evaluate();
                        result.ForAll(a => a.ConversationDomainModified += ConversationDatasourceModified);
                        return result;
                    };
                    Func<DirectoryInfo, DomainFile> makeEmpty = path => DomainFile.CreateEmpty(path, m_domainDataSource, m_domainSerializer, pathOk, DomainNodeFactory, () => DomainUsage, getDomainEnumSource, backend, Origin);
                    m_domainFiles = new ProjectElementList<IDomainFile>(GetFilePath, s => CheckFolder(s, Origin), loader, makeEmpty);
                    m_domainFiles.Load(domainIds);
                }
                m_conversationDataSource = new ConversationDataSource(m_domainFiles.Select(df => df.Data));


                {
                    m_localizer = new LocalizationEngine(GetFilePath, data.LocalizationSets, context, UsedLocalizations, ShouldContract, pathOk, s => CheckFolder(s, Origin), backend, Origin);
                    m_localizer.Localizers.Load(localizerIds);
                    context.CurrentLocalization.Value = m_localizer.LocalizationSets.FirstOrDefault() ?? Project.TData.LocalizerSetData.Empty;
                    m_localizer.LocalizationSetsChanged += () => { m_file.Change(); };
                }

                {
                    GenerateAudio audio = (c) =>
                    {
                        return m_audioProvider.Generate(new AudioGenerationParameters(c.File.File, this.File.File));
                    };

                    //This can be called from multiple threads simultaneously and in arbitrary orders by design of
                    //ConversationDataSource and the underlying ConstantTypeSet and DynamicEnumParameter.Source
                    Func<IDynamicEnumParameter, object, DynamicEnumParameter.Source> getSource = (localEnum, newSourceID) =>
                    {
                        return m_conversationDataSource.GetSource(localEnum.TypeId, newSourceID);
                    };

                    Func<IEnumerable<Tuple<Id<FileInProject>, DocumentPath>>, IEnumerable<IConversationFile>> loadConversations = files =>
                    {
                        //TODO: Can we just construct this serializer once before the parallel loop? If so we should be using it (it's currently unused), if not we should delete this line.
                        ISerializerDeserializer<XmlGraphData<NodeUIData, ConversationEditorData>> conversationSerializerDeserializer = m_conversationSerializerFactory(m_conversationDataSource);
                        //                                                 _/          _/                           _/                         X        _/             X            _/
                        //return files.Select(file => ConversationFile.Load(file, ConversationNodeFactory, conversationSerializerDeserializer, audio, getSource, m_audioProvider, backend));
                        //TODO: This is ok as long as we're not using audio parameters at all
                        return ParallelEnumerable.Select(files.AsParallel(), file => ConversationFile.Load(file.Item1, file.Item2, ConversationNodeFactory, m_conversationSerializerFactory(m_conversationDataSource), audio, getSource, m_audioProvider, backend));
                    };
                    Func<DirectoryInfo, ConversationFile> makeEmpty = path => ConversationFile.CreateEmpty(path, this, ConversationNodeFactory, audio, getSource, m_audioProvider, backend, Origin);
                    m_conversations = new ProjectElementList<IConversationFile>(GetFilePath, s => CheckFolder(s, Origin), loadConversations, makeEmpty);
                    m_conversations.Load(conversationIds);
                }

                RefreshCallbacks(m_conversations, m_actionsOnRefreshCallbacksConversations);
                RefreshCallbacks(m_audioProvider.AudioFiles, m_actionsOnRefreshCallbacksAudios);
                RefreshCallbacks(m_localizer.Localizers, m_actionsOnRefreshCallbacksLocalizations);
                RefreshCallbacks(m_domainFiles, m_actionsOnRefreshCallbacksDomains);

                m_conversations.GotChanged += GotChanged;
                m_audioProvider.AudioFiles.GotChanged += GotChanged;
                m_localizer.Localizers.GotChanged += GotChanged;
                m_domainFiles.GotChanged += GotChanged;

                m_conversations.GotChanged += () => RefreshCallbacks(m_conversations, m_actionsOnRefreshCallbacksConversations);
                m_audioProvider.AudioFiles.GotChanged += () => RefreshCallbacks(m_audioProvider.AudioFiles, m_actionsOnRefreshCallbacksAudios);
                m_localizer.Localizers.GotChanged += () => RefreshCallbacks(m_localizer.Localizers, m_actionsOnRefreshCallbacksLocalizations);
                m_domainFiles.GotChanged += () => RefreshCallbacks(m_domainFiles, m_actionsOnRefreshCallbacksDomains);


                m_domainFiles.GotChanged += ConversationDatasourceModified;
                Action<IDomainFile> MaybeConversationDatasourceModified = file => { if (!file.File.Changed()) ConversationDatasourceModified(); };
                m_domainFiles.Added += argument => { argument.File.SaveStateChanged += () => MaybeConversationDatasourceModified(argument); };
                m_domainFiles.Reloaded += (_, argument) => { argument.File.SaveStateChanged += () => MaybeConversationDatasourceModified(argument); };
                m_domainFiles.ForAll(d => d.File.SaveStateChanged += () => MaybeConversationDatasourceModified(d));
                //m_domainFiles.Added += argument => { argument.File.SaveStateChanged += () => { if (!argument.File.Changed) ReloadDatasource(); }; };
                //m_domainFiles.Reloaded += (from, to) => { to.File.SaveStateChanged += () => { if (!to.File.Changed) ReloadDatasource(); }; };
                //m_domainFiles.ForAll(d => d.File.SaveStateChanged += () => { if (!d.File.Changed) ReloadDatasource(); });

                m_domainFiles.Added += file => m_filePaths[file.Id] = DocumentPath.FromAbsolutePath(file.File.File.FullName, Origin);
                m_conversations.Added += file => m_filePaths[file.Id] = DocumentPath.FromAbsolutePath(file.File.File.FullName, Origin);
                m_audioProvider.AudioFiles.Added += file => m_filePaths[file.Id] = DocumentPath.FromAbsolutePath(file.File.File.FullName, Origin);
                m_localizer.Localizers.Added += file => m_filePaths[file.Id] = DocumentPath.FromAbsolutePath(file.File.File.FullName, Origin);
                m_domainFiles.Removed += file => m_filePaths.Remove(file.Id);
                m_conversations.Removed += file => m_filePaths.Remove(file.Id);
                m_audioProvider.AudioFiles.Removed += file => m_filePaths.Remove(file.Id);
                m_localizer.Localizers.Removed += file => m_filePaths.Remove(file.Id);
                //Files being moved is setup in RefreshCallbacks()

                m_domainUsage = new DomainUsage(this);

                m_audioProvider.UpdateUsage();
            }
        }

        private static void AssertUniqueFilePaths(TData data)
        {
            var allPaths = data.Audios.Select(f => f.Path.RelativePath).Concat(
                           data.Conversations.Select(f => f.Path.RelativePath)).Concat(
                           data.Domains.Select(f => f.Path.RelativePath)).Concat(
                           data.Localizations.Select(f => f.Path.RelativePath));
            var badPaths = allPaths.GroupBy(p => p).Where(g => g.Count() != 1).Select(g => g.Key);
            if (badPaths.Any())
            {
                StringBuilder builder = new StringBuilder("The following file paths are not unique within the project file. This is a violation of the schema.\n");
                foreach (var path in badPaths)
                    builder.AppendLine(path);
                throw new ProjectCreationException(builder.ToString());
            }
        }

        private static void AssertUniqueLocalizationSetNames(TData data)
        {
            var badLocalizationSetNames = data.LocalizationSets.GroupBy(f => f.Name).Where(g => g.Count() != 1).ToList();
            if (badLocalizationSetNames.Any())
            {
                StringBuilder builder = new StringBuilder("The following Localization Set names are not unique within the project file. This is a violation of the schema.\n");
                foreach (var name in badLocalizationSetNames.Select(g => g.Key))
                    builder.AppendLine(name);
                throw new ProjectCreationException(builder.ToString());
            }
        }

        private static void AssertUniqueFileIds(TData data)
        {
            var badAudioIds = data.Audios.GroupBy(f => f.Id).Where(g => g.Count() != 1).ToList();
            var badConversationIds = data.Conversations.GroupBy(f => f.Id).Where(g => g.Count() != 1).ToList();
            var badDomainIds = data.Domains.GroupBy(f => f.Id).Where(g => g.Count() != 1).ToList();
            var badLocalizationIds = data.Localizations.GroupBy(f => f.Id).Where(g => g.Count() != 1).ToList();
            var badLocalizationSetIds = data.LocalizationSets.GroupBy(f => f.Id).Where(g => g.Count() != 1).ToList();
            if (badAudioIds.Any() || badConversationIds.Any() || badDomainIds.Any() || badLocalizationIds.Any() || badLocalizationSetIds.Any())
            {
                StringBuilder builder = new StringBuilder("The following ids are not unique within the project file. This is a violation of the schema.\n");
                var allFileGroups = badAudioIds.Concat(badConversationIds).Concat(badDomainIds).Concat(badLocalizationIds);
                foreach (var id in allFileGroups.Select(g => g.Key.Serialized()))
                    builder.AppendLine(id);
                foreach (var id in badLocalizationSetIds.Select(g => g.Key.Serialized()))
                    builder.AppendLine(id);
                throw new ProjectCreationException(builder.ToString());
            }
        }

        private void RefreshCallbacks<TInterface>(IProjectElementList<TInterface> elements, List<Action> actionsOnRefreshCallbacks)
            where TInterface : ISaveableFileProvider, IInProject
        {
            foreach (var action in actionsOnRefreshCallbacks)
                action();
            actionsOnRefreshCallbacks.Clear();

            foreach (var e in elements)
            {
                var element = e;

                Action onElementModifiedExternally = () => OnElementModifiedExternally(elements, element);
                Action onElementDeletedExternally = () => OnElementDeletedExternally(element);
                Action<Changed<FileInfo>> onElementMoved = change => OnElementMoved(element.Id, change);

                actionsOnRefreshCallbacks.Add(() =>
                {
                    element.File.FileModifiedExternally -= onElementModifiedExternally;
                    element.File.FileDeletedExternally -= onElementDeletedExternally;
                    element.File.Moved -= onElementMoved;
                });

                element.File.FileModifiedExternally += onElementModifiedExternally;
                element.File.FileDeletedExternally += onElementDeletedExternally;
                element.File.Moved += onElementMoved;
            }
        }

        List<Action> m_actionsOnRefreshCallbacksConversations = new List<Action>();
        List<Action> m_actionsOnRefreshCallbacksDomains = new List<Action>();
        List<Action> m_actionsOnRefreshCallbacksAudios = new List<Action>();
        List<Action> m_actionsOnRefreshCallbacksLocalizations = new List<Action>();

        public void OnElementMoved(Id<FileInProject> id, Changed<FileInfo> change)
        {
            m_filePaths[id] = DocumentPath.FromPath(change.To, Origin);
            if (change.From.FullName != change.To.FullName)
                GotChanged();
        }

        public void OnElementModifiedExternally<TInterface>(IProjectElementList<TInterface> list, TInterface element)
            where TInterface : ISaveableFileProvider, IInProject
        {
            ElementModifiedExternally.Execute(element, () =>
                {
                    var path = GetFilePath(element.Id);
                    list.Remove(element, true);
                    m_filePaths[element.Id] = path; //Readd the file mapping for the id we're about to load
                    list.Load(element.Id.Only());
                });
        }

        public void OnElementDeletedExternally(ISaveableFileProvider element)
        {
            ElementDeletedExternally.Execute(element);
        }

        public IEnumerable<ISaveableFileProvider> Elements
        {
            get
            {
                return ElementsExceptThis.Concat(this.Only());
            }
        }

        public IEnumerable<ISaveableFileProvider> ElementsExceptThis
        {
            get
            {
                return m_conversations.Concat<ISaveableFileProvider>(m_localizer.Localizers).Concat(m_domainFiles).Concat(m_audioProvider.AudioFiles);
            }
        }

        public static bool CheckFolder(string path, DirectoryInfo origin)
        {
            if (Path.IsPathRooted(path))
                return (new FileInfo(Path.Combine(origin.FullName, path))).AncestorOf(origin);
            else
                return true; //If it's not absolute then it's relative to the origin
        }

        public string Rerout(string path)
        {
            return Path.IsPathRooted(path) ? path : Path.Combine(Origin.FullName, path);
        }

        public ISaveableFile File { get { return m_file; } }

        private static DirectoryInfo GetOrigin(FileInfo projectFile)
        {
            return projectFile.Directory;
        }

        public DirectoryInfo Origin { get { return GetOrigin(File.File); } }

        public IProjectElementList<IConversationFile> Conversations { get { return m_conversations; } }

        public IProjectElementList<ILocalizationFile> LocalizationFiles { get { return m_localizer.Localizers; } }

        public IProjectElementList<IDomainFile> DomainFiles { get { return m_domainFiles; } }

        public IProjectElementList<IAudioFile> AudioFiles
        {
            get { return m_audioProvider.AudioFiles; }
        }

        private static void Write(GetFilePath getFilePath, IEnumerable<Id<FileInProject>> conversations, IEnumerable<Id<FileInProject>> localizations, IEnumerable<Id<FileInProject>> domains, IEnumerable<Id<FileInProject>> audio, IEnumerable<TData.LocalizerSetData> localizationSets, Stream stream, ISerializer<TData> serializer)
        {
            Func<Id<FileInProject>, TData.FileIdAndPath> getPath = fileId => new TData.FileIdAndPath(fileId, getFilePath(fileId));
            serializer.Write(new TData(conversations.Select(getPath), domains.Select(getPath), localizations.Select(getPath), audio.Select(getPath), localizationSets), stream);
        }

        public void GotChanged()
        {
            //Can't undo changes to the project
            m_file.Change();
        }

        public IDomainDataSource DomainDataSource
        {
            get { return m_domainDataSource; }
        }

        public IDataSource ConversationDataSource
        {
            get { return m_conversationDataSource; }
        }

        public bool CanModifyConversations
        {
            get { return DomainFiles.All(f => f.File.Writable == null || !f.File.Writable.Changed); }
        }

        public bool CanModifyDomain
        {
            get { return Conversations.All(f => f.File.Writable == null || !f.File.Writable.Changed); }
        }

        private readonly IAudioLibrary m_audioProvider;
        public IAudioLibrary AudioProvider
        {
            get { return m_audioProvider; }
        }

        LocalizationEngine m_localizer;
        public LocalizationEngine Localizer
        {
            get { return m_localizer; }
        }

        private IDomainUsage<ConversationNode, TransitionNoduleUIInfo> m_domainUsage;
        public IDomainUsage<ConversationNode, TransitionNoduleUIInfo> DomainUsage
        {
            get { return m_domainUsage; }
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

        /// <summary>
        /// Triggered when one of the elements of the project is modified outside of the editor
        /// First parameter represents the element that was modified
        /// Second parameter is a callback to forcibly reload the element from disk, ignoring any in-editor changes
        /// Third parameter is a callback that should be triggered when the event has been handled. This renables the worker thread listening for such events.
        /// </summary>
        public event Action<ISaveableFileProvider, Action> ElementModifiedExternally;
        public event Action<ISaveableFileProvider> ElementDeletedExternally;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_upToDateFileBackend.Dispose();
                foreach (var element in ElementsExceptThis)
                    element.Dispose();
                m_file.Dispose();
            }
        }

        public IEnumerable<IDomainFile> DomainFilesCollection
        {
            get { return DomainFiles; }
        }

        public IEnumerable<IConversationFile> ConversationFilesCollection
        {
            get { return Conversations; }
        }

        public Func<IParameter, string, Func<ParameterType, DynamicEnumParameter.Source>, IEnumerable<string>> AutoCompleteSuggestions
        {
            get
            {
                return (p, s, e) => m_domainFiles.SelectMany(d => d.AutoCompleteSuggestions(p, s, e));
            }
        }

        Dictionary<Id<FileInProject>, DocumentPath> m_filePaths;

        public DocumentPath GetFilePath(Id<FileInProject> id)
        {
            return m_filePaths[id];
        }
    }
}

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

    internal class Project : Disposable, IProject, ISaveableFileProvider
    {
        public static ILocalizationFile SelectNewLocalizer(IProjectElementList<LocalizationFile, ILocalizationFile> localizers)
        {
            if (localizers.Any())
                return localizers.First();
            else
                return DummyLocalizationFile.Instance;
        }

        public class TData
        {
            public TData(IEnumerable<string> conversations, IEnumerable<string> domains, IEnumerable<string> localizations, IEnumerable<string> audios)
            {
                Conversations = conversations;
                Domains = domains;
                Localizations = localizations;
                Audios = audios;
            }

            public readonly IEnumerable<string> Conversations;
            public readonly IEnumerable<string> Domains;
            public readonly IEnumerable<string> Localizations;
            public readonly IEnumerable<string> Audios;
        }

        public class TConfig
        {
            public TConfig(string lastEdited, string lastLocalization)
            {
                LastEdited = lastEdited;
                LastLocalization = lastLocalization;
            }
            public readonly string LastEdited;
            public readonly string LastLocalization;
        }

        public readonly ISerializer<TConversationData> ConversationSerializer;
        public readonly INodeFactory ConversationNodeFactory;
        public readonly INodeFactory DomainNodeFactory;

        ISerializer<TData> m_serializer;
        ConversationSerializerDeserializerFactory m_conversationSerializerFactory;
        ISerializer<TDomainData> m_domainSerializer;
        private ProjectElementList<ConversationFile, MissingConversationFile, IConversationFile> m_conversations;
        private ProjectElementList<DomainFile, MissingDomainFile, IDomainFile> m_domainFiles;
        private DomainDomain m_domainDataSource;
        private ConversationDataSource m_conversationDataSource;
        SaveableFileNotUndoable m_file;

        public static Project CreateEmpty(ILocalizationContext context, FileInfo path, INodeFactory conversationNodeFactory, INodeFactory domainNodeFactory, ISerializer<TData> serializer, ISerializer<TConversationData> conversationSerializer, ConversationSerializerDeserializerFactory conversationSerializerDeserializer, ISerializer<TDomainData> domainSerializer, PluginsConfig pluginsConfig, Func<IAudioProviderCustomization> audioCustomization)
        {
            Project result = null;
            FileInfo conversationFile;
            LocalizationFile localizationFile;
            //Start by making sure we can open the file
            MemoryStream m = new MemoryStream();

            //Create the new conversation file stream, fill with essential content and close
            conversationFile = ConversationFile.GetAvailableConversationPath(path.Directory, Enumerable.Empty<ISaveableFileProvider>());
            using (FileStream conversationStream = Util.LoadFileStream(conversationFile, FileMode.CreateNew, FileAccess.Write))
            {
                conversationSerializer.Write(SerializationUtils.MakeConversationData(Enumerable.Empty<ConversationNode>(), new ConversationEditorData()), conversationStream);
            }

            LocalizationEngine temp = new LocalizationEngine(context, () => new HashSet<Id<LocalizedText>>(), s => false, s => false, p => !p.Exists, s => true);
            localizationFile = LocalizationFile.MakeNew(path.Directory, s => temp.MakeSerializer(s), p => !p.Exists);

            //Create the new project
            Write(conversationFile.Only(), localizationFile.File.File.Only(), Enumerable.Empty<FileInfo>(), Enumerable.Empty<FileInfo>(), m, path.Directory, serializer);
            using (FileStream projectfile = Util.LoadFileStream(path, FileMode.Create, FileAccess.Write))
            {
                m.Position = 0;
                m.CopyTo(projectfile);
                m.Position = 0;
            }

            var conversationPaths = conversationFile.Only().Select(c => FileSystem.RelativePath(c, path.Directory));
            var localizationPaths = localizationFile.Only().Select(l => FileSystem.RelativePath(l.File.File, path.Directory));
            var domainPaths = Enumerable.Empty<string>();

            TData data = new TData(conversationPaths, domainPaths, localizationPaths, Enumerable.Empty<string>());

            result = new Project(context, data, conversationNodeFactory, domainNodeFactory, m, path, serializer, conversationSerializer, conversationSerializerDeserializer, domainSerializer, pluginsConfig, audioCustomization);
            return result;
        }

        public HashSet<Id<LocalizedText>> UsedLocalizations()
        {
            return new HashSet<Id<LocalizedText>>(m_conversations.SelectMany(f => f.Nodes).SelectMany(f => f.Parameters).OfType<LocalizedStringParameter>().Where(p => !p.Corrupted).Select(p => p.Value));
        }

        public bool ShouldExpand(string file)
        {
            var result = MessageBox.Show(file + " is lacking elements required by the current set of conversations. Expand using data from current localization where available?", "Expand localization?", MessageBoxButtons.YesNo);
            switch (result)
            {
                case DialogResult.No:
                    return false;
                case DialogResult.Yes:
                    return true;
                default:
                    throw new InternalLogicException("Unexpected state sb367f");
            }
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
                m_conversationDataSource = new ConversationDataSource(BaseTypeSet.Make(), m_domainFiles.Select(f => f.Data));
                m_conversations.Reload(); //Reload all conversations
                return true;
            }
            return false;
        }

        public Project(ILocalizationContext context, TData data, INodeFactory conversationNodeFactory, INodeFactory domainNodeFactory, MemoryStream initialData, FileInfo projectFile, ISerializer<TData> serializer, ISerializer<TConversationData> conversationSerializer, ConversationSerializerDeserializerFactory conversationSerializerDeserializerFactory, ISerializer<TDomainData> domainSerializer, PluginsConfig pluginsConfig, Func<IAudioProviderCustomization> audioCustomization)
        {
            Action<Stream> saveTo = stream => { Write(Conversations.Select(c => c.File.File), LocalizationFiles.Select(l => l.File.File), DomainFiles.Select(d => d.File.File), AudioFiles.Select(a => a.File.File), stream, Origin, m_serializer); };
            m_file = new SaveableFileNotUndoable(initialData, projectFile, saveTo);
            ConversationNodeFactory = conversationNodeFactory;
            DomainNodeFactory = domainNodeFactory;
            m_serializer = serializer;
            ConversationSerializer = conversationSerializer;
            m_conversationSerializerFactory = conversationSerializerDeserializerFactory;
            m_domainSerializer = domainSerializer;

            IEnumerable<string> conversationPaths = data.Conversations;
            IEnumerable<string> localizerPaths = data.Localizations;
            IEnumerable<string> domainPaths = data.Domains;
            IEnumerable<string> audioPaths = data.Audios;

            m_audioProvider = new AudioProvider(new FileInfo(projectFile.FullName), s => CheckFolder(s, Origin), this, audioCustomization());
            using (m_audioProvider.SuppressUpdates())
            {
                {
                    IEnumerable<FileInfo> toLoad = Rerout(audioPaths);
                    m_audioProvider.AudioFiles.Load(toLoad);
                }

                //m_conversationDataSource = new ConversationDataSource(BaseTypeSet.Make(), Enumerable.Empty<DomainData>());
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
                    Func<IEnumerable<FileInfo>, IEnumerable<Either<DomainFile, MissingDomainFile>>> loader = paths =>
                    {
                        var result = DomainFile.Load(paths, m_domainDataSource, document => DomainSerializerDeserializer.Make(m_domainDataSource), DomainNodeFactory, () => DomainUsage, getDomainEnumSource).Evaluate();
                        result.ForAll(a => a.Do(b => b.ConversationDomainModified += ConversationDatasourceModified, null));
                        return result;
                    };
                    Func<DirectoryInfo, DomainFile> makeEmpty = path => DomainFile.CreateEmpty(path, m_domainDataSource, m_domainSerializer, pathOk, DomainNodeFactory, () => DomainUsage, getDomainEnumSource);
                    m_domainFiles = new ProjectElementList<DomainFile, MissingDomainFile, IDomainFile>(s => CheckFolder(s, Origin), loader, makeEmpty);
                    IEnumerable<FileInfo> toLoad = Rerout(domainPaths);
                    m_domainFiles.Load(toLoad);
                }
                m_conversationDataSource = new ConversationDataSource(BaseTypeSet.Make(), m_domainFiles.Select(df => df.Data));


                {
                    m_localizer = new LocalizationEngine(context, UsedLocalizations, ShouldContract, ShouldExpand, pathOk, s => CheckFolder(s, Origin));
                    IEnumerable<FileInfo> toLoad = Rerout(localizerPaths);
                    m_localizer.Localizers.Load(toLoad);
                }

                {
                    Func<ISaveableFileProvider, IEnumerable<IParameter>, Audio> audio = (c, p) =>
                    {
                        return m_audioProvider.Generate(new AudioGenerationParameters(c.File.File, this.File.File));
                    };
                    Func<IDynamicEnumParameter, object, DynamicEnumParameter.Source> getSource = (localEnum, newSourceID) =>
                    {
                        return m_conversationDataSource.GetSource(localEnum, newSourceID);
                    };
                    Func<IEnumerable<FileInfo>, IEnumerable<ConversationFile>> loadConversation = files => files.Select(file => ConversationFile.Load(file, ConversationNodeFactory, m_conversationSerializerFactory(m_conversationDataSource), audio, getSource, m_audioProvider));
                    Func<DirectoryInfo, ConversationFile> makeEmpty = path => ConversationFile.CreateEmpty(path, this, ConversationNodeFactory, audio, getSource, m_audioProvider);
                    Func<FileInfo, MissingConversationFile> makeMissing = file => new MissingConversationFile(file);
                    m_conversations = new ProjectElementList<ConversationFile, MissingConversationFile, IConversationFile>(s => CheckFolder(s, Origin), loadConversation, makeEmpty, makeMissing);
                    IEnumerable<FileInfo> toLoad = Rerout(conversationPaths);
                    m_conversations.Load(toLoad);
                }

                RefreshCallbacks(m_conversations);
                RefreshCallbacks(m_audioProvider.AudioFiles);
                RefreshCallbacks(m_localizer.Localizers);
                RefreshCallbacks(m_domainFiles);

                m_conversations.GotChanged += GotChanged;
                m_audioProvider.AudioFiles.GotChanged += GotChanged;
                m_localizer.Localizers.GotChanged += GotChanged;
                m_domainFiles.GotChanged += GotChanged;

                m_conversations.GotChanged += () => RefreshCallbacks(m_conversations);
                m_audioProvider.AudioFiles.GotChanged += () => RefreshCallbacks(m_audioProvider.AudioFiles);
                m_localizer.Localizers.GotChanged += () => RefreshCallbacks(m_localizer.Localizers);
                m_domainFiles.GotChanged += () => RefreshCallbacks(m_domainFiles);


                m_domainFiles.GotChanged += ConversationDatasourceModified;
                Action<IDomainFile> MaybeConversationDatasourceModified = file => { if (!file.File.Changed()) ConversationDatasourceModified(); };
                m_domainFiles.Added += argument => { argument.File.SaveStateChanged += () => MaybeConversationDatasourceModified(argument); };
                m_domainFiles.Reloaded += (_, argument) => { argument.File.SaveStateChanged += () => MaybeConversationDatasourceModified(argument); };
                m_domainFiles.ForAll(d => d.File.SaveStateChanged += () => MaybeConversationDatasourceModified(d));
                //m_domainFiles.Added += argument => { argument.File.SaveStateChanged += () => { if (!argument.File.Changed) ReloadDatasource(); }; };
                //m_domainFiles.Reloaded += (from, to) => { to.File.SaveStateChanged += () => { if (!to.File.Changed) ReloadDatasource(); }; };
                //m_domainFiles.ForAll(d => d.File.SaveStateChanged += () => { if (!d.File.Changed) ReloadDatasource(); });

                m_domainUsage = new DomainUsage(this);

                m_audioProvider.UpdateUsage();
            }
        }

        public void RefreshCallbacks<TReal, TInterface>(IProjectElementList<TReal, TInterface> elements)
            where TReal : TInterface
            where TInterface : ISaveableFileProvider
        {
            foreach (var element in elements)
            {
                element.File.Moved -= OnElementMoved;

                Action onElementModifiedExternally = () => OnElementModifiedExternally(elements, element);
                Action onElementDeletedExternally = () => OnElementDeletedExternally(element);

                element.File.FileModifiedExternally -= onElementModifiedExternally;
                element.File.FileDeletedExternally -= onElementDeletedExternally;

                element.File.Moved += OnElementMoved;
                element.File.FileModifiedExternally += onElementModifiedExternally;
                element.File.FileDeletedExternally += onElementDeletedExternally;
            }
        }

        public void OnElementMoved(Changed<FileInfo> change)
        {
            if (change.From.FullName != change.To.FullName)
                GotChanged();
        }

        public void OnElementModifiedExternally<TReal, TInterface>(IProjectElementList<TReal, TInterface> list, TInterface element)
            where TReal : TInterface
            where TInterface : ISaveableFileProvider
        {
            ElementModifiedExternally.Execute(element, () =>
                {
                    list.Remove(element, true);
                    list.Load(element.File.File.Only());
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

        public IEnumerable<FileInfo> Rerout(IEnumerable<string> paths)
        {
            List<FileInfo> toLoad = new List<FileInfo>();
            foreach (var path in paths)
            {
                FileInfo file = new FileInfo(Path.IsPathRooted(path) ? path : Path.Combine(Origin.FullName, path));

                if (file.AncestorOf(Origin))
                {
                    toLoad.Add(file);
                }
                else
                    throw new InvalidOperationException("Attempting to import file from a folder than is not a subfolder of the project's parent folder");
            }
            return toLoad.Distinct(new GenericEqualityComparer<FileInfo>((a, b) => a.FullName == b.FullName, a => a.FullName.GetHashCode()));
        }

        public ISaveableFile File { get { return m_file; } }

        public DirectoryInfo Origin { get { return File.File.Directory; } }

        public IProjectElementList<ConversationFile, IConversationFile> Conversations { get { return m_conversations; } }

        public IProjectElementList<LocalizationFile, ILocalizationFile> LocalizationFiles { get { return m_localizer.Localizers; } }

        public IProjectElementList<DomainFile, IDomainFile> DomainFiles { get { return m_domainFiles; } }

        public IProjectElementList<AudioFile, IAudioFile> AudioFiles
        {
            get { return m_audioProvider.AudioFiles; }
        }

        private static void Write(IEnumerable<FileInfo> conversations, IEnumerable<FileInfo> localizations, IEnumerable<FileInfo> domains, IEnumerable<FileInfo> audio, Stream stream, DirectoryInfo origin, ISerializer<TData> serializer)
        {
            var conversationPaths = conversations.Select(c => FileSystem.RelativePath(c, origin));
            var localizationPaths = localizations.Select(l => FileSystem.RelativePath(l, origin));
            var domainPaths = domains.Select(d => FileSystem.RelativePath(d, origin));
            var audioPaths = audio.Select(d => FileSystem.RelativePath(d, origin));
            serializer.Write(new Project.TData(conversationPaths, domainPaths, localizationPaths, audioPaths), stream);
        }

        public void GotChanged()
        {
            //Can't undo changes to the project
            m_file.Change();
        }

        public IDataSource DomainDataSource
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
                foreach (var element in ElementsExceptThis)
                    element.Dispose();
                m_file.Dispose();
                m_conversations.Dispose();
                m_domainFiles.Dispose();
                m_localizer.Dispose();
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
    }
}

﻿using System;
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
    using ConversationNode = ConversationNode<INodeGUI>;
    //using TData = Tuple<IEnumerable<string>, IEnumerable<string>, IEnumerable<string>>;
    using TConversationData = XmlGraphData<NodeUIData, ConversationEditorData>;
    using TDomainData = XmlGraphData<NodeUIData, ConversationEditorData>;
    using TConversationSerializerDeserializer = ISerializerDeserializer<Tuple<IEnumerable<ConversationNode<INodeGUI>>, ConversationEditorData>>;
    using TDomainSerializerDeserializer = ISerializerDeserializer<Tuple<IEnumerable<ConversationNode<INodeGUI>>, ConversationEditorData>>;
    using ConversationSerializerDeserializerFactory = Func<IDataSource, ISerializerDeserializer<XmlGraphData<NodeUIData, ConversationEditorData>>>;
    using DomainSerializerDeserializerFactory = Func<IDataSource, DomainSerializerDeserializer>;

    public class Project : IProject, ISaveableFileProvider
    {
        public class TData
        {
            public TData(IEnumerable<string> conversations, IEnumerable<string> domains, IEnumerable<string> localizations, IEnumerable<string> audios, string lastEdited, string selectedLocalization)
            {
                Conversations = conversations;
                Domains = domains;
                Localizations = localizations;
                Audios = audios;
                LastEdited = lastEdited;
                SelectedLocalization = selectedLocalization;
            }

            public readonly IEnumerable<string> Conversations;
            public readonly IEnumerable<string> Domains;
            public readonly IEnumerable<string> Localizations;
            public readonly IEnumerable<string> Audios;
            public readonly string LastEdited;
            public readonly string SelectedLocalization;
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

        public static Project CreateEmpty(FileInfo path, INodeFactory conversationNodeFactory, INodeFactory domainNodeFactory, ISerializer<TData> serializer, ISerializer<TConversationData> conversationSerializer, ConversationSerializerDeserializerFactory conversationSerializerDeserializer, ISerializer<TDomainData> domainSerializer, PluginsConfig pluginsConfig)
        {
            Project result = null;
            FileInfo conversationFile;
            LocalizationFile localizationFile;
            //Start by making sure we can open the file
            using (FileStream projectfile = Util.LoadFileStream(path, FileMode.Create, FileAccess.Write))
            {
                //Create the new conversation file stream, fill with essential content and close
                conversationFile = ConversationFile.GetAvailableConversationPath(path.Directory, Enumerable.Empty<ISaveableFileProvider>(), p => !p.Exists);
                using (FileStream conversationStream = Util.LoadFileStream(conversationFile, FileMode.CreateNew, FileAccess.Write))
                {
                    conversationSerializer.Write(SerializationUtils.MakeConversationData(Enumerable.Empty<ConversationNode>(), new ConversationEditorData()), conversationStream);
                }

                LocalizationEngine temp = new LocalizationEngine(() => new HashSet<ID<LocalizedText>>(), s => false, s => false, p => !p.Exists, s => true);
                localizationFile = LocalizationFile.MakeNew(path.Directory, s => temp.MakeSerializer(s), p => !p.Exists);

                //Create the new project
                Write(conversationFile.Only(), localizationFile.File.File.Only(), Enumerable.Empty<FileInfo>(), Enumerable.Empty<FileInfo>(), projectfile, path.Directory, serializer);
            }

            var conversationPaths = conversationFile.Only().Select(c => FileSystem.RelativePath(c, path.Directory));
            var localizationPaths = localizationFile.Only().Select(l => FileSystem.RelativePath(l.File.File, path.Directory));
            var domainPaths = Enumerable.Empty<string>();

            TData data = new TData(conversationPaths, domainPaths, localizationPaths, Enumerable.Empty<string>(), null, null);

            result = new Project(data, conversationNodeFactory, domainNodeFactory, path, serializer, conversationSerializer, conversationSerializerDeserializer, domainSerializer, pluginsConfig);
            return result;
        }

        public HashSet<ID<LocalizedText>> UsedLocalizations()
        {
            return new HashSet<ID<LocalizedText>>(m_conversations.SelectMany(f => f.Nodes).SelectMany(f => f.Parameters).OfType<LocalizedStringParameter>().Where(p => !p.Corrupted).Select(p => p.Value));
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
                    throw new Exception("Unexpected state sb367f");
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
                    throw new Exception("Unexpected state fv8fbf");
            }
        }

        bool pathOk(FileInfo f)
        {
            return !f.Exists && !Elements.Any(s => s.File.File.FullName == f.FullName);
        }

        public Project(TData data, INodeFactory conversationNodeFactory, INodeFactory domainNodeFactory, FileInfo projectFile, ISerializer<TData> serializer, ISerializer<TConversationData> conversationSerializer, ConversationSerializerDeserializerFactory conversationSerializerDeserializerFactory, ISerializer<TDomainData> domainSerializer, PluginsConfig pluginsConfig)
        {
            Action<Stream> saveTo = stream => { Write(Conversations.Select(c => c.File.File), LocalizationFiles.Select(l => l.File.File), DomainFiles.Select(d => d.File.File), AudioFiles.Select(a => a.File.File), stream, Origin, m_serializer); };
            m_file = new SaveableFileNotUndoable(projectFile, saveTo);
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

            {
                m_audioProvider = new AudioProvider(new FileInfo(projectFile.Name), s => CheckFolder(s, Origin));
                IEnumerable<FileInfo> toLoad = Rerout(audioPaths);
                m_audioProvider.AudioFiles.Load(toLoad);
            }

            m_conversationDataSource = new ConversationDataSource(BaseTypeSet.Make(), Enumerable.Empty<DomainData>());
            {
                m_domainDataSource = new DomainDomain(pluginsConfig);
                Func<IEnumerable<FileInfo>, IEnumerable<Or<DomainFile, MissingDomainFile>>> loader = paths => DomainFile.Load(paths, m_domainDataSource, m_conversationDataSource, DomainSerializerDeserializer.Make(m_domainDataSource), DomainNodeFactory);
                Func<DirectoryInfo, DomainFile> makeEmpty = path => DomainFile.CreateEmpty(path, m_domainDataSource, m_conversationDataSource, m_domainSerializer, pathOk, DomainNodeFactory);
                Func<FileInfo, MissingDomainFile> makeMissing = file => new MissingDomainFile(file);
                m_domainFiles = new ProjectElementList<DomainFile, MissingDomainFile, IDomainFile>(s => CheckFolder(s, Origin), loader, makeEmpty, makeMissing);
                IEnumerable<FileInfo> toLoad = Rerout(domainPaths);
                m_domainFiles.Load(toLoad);
            }
            //m_conversationDataSource = new ConversationDataSource(BaseTypeSet.Make(), m_domainFiles.Select(df => df.Data));

            {
                Func<IEnumerable<FileInfo>, IEnumerable<ConversationFile>> loadConversation = files => files.Select(file => ConversationFile.Load(file, m_conversationDataSource, ConversationNodeFactory, m_conversationSerializerFactory(m_conversationDataSource)));
                Func<DirectoryInfo, ConversationFile> makeEmpty = path => ConversationFile.CreateEmpty(path, this, pathOk, ConversationNodeFactory);
                Func<FileInfo, MissingConversationFile> makeMissing = file => new MissingConversationFile(file);
                m_conversations = new ProjectElementList<ConversationFile, MissingConversationFile, IConversationFile>(s => CheckFolder(s, Origin), loadConversation, makeEmpty, makeMissing);
                IEnumerable<FileInfo> toLoad = Rerout(conversationPaths);
                m_conversations.Load(toLoad);
            }
            {
                m_localizer = new LocalizationEngine(UsedLocalizations, ShouldContract, ShouldExpand, pathOk, s => CheckFolder(s, Origin));
                IEnumerable<FileInfo> toLoad = Rerout(localizerPaths);
                m_localizer.Localizers.Load(toLoad);
                m_localizer.SelectLocalizer(m_localizer.Localizers.First()); //TODO: Pick appropriate localizer
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

            //m_domainFiles.Added += argument => ReloadDatasource();
            m_domainFiles.Removed += argument => ReloadDatasource();
            //m_domainFiles.Reloaded += (from, to) => ReloadDatasource();
            //m_domainFiles.Added += argument => { argument.File.SaveStateChanged += () => { if (!argument.File.Changed) ReloadDatasource(); }; };
            //m_domainFiles.Reloaded += (from, to) => { to.File.SaveStateChanged += () => { if (!to.File.Changed) ReloadDatasource(); }; };
            //m_domainFiles.ForAll(d => d.File.SaveStateChanged += () => { if (!d.File.Changed) ReloadDatasource(); });

            m_domainUsage = new DomainUsage(this);
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

        public void OnElementMoved(FileInfo from, FileInfo to)
        {
            if (from.FullName != to.FullName)
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
            FileInfo file = new FileInfo(Path.IsPathRooted(path) ? path : Path.Combine(origin.FullName, path));
            return file.AncestorOf(origin);
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
                    throw new Exception("Attempting to import file from a folder than is not a subfolder of the project's parent folder");
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
            serializer.Write(new Project.TData(conversationPaths, domainPaths, localizationPaths, audioPaths, null, null), stream); //TODO: Last Edited / Selected localizer
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

        private void ContentMoved(FileInfo a, FileInfo b)
        {
            GotChanged();
        }

        private void ReloadDatasource()
        {
            m_conversationDataSource = new ConversationDataSource(BaseTypeSet.Make(), m_domainFiles.Select(f => f.Data));
            var serializer = m_conversationSerializerFactory(m_conversationDataSource);
            m_conversations.Reload(); //Reload all conversations

            DataSourceChanged.Execute();
        }

        public event Action DataSourceChanged;

        public bool CanModifyConversations
        {
            get { return DomainFiles.All(f => !f.File.Changed); }
        }

        public bool CanModifyDomain
        {
            get { return Conversations.All(f => !f.File.Changed); }
        }

        private readonly IAudioProvider m_audioProvider;
        public IAudioProvider AudioProvider
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

        public void Dispose()
        {
            foreach (var element in ElementsExceptThis)
                element.Dispose();
            File.Dispose();
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Conversation;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGui>;
    using Utilities;

    internal sealed class DummyProject : IProject
    {
        public static readonly IProject Instance = new DummyProject();

        private DummyProject() { }

        public ISaveableFile File { get { return new MissingFile(null); } }

        IAudioLibrary IProject.AudioProvider { get { return null; } }

        IProjectElementList<LocalizationFile, ILocalizationFile> IProject.LocalizationFiles
        {
            get { return DummyProjectElementList<LocalizationFile, ILocalizationFile>.Instance; }
        }

        IDomainDataSource IProject.DomainDataSource
        {
            get { return DummyDataSource.Instance; }
        }

        public IProjectElementList<ConversationFile, IConversationFile> Conversations
        {
            get { return DummyProjectElementList<ConversationFile, IConversationFile>.Instance; }
        }

        public IProjectElementList<DomainFile, IDomainFile> DomainFiles
        {
            get { return DummyProjectElementList<DomainFile, IDomainFile>.Instance; }
        }

        IEnumerable<ISaveableFileProvider> IProject.Elements
        {
            get { return Enumerable.Empty<ISaveableFileProvider>(); }
        }

        IEnumerable<ISaveableFileProvider> IProject.ElementsExceptThis
        {
            get { return Enumerable.Empty<ISaveableFileProvider>(); }
        }

        DirectoryInfo IProject.Origin
        {
            get { return new DirectoryInfo("."); }
        }

        IDataSource IProject.ConversationDataSource
        {
            get { return DummyDataSource.Instance; }
        }

        bool IProject.CanModifyConversations
        {
            get { throw new NotImplementedException(); }
        }

        bool IProject.CanModifyDomain
        {
            get { throw new NotImplementedException(); }
        }

        LocalizationEngine IProject.Localizer
        {
            get { return new LocalizationEngine(null, () => new HashSet<Id<LocalizedText>>(), s => false, s => false, p => true, s => true); }
        }

        IProjectElementList<AudioFile, IAudioFile> IProject.AudioFiles
        {
            get { return DummyProjectElementList<AudioFile, IAudioFile>.Instance; }
        }

        void IProject.GotChanged()
        {
            throw new NotImplementedException();
        }

        IDomainUsage<ConversationNode, TransitionNoduleUIInfo> IProject.DomainUsage
        {
            get { return null; }
        }

        bool IProject.ReloadConversationDatasourceIfRequired()
        {
            return false;
        }

        public IEnumerable<IDomainFile> DomainFilesCollection
        {
            get { return DomainFiles; }
        }

        public IEnumerable<IConversationFile> ConversationFilesCollection
        {
            get { return Conversations; }
        }

        public event Action FileModifiedExternally { add { } remove { } }

        public event Action FileDeletedExternally { add { } remove { } }

        public void Dispose()
        {
        }

        IEnumerable<FileInfo> IProject.Rerout(IEnumerable<string> paths)
        {
            throw new NotImplementedException();
        }

        void IProject.Renamed(ProjectExplorer.FileSystemObject item, string from, string to)
        {
            throw new NotImplementedException();
        }

        Func<IParameter, string, Func<ParameterType, DynamicEnumParameter.Source>, IEnumerable<string>> IProject.AutoCompleteSuggestions { get { return null; } }
    }
}

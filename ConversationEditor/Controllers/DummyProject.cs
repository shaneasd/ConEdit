using System;
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
        UpToDateFile.BackEnd m_backend = new UpToDateFile.BackEnd();
        public static readonly IProject Instance = new DummyProject();

        private DummyProject() { }

        public ISaveableFile File => new MissingFile(Id<FileInProject>.FromGuid(Guid.Empty), DocumentPath.FromPath("", new DirectoryInfo(".")));

        IAudioLibrary IProject.AudioProvider => null;

        IProjectElementList<ILocalizationFile> IProject.LocalizationFiles => DummyProjectElementList<LocalizationFile, ILocalizationFile>.Instance;

        IDomainDataSource IProject.DomainDataSource => DummyDataSource.Instance;

        public IProjectElementList<IConversationFile> Conversations => DummyProjectElementList<ConversationFile, IConversationFile>.Instance;

        public IProjectElementList<IDomainFile> DomainFiles => DummyProjectElementList<DomainFile, IDomainFile>.Instance;

        IEnumerable<ISaveableFileProvider> IProject.Elements => Enumerable.Empty<ISaveableFileProvider>();

        IEnumerable<ISaveableFileProvider> IProject.ElementsExceptThis => Enumerable.Empty<ISaveableFileProvider>();

        DirectoryInfo IProject.Origin => new DirectoryInfo(".");

        IDataSource IProject.ConversationDataSource => DummyDataSource.Instance;

        bool IProject.CanModifyConversations => throw new NotImplementedException();

        bool IProject.CanModifyDomain => throw new NotImplementedException();

        LocalizationEngine IProject.Localizer => new LocalizationEngine(null, Enumerable.Empty<Project.TData.LocalizerSetData>(), null, () => new HashSet<Id<LocalizedText>>(), s => false, p => true, s => true, m_backend, null);

        IProjectElementList<IAudioFile> IProject.AudioFiles => DummyProjectElementList<AudioFile, IAudioFile>.Instance;

        void IProject.GotChanged()
        {
            throw new NotImplementedException();
        }

        IDomainUsage<ConversationNode, TransitionNoduleUIInfo> IProject.DomainUsage => null;

        bool IProject.ReloadConversationDatasourceIfRequired() => false;

        public IEnumerable<IDomainFile> DomainFilesCollection => DomainFiles;

        public IEnumerable<IConversationFile> ConversationFilesCollection => Conversations;

        public event Action FileModifiedExternally { add { } remove { } }

        public event Action FileDeletedExternally { add { } remove { } }

        public void Dispose()
        {
            m_backend.Dispose();
        }

        public IEnumerable<string> AutoCompleteSuggestions(IParameter parameter, string start, IConversationEditorControlData<ConversationNode<INodeGui>, TransitionNoduleUIInfo> document)
        {
            throw new NotSupportedException();
        }
    }
}

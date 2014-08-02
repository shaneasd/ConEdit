using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Conversation;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGUI>;

    public class DummyProject : IProject
    {
        public static readonly IProject Instance = new DummyProject();

        private DummyProject() { }

        ISaveableFile IProject.File { get { return new MissingFile(null); } }

        IAudioProvider IProject.AudioProvider { get { return null; } }

        IProjectElementList<LocalizationFile, ILocalizationFile> IProject.LocalizationFiles
        {
            get { return DummyProjectElementList<LocalizationFile, ILocalizationFile>.Instance; }
        }

        IDataSource IProject.DomainDataSource
        {
            get { return DummyDataSource.Instance; }
        }

        IProjectElementList<ConversationFile, IConversationFile> IProject.Conversations
        {
            get { return DummyProjectElementList<ConversationFile, IConversationFile>.Instance; }
        }

        IProjectElementList<DomainFile, IDomainFile> IProject.DomainFiles
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

        Conversation.IDataSource IProject.ConversationDataSource
        {
            get { return DummyDataSource.Instance; }
        }

        event Action IProject.DataSourceChanged { add { } remove { } }

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
            get { return new LocalizationEngine(() => new HashSet<ID<LocalizedText>>(), s => false, s => false, p => true, s => true); }
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
    }
}

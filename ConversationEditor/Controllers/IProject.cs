using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGUI>;

    public interface IProject
    {
        IDataSource DomainDataSource { get; }
        IDataSource ConversationDataSource { get; }
        bool ReloadConversationDatasourceIfRequired();
        DirectoryInfo Origin { get; }

        ISaveableFile File { get; }
        void GotChanged();

        IProjectElementList<ConversationFile, IConversationFile> Conversations { get; }
        IProjectElementList<LocalizationFile, ILocalizationFile> LocalizationFiles { get; }
        IProjectElementList<DomainFile, IDomainFile> DomainFiles { get; }
        IProjectElementList<AudioFile, IAudioFile> AudioFiles { get; }
        IEnumerable<ISaveableFileProvider> Elements { get; }
        IEnumerable<ISaveableFileProvider> ElementsExceptThis { get; }

        LocalizationEngine Localizer { get; }
        IAudioProvider AudioProvider { get; }
        IDomainUsage<ConversationNode, TransitionNoduleUIInfo> DomainUsage { get; }

        /// <summary>
        /// Currently it is impossible to modify conversations while there are unsaved changes to the domain
        /// </summary>
        bool CanModifyConversations { get; }

        /// <summary>
        /// Currently it is impossible to modify the domain while there are unsaved changes to conversations
        /// </summary>
        bool CanModifyDomain { get; }
    }

}

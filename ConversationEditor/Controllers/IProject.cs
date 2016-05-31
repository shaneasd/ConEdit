using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGui>;

    internal interface IProject : IProject2
    {
        IDataSource DomainDataSource { get; }
        IDataSource ConversationDataSource { get; }
        bool ReloadConversationDatasourceIfRequired();
        DirectoryInfo Origin { get; }
        IEnumerable<FileInfo> Rerout(IEnumerable<string> paths);

        void GotChanged();

        IProjectElementList<ConversationFile, IConversationFile> Conversations { get; }
        IProjectElementList<LocalizationFile, ILocalizationFile> LocalizationFiles { get; }
        IProjectElementList<DomainFile, IDomainFile> DomainFiles { get; }
        IProjectElementList<AudioFile, IAudioFile> AudioFiles { get; }
        IEnumerable<ISaveableFileProvider> Elements { get; }
        IEnumerable<ISaveableFileProvider> ElementsExceptThis { get; }

        LocalizationEngine Localizer { get; }
        IAudioLibrary AudioProvider { get; }
        IDomainUsage<ConversationNode, TransitionNoduleUIInfo> DomainUsage { get; }

        /// <summary>
        /// Currently it is impossible to modify conversations while there are unsaved changes to the domain
        /// </summary>
        bool CanModifyConversations { get; }

        /// <summary>
        /// Currently it is impossible to modify the domain while there are unsaved changes to conversations
        /// </summary>
        bool CanModifyDomain { get; }

        /// <summary>
        /// For an input parameter, current user entered value of that parameter and source for local data, suggest values extending upon that value
        /// </summary>
        Func<IParameter, string, Func<ParameterType, DynamicEnumParameter.Source>, IEnumerable<string>> AutoCompleteSuggestions { get; }
    }

}

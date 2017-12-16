﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

namespace ConversationEditor
{
    using ConversationNode = ConversationNode<INodeGui>;

    public interface IProject : IProject2
    {
        IDomainDataSource DomainDataSource { get; }
        IDataSource ConversationDataSource { get; }
        bool ReloadConversationDatasourceIfRequired();
        DirectoryInfo Origin { get; }

        void GotChanged();

        IProjectElementList<IConversationFile> Conversations { get; }
        IProjectElementList<ILocalizationFile> LocalizationFiles { get; }
        IProjectElementList<IDomainFile> DomainFiles { get; }
        IProjectElementList<IAudioFile> AudioFiles { get; }
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

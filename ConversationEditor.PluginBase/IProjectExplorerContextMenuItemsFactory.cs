using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
    public interface IDomainContextMenuItem
    {
        string Name { get; }
        void Execute(IDomainFile domain);
    }

    public interface IConversationContextMenuItem
    {
        string Name { get; }
        void Execute(IConversationFile conversation, IErrorCheckerUtilities<IConversationNode> util);
    }

    public interface ILocalizationContextMenuItem
    {
        string Name { get; }
        void Execute(ILocalizationFile localization);
    }

    public interface IAudioContextMenuItem
    {
        string Name { get; }
        void Execute(IAudioFile audio);
    }

    public interface IFolderContextMenuItem
    {
        string Name { get; }
        void Execute(IEnumerable<IConversationFile> conversations);
    }

    public interface IProjectExplorerContextMenuItemsFactory
    {
        IEnumerable<IDomainContextMenuItem> DomainContextMenuItems { get; }
        IEnumerable<IConversationContextMenuItem> ConversationContextMenuItems(Func<Id<LocalizedStringType>, Id<LocalizedText>, Tuple<string, DateTime>> localizer);
        IEnumerable<ILocalizationContextMenuItem> LocalizationContextMenuItems { get; }
        IEnumerable<IAudioContextMenuItem> AudioContextMenuItems { get; }
        IEnumerable<IFolderContextMenuItem> FolderContextMenuItems(Func<Id<LocalizedStringType>, Id<LocalizedText>, Tuple<string, DateTime>> localizer);
    }
}

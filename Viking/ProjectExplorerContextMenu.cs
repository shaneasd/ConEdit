using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using Conversation;

namespace Viking
{
    public class ProjectExplorerContextMenu : IProjectExplorerContextMenuItemsFactory
    {
        public IEnumerable<IDomainContextMenuItem> DomainContextMenuItems => Enumerable.Empty<IDomainContextMenuItem>();

        public IEnumerable<IConversationContextMenuItem> ConversationContextMenuItems(Func<Id<LocalizedStringType>, Id<LocalizedText>, Tuple<string, DateTime>> localizer)
        {
            yield return new ExportLimitedLocalization(localizer);
            //yield return new ExportLocalizationLines(localizer); //TODO: Archive this version somewhere
        }

        public IEnumerable<ILocalizationContextMenuItem> LocalizationContextMenuItems => Enumerable.Empty<ILocalizationContextMenuItem>();

        public IEnumerable<IAudioContextMenuItem> AudioContextMenuItems => Enumerable.Empty<IAudioContextMenuItem>();

        public IEnumerable<IFolderContextMenuItem> FolderContextMenuItems(Func<Id<LocalizedStringType>, Id<LocalizedText>, Tuple<string, DateTime>> localizer)
        {
            yield return new ExportLimitedLocalization(localizer);
        }
    }
}

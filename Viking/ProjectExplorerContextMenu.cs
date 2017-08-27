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
        public IEnumerable<IDomainContextMenuItem> DomainContextMenuItems
        {
            get { return Enumerable.Empty<IDomainContextMenuItem>(); }
        }

        public IEnumerable<IConversationContextMenuItem> ConversationContextMenuItems(Func<Id<LocalizedText>, Tuple<string, DateTime>> localizer)
        {
            yield return new ExportLimitedLocalization(localizer);
            //yield return new ExportLocalizationLines(localizer); //TODO: Archive this version somewhere
        }

        public IEnumerable<ILocalizationContextMenuItem> LocalizationContextMenuItems
        {
            get { return Enumerable.Empty<ILocalizationContextMenuItem>(); }
        }

        public IEnumerable<IAudioContextMenuItem> AudioContextMenuItems
        {
            get { return Enumerable.Empty<IAudioContextMenuItem>(); }
        }

        public IEnumerable<IFolderContextMenuItem> FolderContextMenuItems(Func<Id<LocalizedText>, Tuple<string, DateTime>> localizer)
        {
            yield return new ExportLimitedLocalization(localizer);
        }
    }
}

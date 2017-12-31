using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using Conversation;

namespace Clandestine
{
    public class ProjectExplorerContextMenu : IProjectExplorerContextMenuItemsFactory
    {
        public IEnumerable<IDomainContextMenuItem> DomainContextMenuItems => Enumerable.Empty<IDomainContextMenuItem>();

        public IEnumerable<IConversationContextMenuItem> ConversationContextMenuItems(Func<Id<LocalizedStringType>, Id<LocalizedText>, Tuple<string, DateTime>> localizer)
        {
            yield return new ExportAsCsv(localizer);
            yield return new ExportAsSsv(localizer);
        }

        public IEnumerable<ILocalizationContextMenuItem> LocalizationContextMenuItems => Enumerable.Empty<ILocalizationContextMenuItem>();

        public IEnumerable<IAudioContextMenuItem> AudioContextMenuItems => Enumerable.Empty<IAudioContextMenuItem>();

        public IEnumerable<IFolderContextMenuItem> FolderContextMenuItems(Func<Id<LocalizedStringType>, Id<LocalizedText>, Tuple<string, DateTime>> localizer)
        {
            return Enumerable.Empty<IFolderContextMenuItem>();
        }
    }
}

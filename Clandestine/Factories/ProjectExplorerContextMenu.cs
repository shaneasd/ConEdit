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
        public IEnumerable<IDomainContextMenuItem> DomainContextMenuItems
        {
            get { return Enumerable.Empty<IDomainContextMenuItem>(); }
        }

        public IEnumerable<IConversationContextMenuItem> ConversationContextMenuItems(Func<Id<LocalizedText>, string> localizer)
        {
            yield return new ExportAsCsv(localizer);
            yield return new ExportAsSsv(localizer);
        }

        public IEnumerable<ILocalizationContextMenuItem> LocalizationContextMenuItems
        {
            get { return Enumerable.Empty<ILocalizationContextMenuItem>(); }
        }

        public IEnumerable<IAudioContextMenuItem> AudioContextMenuItems
        {
            get { return Enumerable.Empty<IAudioContextMenuItem>(); }
        }
    }
}

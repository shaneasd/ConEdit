using ConversationEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Conversation;

namespace RawSerialization
{
    public class ProjectExplorerContextMenu : IProjectExplorerContextMenuItemsFactory
    {
        public IEnumerable<IAudioContextMenuItem> AudioContextMenuItems
        {
            get { return Enumerable.Empty<IAudioContextMenuItem>(); }
        }

        public IEnumerable<IDomainContextMenuItem> DomainContextMenuItems
        {
            get { return Enumerable.Empty<IDomainContextMenuItem>(); }
        }

        public IEnumerable<IFolderContextMenuItem> FolderContextMenuItems(Func<Id<LocalizedText>, Tuple<string, DateTime>> localizer)
        {
            return Enumerable.Empty<IFolderContextMenuItem>();
        }

        public IEnumerable<ILocalizationContextMenuItem> LocalizationContextMenuItems
        {
            get
            {
                yield return new GenerateRawLocalization();
            }
        }

        public IEnumerable<IConversationContextMenuItem> ConversationContextMenuItems(Func<Id<LocalizedText>, Tuple<string, DateTime>> localizer)
        {
            return Enumerable.Empty<IConversationContextMenuItem>();
        }
    }
}

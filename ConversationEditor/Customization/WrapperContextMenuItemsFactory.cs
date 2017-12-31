using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    internal class WrapperContextMenuItemsFactory : IProjectExplorerContextMenuItemsFactory
    {
        private Func<MainAssembly, IEnumerable<PluginAssembly>> m_plugins;

        public WrapperContextMenuItemsFactory(Func<MainAssembly, IEnumerable<PluginAssembly>> plugins)
        {
            m_plugins = plugins;
        }

        private IEnumerable<IProjectExplorerContextMenuItemsFactory> PluginContextMenuFactories()
        {
            List<IProjectExplorerContextMenuItemsFactory> result = new List<IProjectExplorerContextMenuItemsFactory>();
            foreach (var pa in m_plugins(MainAssembly.Include))
            {
                var factories = pa.Assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IProjectExplorerContextMenuItemsFactory)));
                foreach (var factory in factories)
                {
                    var constructor = factory.GetConstructor(Type.EmptyTypes);
                    if (constructor != null)
                    {
                        IProjectExplorerContextMenuItemsFactory obj = constructor.Invoke(new object[0]) as IProjectExplorerContextMenuItemsFactory;
                        result.Add(obj);
                    }
                }
            }
            return result;
        }

        public IEnumerable<IDomainContextMenuItem> DomainContextMenuItems => PluginContextMenuFactories().SelectMany(p => p.DomainContextMenuItems);

        public IEnumerable<IConversationContextMenuItem> ConversationContextMenuItems(Func<Id<LocalizedStringType>, Id<LocalizedText>, Tuple<string, DateTime>> localizer)
        {
            return PluginContextMenuFactories().SelectMany(p => p.ConversationContextMenuItems(localizer));
        }

        public IEnumerable<ILocalizationContextMenuItem> LocalizationContextMenuItems => PluginContextMenuFactories().SelectMany(p => p.LocalizationContextMenuItems);

        public IEnumerable<IAudioContextMenuItem> AudioContextMenuItems => PluginContextMenuFactories().SelectMany(p => p.AudioContextMenuItems);

        public IEnumerable<IFolderContextMenuItem> FolderContextMenuItems(Func<Id<LocalizedStringType>, Id<LocalizedText>, Tuple<string, DateTime>> localizer)
        {
            return PluginContextMenuFactories().SelectMany(p => p.FolderContextMenuItems(localizer));
        }
    }
}

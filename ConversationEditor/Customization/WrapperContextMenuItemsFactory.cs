using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    internal class WrapperContextMenuItemsFactory : IProjectExplorerContextMenuItemsFactory
    {
        private Func<MainAssemblies, IEnumerable<PluginAssembly>> m_plugins;

        public WrapperContextMenuItemsFactory(Func<MainAssemblies, IEnumerable<PluginAssembly>> plugins)
        {
            m_plugins = plugins;
        }

        private IEnumerable<IProjectExplorerContextMenuItemsFactory> PluginContextMenuFactories()
        {
            List<IProjectExplorerContextMenuItemsFactory> result = new List<IProjectExplorerContextMenuItemsFactory>();
            foreach (var pa in m_plugins(MainAssemblies.Include))
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

        public IEnumerable<IDomainContextMenuItem> DomainContextMenuItems
        {
            get { return PluginContextMenuFactories().SelectMany(p => p.DomainContextMenuItems); }
        }

        public IEnumerable<IConversationContextMenuItem> ConversationContextMenuItems(Func<Id<LocalizedText>, Tuple<string, DateTime>> localizer)
        {
            return PluginContextMenuFactories().SelectMany(p => p.ConversationContextMenuItems(localizer));
        }

        public IEnumerable<ILocalizationContextMenuItem> LocalizationContextMenuItems
        {
            get { return PluginContextMenuFactories().SelectMany(p => p.LocalizationContextMenuItems); }
        }

        public IEnumerable<IAudioContextMenuItem> AudioContextMenuItems
        {
            get { return PluginContextMenuFactories().SelectMany(p => p.AudioContextMenuItems); }
        }

        public IEnumerable<IFolderContextMenuItem> FolderContextMenuItems(Func<Id<LocalizedText>, Tuple<string, DateTime>> localizer)
        {
            return PluginContextMenuFactories().SelectMany(p => p.FolderContextMenuItems(localizer));
        }
    }
}

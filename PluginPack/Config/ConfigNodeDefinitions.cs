using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using ConversationEditor;

namespace PluginPack
{
    public class ConfigNodeDefinitionsFactory : IConfigNodeDefinitionFactory
    {
        public ConfigNodeDefinitionsFactory()
        {
        }

        public IEnumerable<IConfigNodeDefinition> GetConfigNodeDefinitions()
        {
            yield return new IconConfig();
            yield return new ShortcutKey();
            yield return new BackgroundColor();
            yield return new NameConfig();
        }
    }




}

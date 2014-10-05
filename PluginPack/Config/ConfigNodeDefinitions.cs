using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using ConversationEditor;

namespace PluginPack
{
    public class ConfigNodeDefinitions : IConfigNodeDefinitionFactory
    {
        public ConfigNodeDefinitions()
        {
        }

        IEnumerable<IConfigNodeDefinition> IConfigNodeDefinitionFactory.ConfigNodeDefinitions()
        {
            yield return new IconConfig();
            yield return new ShortcutKey();
            yield return new BackgroundColor();
            yield return new NameConfig();
        }
    }




}

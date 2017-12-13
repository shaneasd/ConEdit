using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

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
            yield return new BackgroundColor();
            yield return new NameConfig();
            yield return new DoNotRenderConfig();
        }
    }




}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace Clandestine
{
    public sealed class ConfigNodeDefinitionsFactory : IConfigNodeDefinitionFactory
    {
        public ConfigNodeDefinitionsFactory()
        {
        }

        public IEnumerable<IConfigNodeDefinition> GetConfigNodeDefinitions()
        {
            yield return new DoNotRenderConfig();
        }
    }
}

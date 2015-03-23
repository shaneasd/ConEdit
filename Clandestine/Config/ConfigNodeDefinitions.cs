using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using ConversationEditor;

namespace Clandestine
{
    public class ConfigNodeDefinitions : IConfigNodeDefinitionFactory
    {
        public ConfigNodeDefinitions()
        {
        }

        IEnumerable<IConfigNodeDefinition> IConfigNodeDefinitionFactory.ConfigNodeDefinitions()
        {
            yield return new DontRenderConfig();
        }
    }




}

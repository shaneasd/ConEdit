using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using ConversationEditor;

namespace PluginPack.Config
{
    public class ConfigNodeDefinitions : IConfigNodeDefinitionFactory
    {
        public ConfigNodeDefinitions()
        {
        }

        public class ShortcutKey : IConfigNodeDefinition
        {
            public ID<NodeTypeTemp> Id
            {
                get { return ID<NodeTypeTemp>.Parse("d2ca0f2a-abed-4000-a41c-7f55a7652585"); }
            }

            public string Name
            {
                get { return "Shortcut Key"; }
            }

            public IEnumerable<Parameter> MakeParameters()
            {
                yield return new StringParameter("Key", ID<Parameter>.Parse("70de2664-9d86-470d-b3b5-2a23e5afae91"), BaseTypeString.PARAMETER_TYPE);
            }
        }

        public class BackgroundColor : IConfigNodeDefinition
        {
            public ID<NodeTypeTemp> Id
            {
                get { return ID<NodeTypeTemp>.Parse("5887131d-47aa-49ac-b73f-2e21a176af16"); }
            }

            public string Name
            {
                get { return "Color"; }
            }

            public IEnumerable<Parameter> MakeParameters()
            {
                yield return new StringParameter("Color", ID<Parameter>.Parse("9baa396d-265d-4b69-8a19-0f4799606a3a"), BaseTypeString.PARAMETER_TYPE);
            }
        }

        IEnumerable<IConfigNodeDefinition> IConfigNodeDefinitionFactory.ConfigNodeDefinitions()
        {
            yield return new ShortcutKey();
            yield return new BackgroundColor();
        }
    }
}

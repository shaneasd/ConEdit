using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using ConversationEditor;

namespace PluginPack
{
    public class BackgroundColor : IConfigNodeDefinition
    {
        public Id<NodeTypeTemp> Id => Id<NodeTypeTemp>.Parse("5887131d-47aa-49ac-b73f-2e21a176af16");

        public string Name => "Color";

        public IEnumerable<IParameter> MakeParameters()
        {
            yield return new StringParameter("Color", Id<Parameter>.Parse("9baa396d-265d-4b69-8a19-0f4799606a3a"));
        }
    }
}

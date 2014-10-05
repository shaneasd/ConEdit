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
}

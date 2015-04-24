using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using ConversationEditor;

namespace PluginPack
{
    public class ShortcutKey : IConfigNodeDefinition
    {
        public ID<NodeTypeTemp> Id
        {
            get { return ID<NodeTypeTemp>.Parse("e4914511-fd9d-428f-8e5c-b5c86cf069a9"); }
        }

        public string Name
        {
            get { return "Shortcut Key"; }
        }

        public IEnumerable<Parameter> MakeParameters()
        {
            yield return new StringParameter("Key", ID<Parameter>.Parse("70de2664-9d86-470d-b3b5-2a23e5afae91"), StringParameter.ParameterType);
        }
    }
}

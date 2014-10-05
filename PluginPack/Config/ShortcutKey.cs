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
}

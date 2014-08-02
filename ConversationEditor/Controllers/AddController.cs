using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Drawing;

namespace ConversationEditor
{
    public class AddController
    {
        private Action<Func<uint, IEditable>> AddNode;

        public AddController(Action<Func<uint, IEditable>> addNode)
        {
            AddNode = addNode;
        }

        public void AddJumpTo(DynamicEnumParameter.Source jumpSource)
        {
            AddNode();
        }
    }
}

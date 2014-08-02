using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    public class IdProvider
    {
        public IdProvider()
        {
        }
        public NodeID Next() { return new NodeID(); }
    }
}

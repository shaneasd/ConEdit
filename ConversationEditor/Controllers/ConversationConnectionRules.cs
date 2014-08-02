using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    public class ConversationConnectionRules : IConnectionRules
    {
        public static readonly ConversationConnectionRules Instance = new ConversationConnectionRules();

        public bool CanConnect(ID<TConnectorDefinition> a, ID<TConnectorDefinition> b)
        {
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public abstract class ConversationError<T> where T : IConversationNode
    {
        public abstract string Message { get; }
        public IEnumerable<T> Nodes { get; }

        public override string ToString()
        {
            return Message;
        }
        protected ConversationError(IEnumerable<T> nodes)
        {
            Nodes = nodes;
        }
    }
}

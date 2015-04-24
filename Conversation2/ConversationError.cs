using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public abstract class ConversationError<T> where T : IConversationNode
    {
        public abstract string Message { get; }
        private readonly IEnumerable<T> m_nodes;

        public IEnumerable<T> Nodes { get { return m_nodes; } }

        public override string ToString()
        {
            return Message;
        }
        protected ConversationError(IEnumerable<T> nodes)
        {
            m_nodes = nodes;
        }
    }
}

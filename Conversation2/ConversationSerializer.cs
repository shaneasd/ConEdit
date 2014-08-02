using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public interface IConversationSerializer<in TNode>
    {
        void Write(IEnumerable<TNode> nodes);
    }
}

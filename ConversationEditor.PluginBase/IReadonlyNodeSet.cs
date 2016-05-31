using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    public interface IReadonlyNodeSet
    {
        IEnumerable<Id<NodeTemp>> Nodes { get; }
        IEnumerable<NodeGroup> Groups { get; }
        int Count();
    }
}

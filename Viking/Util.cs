using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace Viking
{
    internal static class Util
    {
        internal static bool IsAIBark(Id<NodeTypeTemp> type, IErrorCheckerUtilities<IConversationNode> utils)
        {
            return utils.GetCategory(type) == Guid.Parse("65fe5351-37b0-44c8-b834-895b55e5c454");
        }

        internal static bool IsStartNode(Id<NodeTypeTemp> type, IErrorCheckerUtilities<IConversationNode> utils)
        {
            Id<NodeTypeTemp> startId = Id<NodeTypeTemp>.Parse("656a48b2-324e-4484-a1b1-c3b91ad10c3e");
            Id<NodeTypeTemp> returnJumpTarget = Id<NodeTypeTemp>.Parse("8a00b0d2-0425-441b-a94c-7648cbeb7c3a");
            return type == startId || type == returnJumpTarget || IsAIBark(type, utils);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace Clandestine
{
    internal static class Util
    {
        internal static bool IsAIBark(Id<NodeTypeTemp> type, IErrorCheckerUtilities<IConversationNode> utils)
        {
            return utils.GetCategory(type) == Guid.Parse("5ee1cde6-a1d4-4fe5-9607-b782f324da4e");
        }

        internal static bool IsStartNode(Id<NodeTypeTemp> type, IErrorCheckerUtilities<IConversationNode> utils)
        {
            return type == SpecialNodes.Start || type == SpecialNodes.StartRadio || IsAIBark(type, utils);
        }
    }
}

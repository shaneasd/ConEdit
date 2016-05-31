using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Drawing;
using Utilities;

namespace ConversationEditor
{
    internal static class INodeGuiUtils
    {
        public static void Offset(this IGui node, PointF offset)
        {
            node.MoveTo(node.Area.Center().Plus(offset));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Drawing;
using Utilities;

namespace ConversationEditor
{
    public static class INodeGUIUtils
    {
        public static void Offset(this IGUI node, PointF offset)
        {
            node.MoveTo(node.Area.Center().Plus(offset));
        }
    }

    public interface INodeGUI : INodeUI<INodeGUI>, IGUI
    {
        string DisplayName { get; }
    }
}

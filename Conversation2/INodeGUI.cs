using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Utilities;

namespace Conversation
{
    public interface IGUI
    {
        RectangleF Area { get; }
        void UpdateArea();
        void Draw(Graphics g, bool selected);
        void MoveTo(PointF point);
    }

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Conversation;

namespace ConversationEditor
{
    public class NodeUIData
    {
        public RectangleF Area;

        public NodeUIData() { }
        public NodeUIData(PointF p)
        {
            Area = RectangleF.FromLTRB(p.X - 10, p.Y - 10, p.X + 10, p.Y + 10);
        }

        public static NodeUIData Make(IGui renderer)
        {
            return new NodeUIData { Area = renderer.Area };
        }
    }
}

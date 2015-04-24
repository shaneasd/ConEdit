using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGUI>;
using System.Drawing.Drawing2D;
using Utilities.UI;

namespace ConversationEditor
{
    internal class UnknownNodeRenderer : NodeUI
    {
        static Font Font = SystemFonts.DefaultFont;

        public UnknownNodeRenderer(ConversationNode<INodeGUI> node, PointF p) :
            base(node, p)
        {
        }

        public override string DisplayName
        {
            get { return "Unknown Node Renderer"; }
        }

        protected override void InnerDraw(System.Drawing.Graphics g, bool selected)
        {
            using (Brush background = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.White, Color.LightGray))
            {
                g.FillRectangle(background, Area);
            }
            var l = Area.Location.X;
            var r = Area.Location.X + Area.Width;
            var t = Area.Location.Y;
            var b = Area.Location.Y + Area.Height;
            var pen = new Pen(Brushes.Red, 2);
            g.DrawRectangle(pen, Area);
            g.DrawString(Text, Font, Brushes.Black, Area.Location);
        }

        public string Text
        {
            get
            {
                return Node.Parameters.Aggregate(Node.NodeName, (a, p) => a + "\r\n  -  " + p.Name, s => s);
            }
        }

        protected override SizeF CalculateArea(System.Drawing.Graphics g)
        {
            return g.MeasureString(Text, Font);
            //return new SizeF(30, 30);
        }
    }

}

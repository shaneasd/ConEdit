using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
using Utilities.UI;

namespace ConversationEditor
{
    internal class CorruptedNodeRenderer : NodeUI
    {
        static Font Font = SystemFonts.DefaultFont;

        public CorruptedNodeRenderer(ConversationNode<INodeGui> node, PointF p) :
            base(node, p)
        {
        }

        protected override void InnerDraw(System.Drawing.Graphics g, bool selected)
        {
            g.FillRectangle(Brushes.White, Area);
            //var l = Area.Location.X;
            //var r = Area.Location.X + Area.Width;
            //var t = Area.Location.Y;
            //var b = Area.Location.Y + Area.Height;
            using (var pen = new Pen(Brushes.Red, 2))
            {
                g.DrawRectangle(pen, Area);
                //g.DrawLine(pen, l, t, r, b);
                //g.DrawLine(pen, l, b, r, t);
            }
            g.DrawString(Text, Font, Brushes.Black, Area.Location);
        }

        public string Text => Node.Data.Parameters.Where(p => p.Corrupted).Aggregate("Corrupted " + Node.Data.Name + " ID: " + Node.Data.NodeId.Guid, (a, p) => a + "\r\n  -  " + p.Name, s => s);

        protected override SizeF CalculateArea(System.Drawing.Graphics g)
        {
            return g.MeasureString(Text, Font);
            //return new SizeF(30, 30);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Utilities;

namespace ConversationEditor
{
    public class TransitionNoduleUIInfo
    {
        public TransitionNoduleUIInfo(RectangleF area)
        {
            Area.Value = area;
        }

        public NotifierProperty<RectangleF> Area = new NotifierProperty<RectangleF>(RectangleF.Empty);

        public void Draw(Graphics g, Color color)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(color, 2))
            {
                g.DrawEllipse(pen, Area.Value);
            }
        }
    }
}

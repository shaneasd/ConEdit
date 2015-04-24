using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ConversationEditor
{
    public class TransitionNoduleUIInfo
    {
        public TransitionNoduleUIInfo(Func<RectangleF> area)
        {
            m_area = area;
        }

        private readonly Func<RectangleF> m_area;
        public RectangleF Area
        {
            get { return m_area(); }
        }

        public void Draw(Graphics g, Color color)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(color, 2))
            {
                g.DrawEllipse(pen, Area);
            }
        }
    }
}

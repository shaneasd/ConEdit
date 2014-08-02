using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ConversationEditor
{
    public class TransitionNoduleUIInfo
    {
        public TransitionNoduleUIInfo(Func<Rectangle> area)
        {
            m_area = area;
        }

        private readonly Func<Rectangle> m_area;
        public Rectangle Area
        {
            get { return m_area(); }
        }

        public void Draw(Graphics g)
        {
            Pen pen = new Pen(Brushes.Black, 2);
            g.DrawEllipse(pen, Area);
        }
    }
}

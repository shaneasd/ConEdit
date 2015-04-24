using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Utilities.UI
{
    public static class GraphicsUtil
    {
        public static IDisposable SaveState(this Graphics g)
        {
            return new RestorableGraphicsState(g);
        }
        private class RestorableGraphicsState : IDisposable
        {
            private GraphicsState m_state;
            private Graphics m_graphics;
            public RestorableGraphicsState(Graphics g)
            {
                m_graphics = g;
                m_state = g.Save();
            }
            public void Dispose()
            {
                m_graphics.Restore(m_state);
            }
        }
    }
}

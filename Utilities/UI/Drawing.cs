using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Utilities.UI
{
    public static class Drawing
    {
        public static void DrawRectangle(this Graphics g, Pen pen, RectangleF rect)
        {
            g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}

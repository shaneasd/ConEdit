using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ConversationEditor
{
    public interface IGUI
    {
        RectangleF Area { get; }
        void UpdateArea();
        void Draw(Graphics g, bool selected, ColorScheme scheme);
        void MoveTo(PointF point);
    }
}

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
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Utilities;

namespace ConversationEditor
{
    public interface IGui
    {
        RectangleF Area { get; }
        void UpdateArea();
        void Draw(Graphics g, bool selected, ColorScheme scheme);
        void MoveTo(PointF location);
        event Action<Changed<RectangleF>> AreaChanged;
    }
}

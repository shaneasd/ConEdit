using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utilities.UI
{
    public static class ButtonDrawer
    {
        public enum ButtonState
        {
            Neutral,
            Hovered,
            Pressed
        }

        static Color NeutralTopColour { get; } = Color.FromArgb(61, 61, 61);
        static Color NeutralBottomColour { get; } = Color.FromArgb(42, 42, 42);
        static Pen NeutralOutline => Pens.Black;
        static Pen HoveredOutline { get; } = new Pen(Color.FromArgb(153, 153, 153));
        static Pen PressedOutline { get; } = new Pen(Color.FromArgb(153, 153, 153));
        static Color HoveredTopColour { get; } = Color.FromArgb(61, 61, 61);
        static Color PressedTopColour { get; } = Color.FromArgb(42, 42, 42);
        static Color HoveredBottomColour { get; } = Color.FromArgb(42, 42, 42);
        static Color PressedBottomColour { get; } = Color.FromArgb(61, 61, 61);

        static Rectangle InnerRectangle(Rectangle rect) => Rectangle.FromLTRB(rect.Left + 1, rect.Top + 1, rect.Right - 1, rect.Bottom - 1);

        static LinearGradientBrush MakeGradient(Rectangle rect, ButtonState state)
        {
            switch (state)
            {
                case ButtonState.Neutral: return new LinearGradientBrush(new Point(0, rect.Top + 1), new PointF(0, rect.Bottom - 1), NeutralTopColour, NeutralBottomColour);
                case ButtonState.Hovered: return new LinearGradientBrush(new Point(0, rect.Top + 1), new PointF(0, rect.Bottom - 1), HoveredTopColour, HoveredBottomColour);
                case ButtonState.Pressed: return new LinearGradientBrush(new Point(0, rect.Top + 1), new PointF(0, rect.Bottom - 1), PressedTopColour, PressedBottomColour);
                default: throw new ArgumentOutOfRangeException(nameof(state));
            }
        }

        private static Pen GetOutline(ButtonState state)
        {
            switch (state)
            {
                case ButtonState.Neutral:
                    return NeutralOutline;
                case ButtonState.Hovered:
                    return HoveredOutline;
                case ButtonState.Pressed:
                    return PressedOutline;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }

        public static void DrawButton(Rectangle area, Graphics graphics, ButtonState state)
        {
            using (var gradient = MakeGradient(area, state))
            {
                graphics.FillRectangle(gradient, InnerRectangle(area));
                Pen outline = GetOutline(state);
                graphics.DrawLine(outline, new Point(area.Left + 1, area.Top), new Point(area.Right - 1 - 1, area.Top));
                graphics.DrawLine(outline, new Point(area.Left + 1, area.Bottom - 1), new Point(area.Right - 1 - 1, area.Bottom - 1));
                graphics.DrawLine(outline, new Point(area.Left, area.Top + 1), new Point(area.Left, area.Bottom - 1 - 1));
                graphics.DrawLine(outline, new Point(area.Right - 1, area.Top + 1), new Point(area.Right - 1, area.Bottom - 1 - 1));
            }
        }
    }
}

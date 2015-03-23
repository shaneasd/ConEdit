using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Utilities
{
    public class ColorScheme
    {
        protected static class Defaults
        {
            public static readonly Color Foreground = Color.FromArgb(205, 205, 205);
            public static readonly Color Background = Color.FromArgb(56, 56, 56);
        }

        public readonly Color SelectionRectangle = Color.FromArgb(128, Color.Blue);
        public readonly Pen ControlBorder = new Pen(Color.Black, 1);
        public readonly Color FormBackground = Color.FromArgb(45, 45, 45);
        public readonly Color Background = Color.FromArgb(56, 56, 56);
        public readonly Color Foreground = Color.FromArgb(205, 205, 205);
        public readonly Pen ForegroundPen = new Pen(Defaults.Foreground);
        public readonly SolidBrush ForegroundBrush = new SolidBrush(Defaults.Foreground);
        public readonly SolidBrush BackgroundBrush = new SolidBrush(Defaults.Background);
        public readonly Color GroupBackgroundSelected = Color.FromArgb(92, Color.White);
        public readonly Color GroupBackgroundUnselected = Color.FromArgb(51, Color.White);
        public readonly Color Grid = Color.FromArgb(42, 42, 42);
        public readonly Color MinorGrid = Color.FromArgb(49, 49, 49);
        public readonly Color SelectedText = Color.Blue;

        public ColorScheme()
        {
        }
    }
}

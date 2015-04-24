using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Utilities.UI
{
    public class ColorScheme : Disposable
    {
        protected static class Defaults
        {
            public static readonly Color Foreground = Color.FromArgb(205, 205, 205);
            public static readonly Color Background = Color.FromArgb(56, 56, 56);
        }

        private readonly Color m_selectionRectangle = Color.FromArgb(128, Color.Blue);
        public Color SelectionRectangle { get { return m_selectionRectangle; } }

        private readonly Pen m_controlBorder = new Pen(Color.Black, 1);
        public Pen ControlBorder { get { return m_controlBorder; } }

        private readonly Color m_formBackground = Color.FromArgb(45, 45, 45);
        public Color FormBackground { get { return m_formBackground; } }

        private readonly Color m_background = Color.FromArgb(56, 56, 56);
        public Color Background { get { return m_background; } }

        private readonly Color m_foreground = Color.FromArgb(205, 205, 205);
        public Color Foreground { get { return m_foreground; } }

        private readonly Pen m_foregroundPen = new Pen(Defaults.Foreground);
        public Pen ForegroundPen { get { return m_foregroundPen; } }

        private readonly SolidBrush m_foregroundBrush = new SolidBrush(Defaults.Foreground);
        public SolidBrush ForegroundBrush { get { return m_foregroundBrush; } }

        private readonly SolidBrush m_backgroundBrush = new SolidBrush(Defaults.Background);
        public SolidBrush BackgroundBrush { get { return m_backgroundBrush; } }

        private readonly Color m_groupBackgroundSelected = Color.FromArgb(92, Color.White);
        public Color GroupBackgroundSelected { get { return m_groupBackgroundSelected; } }

        private readonly Color m_groupBackgroundUnselected = Color.FromArgb(51, Color.White);
        public Color GroupBackgroundUnselected { get { return m_groupBackgroundUnselected; } }

        private readonly Color m_grid = Color.FromArgb(42, 42, 42);
        public Color Grid { get { return m_grid; } }

        private readonly Color m_minorGrid = Color.FromArgb(49, 49, 49);
        public Color MinorGrid { get { return m_minorGrid; } }

        private readonly Color m_selectedText = Color.Blue;
        public Color SelectedText { get { return m_selectedText; } }

        public ColorScheme()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_controlBorder.Dispose();
                m_foregroundPen.Dispose();
                m_foregroundBrush.Dispose();
                m_backgroundBrush.Dispose();
            }
        }
    }
}

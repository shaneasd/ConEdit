using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace Utilities.UI
{
    public class DrawWindow : UserControl
    {
        bool m_colorSchemeAssigned = false;
        ColorScheme m_colorScheme = new ColorScheme();
        public ColorScheme ColorScheme
        {
            set { m_colorScheme = value; BackColor = value.Background; m_colorSchemeAssigned = true; }
            get { return m_colorScheme; }
        }

        public DrawWindow()
            : base()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!m_colorSchemeAssigned && !Util.DesignMode())
                throw new InvalidOperationException("drawnwindow not given a color scheme");
            base.OnPaint(e);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DrawWindow
            // 
            this.Name = "DrawWindow";
            this.Size = new System.Drawing.Size(178, 128);
            this.ResumeLayout(false);
        }

        public bool HandleNavigation { get; set; }

        static Keys[] sm_handledKeys = { Keys.Left, Keys.Right, Keys.Up, Keys.Down, Keys.Home, Keys.End, Keys.Enter, Keys.Back, Keys.Delete, Keys.Escape };

        protected override bool IsInputKey(Keys keyData)
        {
            return sm_handledKeys.Contains(keyData) || sm_handledKeys.Any(k => (k | Keys.Shift) == keyData) || base.IsInputKey(keyData);
        }
    }
}

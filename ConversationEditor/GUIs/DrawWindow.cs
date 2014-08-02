using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace ConversationEditor
{
    public class DrawWindow : UserControl
    {
        public DrawWindow()
            : base()
        {
            BackColor = ColorScheme.Background;
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
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

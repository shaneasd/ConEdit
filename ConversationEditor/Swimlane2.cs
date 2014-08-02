using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ConversationEditor
{
    public partial class Swimlane2 : UserControl
    {
        public Swimlane2()
        {
            InitializeComponent();
            //spiels.Add(new SpielGraphics() { Text = "Spiel 1", Area = new Rectangle(0, 0, 30, 30) });
        }

        IList<SpielGraphics> spiels = new List<SpielGraphics>();
        SpielGraphics selected = null;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
        }

        public void Paint(PaintEventArgs e)
        {
            foreach (SpielGraphics g in spiels)
            {
                g.Draw(e, selected == g);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Paint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            spiels.AsParallel().ForAll(g => { g.Area.Width = this.Width - 4; }); 
        }
    }
}

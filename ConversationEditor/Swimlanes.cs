using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ConversationEditorOld
{
    public partial class Swimlanes : UserControl
    {
        public Swimlanes()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int iSplitter = tableLayoutPanel1.RowCount++;
            int iSwimlane = tableLayoutPanel1.RowCount++;

            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 4));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var swimlane = new Swimlane();
            var splitter = new Splitter();
            swimlane.Dock = DockStyle.Fill;
            this.tableLayoutPanel1.Controls.Add(splitter, 0, iSplitter);
            this.tableLayoutPanel1.Controls.Add(swimlane, 0, iSwimlane);
        }
    }
}

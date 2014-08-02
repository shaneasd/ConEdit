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
    public partial class Swimlane : UserControl
    {
        public Swimlane()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Spiel spiel = new Spiel();
            spiel.Dock = DockStyle.Left;
            this.panel1.Controls.Add(spiel);
        }
    }
}

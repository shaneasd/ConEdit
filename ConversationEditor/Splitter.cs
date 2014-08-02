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
    public partial class Splitter : UserControl
    {
        public Splitter()
        {
            InitializeComponent();

            Dock = DockStyle.Fill;
            BackColor = SystemColors.ActiveBorder;
        }
    }
}

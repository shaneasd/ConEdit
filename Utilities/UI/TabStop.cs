using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Utilities.UI
{
    public partial class TabStop : UserControl
    {
        public TabStop()
        {
            InitializeComponent();

            button1.GotFocus += new EventHandler(button1_GotFocus);
            button2.GotFocus += new EventHandler(button2_GotFocus);
        }

        public event Action ForwardFocus;
        public event Action BackwardFocus;

        public bool Active
        {
            set
            {
                button1.TabStop = value;
                button2.TabStop = value;
            }
            get { return true; }
        }

        void button2_GotFocus(object sender, EventArgs e)
        {
            BackwardFocus.Execute();
        }

        void button1_GotFocus(object sender, EventArgs e)
        {
            ForwardFocus.Execute();
        }
    }
}

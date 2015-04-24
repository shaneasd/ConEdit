using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PluginPack
{
    public partial class FancyEnumForm : Form
    {
        public FancyEnumForm()
        {
            InitializeComponent();
            greyScrollBar1.ColorScheme = new Utilities.UI.ColorScheme();
        }

        private void FancyEnumForm_Leave(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

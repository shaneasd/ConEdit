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
    public partial class ScrollPanel : UserControl
    {
        public ScrollPanel()
        {
            InitializeComponent();
        }

        public ColorScheme ColorScheme { get { return greyScrollBar1.ColorScheme; } set { greyScrollBar1.ColorScheme = value; } }
    }
}

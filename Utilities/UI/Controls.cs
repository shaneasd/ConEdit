using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utilities
{
    public static class Controls
    {
        public static IEnumerable<Control> AllDescendants(this Control c)
        {
            return c.Controls.OfType<Control>().Concat(c.Controls.OfType<Control>().SelectMany(a => a.AllDescendants()));
        }
    }
}

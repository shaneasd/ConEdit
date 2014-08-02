using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RuntimeConversation
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.Run(new Form1());
        }
    }
}

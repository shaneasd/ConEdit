using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ConversationEditor
{
    public partial class TodoEdit : Form
    {
        public TodoEdit()
        {
            InitializeComponent();
        }

        public string TodoText { get { return this.textBox1.Text; } }
    }
}

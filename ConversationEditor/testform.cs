using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Conversation;

namespace ConversationEditor
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
            tabStop1.Active = true;
            //tabStop1.Enter += (a, b) => 
            tabStop1.Leave += (a, b) => tabStop1.Active = true;
            tabStop1.ForwardFocus += () => { tabStop1.Active = false; };
            tabStop1.BackwardFocus += () => { tabStop1.Active = false; };
        }


    }
}

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
    public partial class SpielEdit : Form
    {
        private ISpiel m_spiel;

        public SpielEdit()
        {
            InitializeComponent();
        }

        public SpielEdit(ISpiel spiel)
            : this()
        {
            m_spiel = spiel;
            Summary.Text = spiel.Text;
        }

        private void SpielEdit_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_spiel.Text = Summary.Text;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Conversation;
using Utilities;

namespace ConversationEditor.GUIs
{
    public partial class DataEditor : Form
    {
        private IDataSource m_datasource;

        public DataEditor()
        {
            InitializeComponent();
        }

        public void Initialize(IDataSource datasource)
        {
            m_datasource = datasource;
            foreach (var type in m_datasource.Enumerations)
            {
                comboBox1.Items.Add(new ToStringWrapper<Enumeration>(type, type.TypeName));
            }
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            listBox1.Items.Clear();
            foreach (var value in SelectedEnumeration.Value.Options)
            {
                dataGridView1.Rows.Add(value);
                listBox1.Items.Add(value);
            }
            textBox1_TextChanged(sender, e);
        }

        private ToStringWrapper<Enumeration> SelectedEnumeration
        {
            get { return comboBox1.SelectedItem as ToStringWrapper<Enumeration>; }
        }

        private bool CanAdd(string text)
        {
            return !string.IsNullOrWhiteSpace(text) && !listBox1.Items.OfType<object>().Any(a => a.ToString() == text);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (CanAdd(textBox1.Text))
            {
                SelectedEnumeration.Value.AddOption(textBox1.Text);
                textBox1.Text = "";
                comboBox1_SelectedIndexChanged(sender, e);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button1.Enabled = CanAdd(textBox1.Text);
        }
    }
}

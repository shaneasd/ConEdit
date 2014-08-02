using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Utilities;
using Conversation;

namespace ConversationEditor
{
    public partial class DefaultFilePathEditor : UserControl, IParameterEditor<DefaultFilePathEditor>
    {
        public DefaultFilePathEditor()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Ok.Execute();
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            textBox1.Text = openFileDialog1.FileName;
        }

        public bool WillEdit(IParameter parameter)
        {
            return parameter is IFilePathParameter;
        }

        IFilePathParameter m_parameter;
        public void Setup(IParameter parameter, Localization.ILocalizer localizer)
        {
            m_parameter = parameter as IFilePathParameter;
            textBox1.Text = m_parameter.Value.FullName;
        }

        public DefaultFilePathEditor AsControl
        {
            get { return this; }
        }

        public void UpdateParameter()
        {
            var f = new FileInfo(textBox1.Text); 
            if (!f.Exists)
                throw new Exception();
            m_parameter.Value = f;
        }

        public string DisplayName
        {
            get { return "Default File Path Editor"; }
        }
        
        public bool IsValid()
        {
            var f = new FileInfo(textBox1.Text);
            return f.Exists;
        }

        public event Action Ok;
    }
}

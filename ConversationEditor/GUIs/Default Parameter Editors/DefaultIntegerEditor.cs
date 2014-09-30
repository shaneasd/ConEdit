using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Conversation;
using Utilities;

namespace ConversationEditor
{
    public partial class DefaultIntegerEditor : UserControl, IParameterEditor<DefaultIntegerEditor>
    {
        MyNumericUpDown<int> m_numericUpDown;
        public DefaultIntegerEditor()
        {
            InitializeComponent();

            m_numericUpDown = new MyNumericUpDown<int>(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), false);
            m_numericUpDown.RequestedAreaChanged += () =>
            {
                MinimumSize = new Size(0, (int)m_numericUpDown.RequestedArea.Height);
                Size = m_numericUpDown.RequestedArea.ToSize();
            };
            m_numericUpDown.Colors.BorderPen = ColorScheme.ControlBorder;
            m_numericUpDown.SetupCallbacks(drawWindow1);
        }

        public bool WillEdit(ID<ParameterType> type, WillEdit willEdit)
        {
            return willEdit.IsInteger(type);
        }

        IIntegerParameter m_parameter;
        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as IIntegerParameter;
            m_numericUpDown.Minimum = m_parameter.Min;
            m_numericUpDown.Maximum = m_parameter.Max;
            m_numericUpDown.Value = m_parameter.Value;
        }

        public DefaultIntegerEditor AsControl
        {
            get { return this; }
        }

        public UpdateParameterData UpdateParameterAction()
        {
            return m_parameter.SetValueAction((int)m_numericUpDown.Value);
        }

        private void numericUpDown1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Ok.Execute();
            }
        }

        public string DisplayName
        {
            get { return "Default Integer Editor"; }
        }

        public bool IsValid()
        {
            return true;
        }

        public event Action Ok;
    }
}

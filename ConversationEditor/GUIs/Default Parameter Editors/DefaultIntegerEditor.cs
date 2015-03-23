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
        public class Factory : IParameterEditorFactory
        {
            public static readonly Guid GUID = Guid.Parse("1e5e942c-b9a9-4f5c-a572-0a189c9545fe");
            public bool WillEdit(ParameterType type, WillEdit willEdit)
            {
                return willEdit.IsInteger(type);
            }

            public string Name
            {
                get { return "Default Integer Editor"; }
            }

            public Guid Guid
            {
                get { return GUID; }
            }

            public IParameterEditor<Control> Make(ColorScheme scheme)
            {
                var result =  new DefaultIntegerEditor();
                result.Scheme = scheme;
                return result;
            }
        }

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
            m_numericUpDown.SetupCallbacks(drawWindow1);
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

        public bool IsValid()
        {
            return true;
        }

        public event Action Ok;

        ColorScheme m_scheme;
        public ColorScheme Scheme
        {
            get { return m_scheme; }
            set
            {
                m_scheme = value;
                m_numericUpDown.Colors.BorderPen = Scheme.ControlBorder;
            }
        }
    }
}

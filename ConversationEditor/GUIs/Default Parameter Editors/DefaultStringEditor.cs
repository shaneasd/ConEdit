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
    public partial class DefaultStringEditor : UserControl, IParameterEditor<DefaultStringEditor>
    {
        private MyTextBox m_textBox;

        public DefaultStringEditor()
        {
            InitializeComponent();

            m_textBox = new MyTextBox(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), MyTextBox.InputFormEnum.Text);
            m_textBox.Colors.BorderPen = ColorScheme.ControlBorder;
            m_textBox.RequestedAreaChanged += () =>
            {
                //Draw window is the whole control so we can just modify the control
                MinimumSize = new Size(0, (int)m_textBox.RequestedArea.Height);
                Size = m_textBox.RequestedArea.ToSize();
            };
            m_textBox.EnterPressed += () => Ok.Execute();
            m_textBox.SpecialEnter = true;
            MyTextBox.SetupCallbacks(drawWindow1, m_textBox);
        }

        public bool WillEdit(ID<ParameterType> type, WillEdit willEdit)
        {
            return type == BaseTypeString.PARAMETER_TYPE;
        }

        IStringParameter m_parameter;
        public void Setup(IParameter parameter, LocalizationEngine localizer, IAudioProvider audioProvider)
        {
            m_parameter = parameter as IStringParameter;
            if (!m_parameter.Corrupted)
                m_textBox.Text = m_parameter.Value;
        }

        public DefaultStringEditor AsControl
        {
            get { return this; }
        }

        public SimpleUndoPair? UpdateParameterAction()
        {
            return m_parameter.SetValueAction(m_textBox.Text);
        }

        public string DisplayName
        {
            get { return "Default String Editor"; }
        }

        public bool IsValid()
        {
            return true;
        }

        public event Action Ok;
    }
}

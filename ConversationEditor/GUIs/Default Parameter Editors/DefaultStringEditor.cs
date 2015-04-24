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
using Utilities.UI;

namespace ConversationEditor
{
    public class DefaultStringEditorFactory : IParameterEditorFactory
    {
        public static readonly Guid StaticId = Guid.Parse("b3948691-425b-4220-ba6e-9ef00c5bc0f7");
        public bool WillEdit(ParameterType type, WillEdit willEdit)
        {
            return type == BaseTypeString.ParameterType;
        }

        public string Name
        {
            get { return "Default String Editor"; }
        }

        public Guid Guid
        {
            get { return StaticId; }
        }

        public IParameterEditor<Control> Make(ColorScheme scheme)
        {
            var result = new DefaultStringEditor();
            result.Scheme = scheme;
            return result;
        }
    }

    internal partial class DefaultStringEditor : UserControl, IParameterEditor<DefaultStringEditor>
    {
        private MyTextBox m_textBox;

        public DefaultStringEditor()
        {
            InitializeComponent();

            m_textBox = new MyTextBox(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), MyTextBox.InputFormEnum.Text);
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

        IStringParameter m_parameter;
        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as IStringParameter;
            if (!m_parameter.Corrupted)
                m_textBox.Text = m_parameter.Value;
        }

        public DefaultStringEditor AsControl
        {
            get { return this; }
        }

        public UpdateParameterData UpdateParameterAction()
        {
            return m_parameter.SetValueAction(m_textBox.Text);
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
                m_textBox.Colors.BorderPen = value.ControlBorder;
                drawWindow1.ColorScheme = value;
            }
        }
    }
}

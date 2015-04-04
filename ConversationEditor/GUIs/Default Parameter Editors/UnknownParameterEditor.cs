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
    public partial class UnknownParameterEditor : UserControl, IFocusProvider, IParameterEditor<UnknownParameterEditor>
    {
        private MyTextBox m_textBox;
        private MyButton m_button;
        public MyControl LastFocused { get; set; }

        public UnknownParameterEditor()
        {
            InitializeComponent();

            const int BUTTON_SIZE = 16;

            m_textBox = new MyTextBox(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width - BUTTON_SIZE, Math.Max(BUTTON_SIZE, drawWindow1.Height)), MyTextBox.InputFormEnum.None);
            m_textBox.RequestedAreaChanged += () =>
            {
                //Draw window is the whole control so we can just modify the control
                MinimumSize = new Size(BUTTON_SIZE, (int)m_textBox.RequestedArea.Height);
                Size size = m_textBox.RequestedArea.ToSize();
                size.Width += BUTTON_SIZE;
                Size = size;
            };
            m_textBox.RegisterCallbacks(this, drawWindow1);
            //MyTextBox.SetupCallbacks(drawWindow1, m_textBox);

            m_button = new CrossButton(() => new RectangleF(drawWindow1.Width - BUTTON_SIZE, (drawWindow1.Height - BUTTON_SIZE) / 2, BUTTON_SIZE, BUTTON_SIZE), () => { Remove.Execute(); m_remove = true; }, Scheme.ControlBorder, Scheme.BackgroundBrush);
            m_button.RegisterCallbacks(this, drawWindow1);

            LastFocused = m_textBox;
        }

        UnknownParameter m_parameter;
        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as UnknownParameter;
            if (!m_parameter.Corrupted)
                m_textBox.Text = m_parameter.Value;
        }

        public UnknownParameterEditor AsControl
        {
            get { return this; }
        }

        bool m_remove = false;
        private SimpleUndoPair m_removeParameter;
        public UpdateParameterData UpdateParameterAction()
        {
            if (m_remove)
                return m_removeParameter;
            else
                return null;
        }

        public bool IsValid()
        {
            return true;
        }

        public event Action Ok { add { } remove { } }
        public event Action Remove;

        internal static UnknownParameterEditor Make(ColorScheme scheme, ParameterEditorSetupData data, SimpleUndoPair removeParameter, Action remove)
        {
            var result = new UnknownParameterEditor();
            result.Scheme = scheme;
            result.Setup(data);
            result.m_removeParameter = removeParameter;
            result.Remove += remove;
            return result;
        }

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

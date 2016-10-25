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
        public bool WillEdit(ParameterType type, WillEdit queries)
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

        public IParameterEditor<Control> Make(IColorScheme scheme)
        {
            return new DefaultStringEditor(scheme);
        }
    }


    internal partial class DefaultStringEditor : UserControl, IParameterEditor<DefaultStringEditor>
    {

        private MyTextBox m_textBox;

        internal IEnumerable<string> AutoCompleteSuggestions(string arg)
        {
            if (m_autoCompleteSuggestions != null)
                return m_autoCompleteSuggestions(arg);
            else
                return Enumerable.Empty<string>();
        }

        public DefaultStringEditor(IColorScheme scheme) : this()
        {
            Scheme = scheme;
        }

        public DefaultStringEditor()
        {
            InitializeComponent();

            m_textBox = new MyTextBox(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), MyTextBox.InputFormEnum.Text, AutoCompleteSuggestions);
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
            m_autoCompleteSuggestions = data.AutoCompleteSuggestions;
        }

        public DefaultStringEditor AsControl
        {
            get { return this; }
        }

        public UpdateParameterData UpdateParameterAction()
        {
            return m_parameter.SetValueAction(m_textBox.Text);
        }

        public string IsValid()
        {
            return null;
        }

        public event Action Ok;

        IColorScheme m_scheme;
        private Func<string, IEnumerable<string>> m_autoCompleteSuggestions;

        public IColorScheme Scheme
        {
            get { return m_scheme; }
            set
            {
                m_scheme = value;
                m_textBox.Colors.BorderPen = value.ControlBorder;
                drawWindow1.ColorScheme = value;
            }
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_textBox.Dispose();
            }
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

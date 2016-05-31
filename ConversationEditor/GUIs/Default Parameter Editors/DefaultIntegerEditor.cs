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
    public class DefaultIntegerEditorFactory : IParameterEditorFactory
    {
        public static readonly Guid StaticId = Guid.Parse("1e5e942c-b9a9-4f5c-a572-0a189c9545fe");
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
            get { return StaticId; }
        }

        public IParameterEditor<Control> Make(ColorScheme scheme)
        {
            var result = new DefaultIntegerEditor();
            result.Scheme = scheme;
            return result;
        }
    }

    internal partial class DefaultIntegerEditor : UserControl, IParameterEditor<DefaultIntegerEditor>
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

        public string IsValid()
        {
            if (m_numericUpDown.Value > m_numericUpDown.Maximum)
                return "Entered value is greater than maximum allowed value";
            else if (m_numericUpDown.Value < m_numericUpDown.Minimum)
                return "Entered value is less than minimum allowed value";
            else
                return null;
        }

        public event Action Ok { add { } remove { } }

        ColorScheme m_scheme;
        public ColorScheme Scheme
        {
            get { return m_scheme; }
            set
            {
                m_scheme = value;
                m_numericUpDown.Colors.BorderPen = Scheme.ControlBorder;
                drawWindow1.ColorScheme = value;
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_numericUpDown.Dispose();
            }
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}

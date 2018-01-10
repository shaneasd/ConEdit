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
        public static Guid StaticId { get; } = Guid.Parse("1e5e942c-b9a9-4f5c-a572-0a189c9545fe");
        public bool WillEdit(ParameterType type, WillEdit queries)
        {
            return queries.IsInteger(type);
        }

        public string Name => "Default Integer Editor";

        public Guid Guid => StaticId;

        public IParameterEditor Make(IColorScheme scheme)
        {
            return new DefaultIntegerEditor(scheme);
        }
    }

    internal partial class DefaultIntegerEditor : UserControl, IParameterEditor
    {
        MyNumericUpDown<int> m_numericUpDown;

        public DefaultIntegerEditor(IColorScheme scheme) : this()
        {
            Scheme = scheme;
        }

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
            m_numericUpDown.Minimum = () => m_parameter.Min;
            m_numericUpDown.Maximum = () => m_parameter.Max;
            m_numericUpDown.Value = m_parameter.Value;
        }

        public Control AsControl => this;

        public UpdateParameterData UpdateParameterAction()
        {
            return m_parameter.SetValueAction((int)m_numericUpDown.Value);
        }

        public string IsValid()
        {
            if (m_numericUpDown.Value > m_parameter.Max)
                return "Entered value is greater than maximum allowed value";
            else if (m_numericUpDown.Value < m_parameter.Min)
                return "Entered value is less than minimum allowed value";
            else
                return null;
        }

        public event Action Ok { add { } remove { } }

        IColorScheme m_scheme;
        public IColorScheme Scheme
        {
            get { return m_scheme; }
            set
            {
                m_scheme = value;
                m_numericUpDown.Colors.BorderPen = value.ControlBorder;
                //drawWindow1.ColorScheme = value;
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

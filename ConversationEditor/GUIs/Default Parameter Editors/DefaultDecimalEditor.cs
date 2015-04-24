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
    public class DefaultDecimalEditorFactory : IParameterEditorFactory
    {
        public static readonly Guid StaticId = Guid.Parse("846c1908-4e19-4b11-bbb3-ea40b58ef72a");
        public bool WillEdit(ParameterType type, WillEdit willEdit)
        {
            return willEdit.IsDecimal(type);
        }

        public string Name
        {
            get { return "Default Decimal Editor"; }
        }

        public Guid Guid
        {
            get { return StaticId; }
        }

        public IParameterEditor<Control> Make(ColorScheme scheme)
        {
            var result = new DefaultDecimalEditor();
            result.Scheme = scheme;
            return result;
        }
    }

    internal partial class DefaultDecimalEditor : UserControl, IParameterEditor<DefaultDecimalEditor>
    {
        MyNumericUpDown<decimal> m_numericUpDown;

        public DefaultDecimalEditor()
        {
            InitializeComponent();

            m_numericUpDown = new MyNumericUpDown<decimal>(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), true);
            m_numericUpDown.RequestedAreaChanged += () =>
            {
                MinimumSize = new Size(0, (int)m_numericUpDown.RequestedArea.Height);
                Size = m_numericUpDown.RequestedArea.ToSize();
            };
            m_numericUpDown.SetupCallbacks(drawWindow1);
        }

        IDecimalParameter m_parameter;
        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as IDecimalParameter;
            m_numericUpDown.Minimum = m_parameter.Min;
            m_numericUpDown.Maximum = m_parameter.Max;
            m_numericUpDown.Value = m_parameter.Value;
        }

        public DefaultDecimalEditor AsControl
        {
            get { return this; }
        }

        public UpdateParameterData UpdateParameterAction()
        {
            return m_parameter.SetValueAction(m_numericUpDown.Value);
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
                m_numericUpDown.Colors.BorderPen = value.ControlBorder;
                drawWindow1.ColorScheme = value;
            }
        }
    }
}

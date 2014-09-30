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
    public partial class DefaultDynamicEnumEditor : UserControl, IParameterEditor<DefaultDynamicEnumEditor>
    {
        private MyComboBox<string> m_comboBox;
        private IEnumerable<MyComboBox<string>.Item> m_comboBoxItems;

        public DefaultDynamicEnumEditor()
        {
            InitializeComponent();

            m_comboBoxItems = (new ExtraLazyEnumerable<MyComboBox<string>.Item>(() => m_parameter.Options.Select(ch => new MyComboBox<string>.Item(ch, ch))));
            m_comboBox = new MyComboBox<string>(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), true, m_comboBoxItems);
            m_comboBox.SetupCallbacks();
            m_comboBox.RequestedAreaChanged += () =>
            {
                //Draw window is the whole control so we can just modify the control
                MinimumSize = new Size(0, (int)m_comboBox.RequestedArea.Height);
                Size = m_comboBox.RequestedArea.ToSize();
                Invalidate(true);
            };
            m_comboBox.Colors.BorderPen = ColorScheme.ControlBorder;
            m_comboBox.Renderer = ColorScheme.ContextMenu;
            m_comboBox.SpecialEnter = true;
            m_comboBox.EnterPressed += () => { Ok.Execute(); };
        }

        public bool WillEdit(ID<ParameterType> type, WillEdit willEdit)
        {
            return willEdit.IsDynamicEnum(type);
        }

        IDynamicEnumParameter m_parameter;
        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as IDynamicEnumParameter;
            if (!data.Parameter.Corrupted)
                m_comboBox.SelectedItem = new MyComboBox<string>.Item(m_parameter.Value, m_parameter.Value);
        }

        public DefaultDynamicEnumEditor AsControl
        {
            get { return this; }
        }

        public UpdateParameterData UpdateParameterAction()
        {
            return m_parameter.SetValueAction(m_comboBox.SelectedItem.DisplayString);
        }

        public string DisplayName
        {
            get { return "Default Dynamic Enumeration Editor"; }
        }

        public bool IsValid()
        {
            return true;
        }

        public event Action Ok;
    }
}

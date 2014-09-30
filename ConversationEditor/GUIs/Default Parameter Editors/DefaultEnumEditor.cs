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
    public partial class DefaultEnumEditor : UserControl, IParameterEditor<DefaultEnumEditor>
    {
        MyComboBox<Guid> m_comboBox;
        List<MyComboBox<Guid>.Item> m_comboBoxItems = new List<MyComboBox<Guid>.Item>();
        IEnumParameter m_parameter;

        public DefaultEnumEditor()
        {
            InitializeComponent();

            m_comboBox = new MyComboBox<Guid>(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), false, m_comboBoxItems);
            m_comboBox.SetupCallbacks();
            m_comboBox.RequestedAreaChanged += () =>
                {
                    MinimumSize = new Size(0, (int)m_comboBox.RequestedArea.Height);
                    Size = m_comboBox.RequestedArea.ToSize();
                };
            m_comboBox.Colors.BorderPen = ColorScheme.ControlBorder;
            m_comboBox.Renderer = ColorScheme.ContextMenu;
            m_comboBox.SelectionChanged += () => m_parameter.EditorSelected = m_comboBox.SelectedItem.Contents;
            m_comboBox.SpecialEnter = true;
            m_comboBox.EnterPressed += () => { Ok.Execute(); };
        }

        public bool WillEdit(ID<ParameterType> type, WillEdit willEdit)
        {
            return willEdit.IsEnum(type);
        }

        private Guid SelectedItem
        {
            get
            {
                return m_comboBox.SelectedItem.Contents;
            }
            set
            {
                m_comboBox.SelectedItem = new MyComboBox<Guid>.Item(m_parameter.GetName(value), value);
            }
        }

        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as IEnumParameter;
            foreach (var ch in m_parameter.Options)
            {
                m_comboBoxItems.Add(new MyComboBox<Guid>.Item(m_parameter.GetName(ch), ch));
            }

            if (!m_parameter.Corrupted)
            {
                var valueName = m_parameter.GetName(m_parameter.Value);
                if (valueName != null)
                    m_comboBox.SelectedItem = new MyComboBox<Guid>.Item(valueName, m_parameter.Value);
                else
                    m_comboBox.SelectedItem = new MyComboBox<Guid>.Item(EnumParameter.INVALID_VALUE);
            }
        }

        public DefaultEnumEditor AsControl
        {
            get { return this; }
        }

        public UpdateParameterData UpdateParameterAction()
        {
            if (!IsValid())
                throw new Exception("Current enum selection is invalid");

            return m_parameter.SetValueAction(SelectedItem);
        }

        public string DisplayName
        {
            get { return "Default Enumeration Editor"; }
        }

        public bool IsValid()
        {
            return m_comboBox.Items.Any(i => i.Contents == m_comboBox.SelectedItem.Contents);
        }

        public event Action Ok;
        //{
        //    add { }
        //    remove { }
        //}
    }
}

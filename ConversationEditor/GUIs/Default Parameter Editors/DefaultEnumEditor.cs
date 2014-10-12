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
        public class Factory : IParameterEditorFactory
        {
            public static readonly Guid GUID = Guid.Parse("1e8f8730-a710-4341-be04-2c80272e896c");
            public bool WillEdit(ID<ParameterType> type, WillEdit willEdit)
            {
                return willEdit.IsEnum(type);
            }

            public string Name
            {
                get { return "Default Enumeration Editor"; }
            }

            public Guid Guid
            {
                get { return GUID; }
            }

            public IParameterEditor<Control> Make()
            {
                return new DefaultEnumEditor();
            }
        }

        MySuggestionBox<Guid> m_comboBox;
        List<MySuggestionBox<Guid>.Item> m_comboBoxItems = new List<MySuggestionBox<Guid>.Item>();
        IEnumParameter m_parameter;

        public DefaultEnumEditor()
        {
            InitializeComponent();

            m_comboBox = new MySuggestionBox<Guid>(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), false, m_comboBoxItems);
            m_comboBox.SetupCallbacks();
            m_comboBox.RequestedAreaChanged += () =>
                {
                    MinimumSize = new Size(0, (int)m_comboBox.RequestedArea.Height);
                    Size = m_comboBox.RequestedArea.ToSize();
                };
            m_comboBox.Colors.TextBox.BorderPen = ColorScheme.ControlBorder;
            m_comboBox.Colors.SelectedBackground = ColorScheme.SelectedConversationListItemPrimaryBackground;
            m_comboBox.Renderer = ColorScheme.ContextMenu;
            m_comboBox.SelectionChanged += () => m_parameter.EditorSelected = m_comboBox.SelectedItem.Contents;
            m_comboBox.EnterPressed += () => { Ok.Execute(); };
        }

        private Guid SelectedItem
        {
            get
            {
                return m_comboBox.SelectedItem.Contents;
            }
            set
            {
                m_comboBox.SelectedItem = new MySuggestionBox<Guid>.Item(m_parameter.GetName(value), value);
            }
        }

        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as IEnumParameter;
            foreach (var ch in m_parameter.Options.OrderBy(o=>m_parameter.GetName(o)))
            {
                m_comboBoxItems.Add(new MySuggestionBox<Guid>.Item(m_parameter.GetName(ch), ch));
            }

            if (!m_parameter.Corrupted)
            {
                var valueName = m_parameter.GetName(m_parameter.Value);
                if (valueName != null)
                    m_comboBox.SelectedItem = new MySuggestionBox<Guid>.Item(valueName, m_parameter.Value);
                else
                    m_comboBox.SelectedItem = new MySuggestionBox<Guid>.Item(EnumParameter.INVALID_VALUE);
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

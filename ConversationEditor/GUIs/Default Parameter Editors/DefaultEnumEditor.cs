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

using TControl = Utilities.UI.MySuggestionBox<System.Guid>;
//using TControl = Utilities.MyComboBox<System.Guid>;

namespace ConversationEditor
{
    public class DefaultEnumEditorFactory : IParameterEditorFactory
    {
        public static readonly Guid StaticId = Guid.Parse("1e8f8730-a710-4341-be04-2c80272e896c");
        public bool WillEdit(ParameterType type, WillEdit willEdit)
        {
            return willEdit.IsEnum(type);
        }

        public string Name
        {
            get { return "Default Enumeration Editor"; }
        }

        public Guid Guid
        {
            get { return StaticId; }
        }

        public IParameterEditor<Control> Make(ColorScheme scheme)
        {
            var result = new DefaultEnumEditor();
            result.Scheme = scheme;
            return result;
        }
    }

    internal partial class DefaultEnumEditor : UserControl, IParameterEditor<DefaultEnumEditor>
    {
        TControl m_comboBox;
        List<TControl.Item> m_comboBoxItems = new List<TControl.Item>();
        IEnumParameter m_parameter;

        public DefaultEnumEditor()
        {
            InitializeComponent();

            m_comboBox = new TControl(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), true, m_comboBoxItems);
            m_comboBox.SetupCallbacks();
            m_comboBox.RequestedAreaChanged += () =>
                {
                    MinimumSize = new Size(0, (int)m_comboBox.RequestedArea.Height);
                    Size = m_comboBox.RequestedArea.ToSize();
                };
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
                m_comboBox.SelectedItem = new TControl.Item(m_parameter.GetName(value), value);
            }
        }

        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as IEnumParameter;
            foreach (var ch in m_parameter.Options.OrderBy(o=>m_parameter.GetName(o)))
            {
                m_comboBoxItems.Add(new TControl.Item(m_parameter.GetName(ch), ch));
            }

            if (!m_parameter.Corrupted)
            {
                var valueName = m_parameter.GetName(m_parameter.Value);
                if (valueName != null)
                    m_comboBox.SelectedItem = new TControl.Item(valueName, m_parameter.Value);
                else
                    m_comboBox.SelectedItem = new TControl.Item(EnumParameter.InvalidValue);
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

        ColorScheme m_scheme;
        public ColorScheme Scheme
        {
            get { return m_scheme; }
            set
            {
                m_scheme = value;
                m_comboBox.TextBoxColors.BorderPen = value.ControlBorder;
                m_comboBox.SelectedBackgroundColor = value.SelectedConversationListItemPrimaryBackground;
                m_comboBox.Renderer = value.ContextMenu;
                drawWindow1.ColorScheme = value;
            }
        }
    }
}

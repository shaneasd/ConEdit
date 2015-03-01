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

using TControl = Utilities.MySuggestionBox<string>;
//using TControl = Utilities.MyComboBox<string>;

namespace ConversationEditor
{
    public partial class DefaultDynamicEnumEditor : UserControl, IParameterEditor<DefaultDynamicEnumEditor>
    {
        public class Factory : IParameterEditorFactory
        {
            public static readonly Guid GUID = Guid.Parse("a9083141-9c56-44f1-8d5d-c10479877663");
            public bool WillEdit(ID<ParameterType> type, WillEdit willEdit)
            {
                return willEdit.IsDynamicEnum(type);
            }

            public string Name
            {
                get { return "Default Dynamic Enumeration Editor"; }
            }

            public Guid Guid
            {
                get { return GUID; }
            }

            public IParameterEditor<Control> Make()
            {
                return new DefaultDynamicEnumEditor();
            }
        }

        private TControl m_comboBox;
        private IEnumerable<TControl.Item> m_comboBoxItems;

        public DefaultDynamicEnumEditor()
        {
            InitializeComponent();

            m_comboBoxItems = (new ExtraLazyEnumerable<TControl.Item>(() => m_parameter.Options.Select(ch => new TControl.Item(ch, ch))));
            m_comboBox = new TControl(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), true, m_comboBoxItems);
            m_comboBox.SetupCallbacks();
            m_comboBox.RequestedAreaChanged += () =>
            {
                //Draw window is the whole control so we can just modify the control
                MinimumSize = new Size(0, (int)m_comboBox.RequestedArea.Height);
                Size = m_comboBox.RequestedArea.ToSize();
                Invalidate(true);
            };
            m_comboBox.TextBoxColors.BorderPen = ColorScheme.ControlBorder;
            m_comboBox.SelectedBackgroundColor = ColorScheme.SelectedConversationListItemPrimaryBackground;
            m_comboBox.Renderer = ColorScheme.ContextMenu;
            m_comboBox.EnterPressed += () => { Ok.Execute(); };
        }

        IDynamicEnumParameter m_parameter;
        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as IDynamicEnumParameter;
            if (!data.Parameter.Corrupted)
                m_comboBox.SelectedItem = new TControl.Item(m_parameter.Value, m_parameter.Value);
        }

        public DefaultDynamicEnumEditor AsControl
        {
            get { return this; }
        }

        public UpdateParameterData UpdateParameterAction()
        {
            return m_parameter.SetValueAction(m_comboBox.SelectedItem.DisplayString);
        }

        public bool IsValid()
        {
            return true;
        }

        public event Action Ok;
    }
}

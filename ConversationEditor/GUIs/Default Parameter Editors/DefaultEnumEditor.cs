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
using TItem = Utilities.UI.MyComboBoxItem<System.Guid>;
//using TControl = Utilities.MyComboBox<System.Guid>;

namespace ConversationEditor
{
    public class DefaultEnumEditorFactory : IParameterEditorFactory
    {
        public static readonly Guid StaticId = Guid.Parse("1e8f8730-a710-4341-be04-2c80272e896c");
        public bool WillEdit(ParameterType type, WillEdit queries)
        {
            return queries.IsEnum(type);
        }

        public string Name
        {
            get { return "Default Enumeration Editor"; }
        }

        public Guid Guid
        {
            get { return StaticId; }
        }

        public IParameterEditor Make(IColorScheme scheme)
        {
            return new DefaultEnumEditor(scheme);
        }
    }

    internal partial class DefaultEnumEditor : UserControl, IParameterEditor
    {
        TControl m_comboBox;
        List<TItem> m_comboBoxItems = new List<TItem>();
        IEnumParameter m_parameter;

        public DefaultEnumEditor(IColorScheme scheme) : this()
        {
            Scheme = scheme;
        }

        public DefaultEnumEditor()
        {
            InitializeComponent();
            drawWindow1.SizeChanged += (a, args) => m_comboBox.AreaChanged();

            m_comboBox = new TControl(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), true, m_comboBoxItems);
            m_comboBox.SetupCallbacks();
            m_comboBox.RequestedAreaChanged += () =>
            {
                MinimumSize = new Size(0, (int)m_comboBox.RequestedArea.Height);
                Size = m_comboBox.RequestedArea.ToSize();
                drawWindow1.Size = Size; //This should not be necessary, due to docking, but for some reason is.
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
                m_comboBox.SelectedItem = new TItem(m_parameter.GetName(value), value);
            }
        }

        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as IEnumParameter;
            foreach (var ch in m_parameter.Options.OrderBy(o=>m_parameter.GetName(o)))
            {
                m_comboBoxItems.Add(new TItem(m_parameter.GetName(ch), ch));
            }

            if (!m_parameter.Corrupted)
            {
                var valueName = m_parameter.GetName(m_parameter.Value);
                if (valueName != null)
                    m_comboBox.SelectedItem = new TItem(valueName, m_parameter.Value);
                else
                    m_comboBox.SelectedItem = new TItem(InvalidValue);
            }
        }

        public const string InvalidValue = "ERROR: Unknown enumeration value";

        public Control AsControl
        {
            get { return this; }
        }

        public UpdateParameterData UpdateParameterAction()
        {
            if (IsValid() != null)
                throw new InvalidOperationException("Current enum selection is invalid");

            return m_parameter.SetValueAction(SelectedItem);
        }

        public string IsValid()
        {
            return m_comboBox.Items.Any(i => i.Contents == m_comboBox.SelectedItem.Contents) ? null : "Selected item does not exist in enumeration";
        }

        public event Action Ok;

        IColorScheme m_scheme;
        public IColorScheme Scheme
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

        //TODO: Awful hack
        internal void ParentFormMouseActivatedHack()
        {
            m_comboBox.ParentFormMouseActivatedHack();
        }
    }
}

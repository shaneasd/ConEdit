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

using TControl = Utilities.UI.MySuggestionBox<string>;
using TItem = Utilities.UI.MyComboBoxItem<string>;

namespace ConversationEditor
{
    public class DefaultDynamicEnumEditorFactory : IParameterEditorFactory
    {
        public static readonly Guid StaticId = Guid.Parse("a9083141-9c56-44f1-8d5d-c10479877663");
        public bool WillEdit(ParameterType type, WillEdit queries)
        {
            return queries.IsDynamicEnum(type);
        }

        public string Name => "Default Dynamic Enumeration Editor";

        public Guid Guid => StaticId;

        public IParameterEditor Make(IColorScheme scheme)
        {
            return new DefaultDynamicEnumEditor(scheme);
        }
    }

    internal partial class DefaultDynamicEnumEditor : UserControl, IParameterEditor
    {
        private TControl m_comboBox;
        private IEnumerable<TItem> m_comboBoxItems;

        public DefaultDynamicEnumEditor(IColorScheme scheme) : this()
        {
            Scheme = scheme;
        }
        
        public DefaultDynamicEnumEditor()
        {
            InitializeComponent();

            m_comboBoxItems = (new ExtraLazyEnumerable<TItem>(() => m_parameter.Options.Select(ch => new TItem(ch, ch))));
            m_comboBox = new TControl(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), true, m_comboBoxItems, Fonts.Default);
            m_comboBox.SetupCallbacks();
            m_comboBox.RequestedAreaChanged += () =>
            {
                //Draw window is the whole control so we can just modify the control
                MinimumSize = new Size(0, (int)m_comboBox.RequestedArea.Height);
                Size = m_comboBox.RequestedArea.ToSize();
                Invalidate(true);
            };
            m_comboBox.EnterPressed += () => { Ok.Execute(); };
        }

        IDynamicEnumParameter m_parameter;
        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as IDynamicEnumParameter;
            if (!data.Parameter.Corrupted)
                m_comboBox.SelectedItem = new TItem(m_parameter.Value, m_parameter.Value);
        }

        public Control AsControl => this;

        public UpdateParameterData UpdateParameterAction()
        {
            return m_parameter.SetValueAction(m_comboBox.SelectedItem.DisplayString);
        }

        public string IsValid()
        {
            return null;
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
                m_comboBox.Dispose();
            }
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

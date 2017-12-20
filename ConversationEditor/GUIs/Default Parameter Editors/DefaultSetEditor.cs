using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Conversation;
using Utilities;

using TControl = Utilities.UI.MySuggestionBox<System.Guid>;
using TItem = Utilities.UI.MyComboBoxItem<System.Guid>;
using Utilities.UI;

namespace ConversationEditor
{
    public class DefaultSetEditorFactory : IParameterEditorFactory
    {
        public static readonly Guid StaticId = Guid.Parse("7f171542-ddff-42dd-a253-161947eebebb");
        public bool WillEdit(ParameterType type, WillEdit queries)
        {
            return type.IsSet;
        }

        public string Name
        {
            get { return "Default Set Editor"; }
        }

        public Guid Guid
        {
            get { return StaticId; }
        }

        public IParameterEditor Make(IColorScheme scheme)
        {
            return new DefaultSetEditor(scheme);
        }
    }

    internal partial class DefaultSetEditor : UserControl, IParameterEditor
    {
        List<TControl> m_comboBoxes = new List<TControl>();
        List<CrossButton> m_buttons = new List<CrossButton>();
        List<TItem> m_comboBoxItems = new List<TItem>();
        ISetParameter m_parameter;

        public DefaultSetEditor(IColorScheme scheme) : this()
        {
            Scheme = scheme;
        }

        public DefaultSetEditor()
        {
            InitializeComponent();

            AddComboBox();
        }

        private IFocusProvider m_focusProvider = new FocusProvider(null);

        private int IndexOfCombo(TControl control)
        {
            return m_comboBoxes.IndexOf(t => object.ReferenceEquals(t, control));
        }

        const int COMBO_WIDTH = 80;
        private void AddComboBox()
        {
            TControl comboBox = null;
            comboBox = new TControl(drawWindow1, () => new RectangleF(IndexOfCombo(comboBox) * (COMBO_WIDTH + drawWindow1.Height), 0, COMBO_WIDTH, drawWindow1.Height), true, m_comboBoxItems);
            comboBox.RequestedAreaChanged += () =>
            {
                MinimumSize = new Size(0, (int)comboBox.RequestedArea.Height);
                Size = m_comboBoxes.Select(b => b.RequestedArea.ToSize()).Aggregate(Size.Empty, (a, b) => new Size(a.Width + COMBO_WIDTH + drawWindow1.Height, Math.Max(a.Height, b.Height)));
                drawWindow1.Size = Size; //This should not be necessary, due to docking, but for some reason is.
            };

            comboBox.RegisterCallbacks(m_focusProvider, drawWindow1);
            m_focusProvider.LastFocused = comboBox;

            comboBox.SelectionChanged += () => SelectionChanged(comboBox);

            if (Scheme != null)
                SetupColors(Scheme, comboBox);

            //comboBox.EnterPressed += () => { Ok.Execute(); };
            m_comboBoxes.Add(comboBox);
            Size = m_comboBoxes.Select(b => b.RequestedArea.ToSize()).Aggregate(Size.Empty, (a, b) => new Size(a.Width + COMBO_WIDTH + drawWindow1.Height, Math.Max(a.Height, b.Height)));
            drawWindow1.Size = Size; //This should not be necessary, due to docking, but for some reason is.

            if (m_comboBoxes.Count > 1)
            {
                int i = m_comboBoxes.Count - 2;
                CrossButton button = null;
                try
                {
                    button = new CrossButton(() => new RectangleF(i * (COMBO_WIDTH + drawWindow1.Height) + COMBO_WIDTH, 0, drawWindow1.Height, drawWindow1.Height), () => { Remove(i); }, Scheme.ControlBorder, Scheme.BackgroundBrush);
                    button.RegisterCallbacks(m_focusProvider, drawWindow1);
                    m_buttons.Add(button);
                    button = null;
                }
                finally
                {
                    if (button != null)
                        button.Dispose();
                }
            }
        }

        private void Remove(int i)
        {
            m_comboBoxes[i].Dispose();
            m_comboBoxes.RemoveAt(i);
            m_buttons[m_buttons.Count - 1].Dispose();
            m_buttons.RemoveAt(m_buttons.Count - 1);
            drawWindow1.Invalidate();
        }

        private void SelectionChanged(TControl comboBox)
        {
            if (object.ReferenceEquals(comboBox, m_comboBoxes.Last()))
                if (comboBox.SelectedItem.Contents != Guid.Empty)
                    AddComboBox();
        }

        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as ISetParameter;
            foreach (var ch in m_parameter.Options.OrderBy(o => m_parameter.GetName(o)))
            {
                m_comboBoxItems.Add(new TItem(m_parameter.GetName(ch), ch));
            }

            if (!m_parameter.Corrupted)
            {
                foreach (Guid selection in m_parameter.Value)
                {
                    AddComboBox();
                    var valueName = m_parameter.GetName(selection);
                    var comboBox = m_comboBoxes[m_comboBoxes.Count - 2];
                    if (valueName != null)
                        comboBox.SelectedItem = new TItem(valueName, selection);
                    else
                        comboBox.SelectedItem = new TItem(DefaultEnumEditor.InvalidValue);
                }
            }

            int i = 0;
            foreach (var c in m_comboBoxes)
            {
                c.Name = "Combobox " + i++;
            }
        }

        public Control AsControl
        {
            get { return this; }
        }

        public UpdateParameterData UpdateParameterAction()
        {
            if (IsValid() != null)
                throw new InvalidOperationException("Current enum selection is invalid");

            ReadOnlySet<Guid> selection = new ReadOnlySet<Guid>(m_comboBoxes.TakeWhile((c, i) => i < m_comboBoxes.Count - 1).Select(c => c.SelectedItem.Contents));

            return m_parameter.SetValueAction(selection);
        }

        public string IsValid()
        {
            bool valid = true;
            StringBuilder errorBuilder = new StringBuilder();
            foreach (var combobox in m_comboBoxes.Take(m_comboBoxes.Count - 1))//Ignore the last one as we know it's empty.
            {
                if (!combobox.Items.Any(i => i.Contents == combobox.SelectedItem.Contents))
                {
                    valid = false;
                    errorBuilder.AppendLine("Selected item does not exist in enumeration");
                }
            }
            if (!valid)
                return errorBuilder.ToString();
            else
                return null;
        }

        public event Action Ok
        {
            add { }
            remove { }
        }

        IColorScheme m_scheme;
        public IColorScheme Scheme
        {
            get { return m_scheme; }
            set
            {
                m_scheme = value;
                foreach (var b in m_comboBoxes)
                {
                    SetupColors(value, b);
                }
                foreach (var b in m_buttons)
                {
                    b.Foreground = Scheme.ControlBorder;
                    b.Background = Scheme.BackgroundBrush;
                }
                drawWindow1.ColorScheme = value;
            }
        }

        private static void SetupColors(IColorScheme value, TControl b)
        {
            b.TextBoxColors.BorderPen = value.ControlBorder;
            b.SelectedBackgroundColor = value.SelectedConversationListItemPrimaryBackground;
            b.Renderer = value.ContextMenu;
        }
    }
}

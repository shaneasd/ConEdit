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
using System.Text.RegularExpressions;
using Utilities.UI;

namespace ConversationEditor
{
    public class DefaultLocalizedStringEditorFactory : IParameterEditorFactory
    {
        public static readonly Guid StaticId = Guid.Parse("df3f30b8-ee05-4972-8b41-fb075d5502a7");
        public bool WillEdit(ParameterType type, WillEdit queries)
        {
            return type == BaseTypeLocalizedString.ParameterType;
        }

        public string Name
        {
            get { return "Default Localized String Editor"; }
        }

        public Guid Guid
        {
            get { return StaticId; }
        }

        public IParameterEditor Make(IColorScheme scheme)
        {
            return new DefaultLocalizedStringEditor(scheme);
        }
    }

    internal partial class DefaultLocalizedStringEditor : UserControl, IParameterEditor
    {
        private MyTextBox m_textBox;

        internal IEnumerable<string> AutoCompleteSuggestions(string arg)
        {
            if (m_autoCompleteSuggestions != null)
                return m_autoCompleteSuggestions(arg);
            else
                return Enumerable.Empty<string>();
        }

        public DefaultLocalizedStringEditor(IColorScheme scheme) : this()
        {
            Scheme = scheme;
        }

        public DefaultLocalizedStringEditor()
        {
            InitializeComponent();

            //TODO: Suggest things that make sense from the domain for autoCompleteSuggestions
            m_textBox = new MyTextBox(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), MyTextBox.InputFormEnum.Text, AutoCompleteSuggestions);
            m_textBox.RequestedAreaChanged += () =>
            {
                //Draw window is the whole control so we can just modify the control
                MinimumSize = new Size(0, (int)m_textBox.RequestedArea.Height);
                Size = m_textBox.RequestedArea.ToSize();
            };
            m_textBox.EnterPressed += () => Ok.Execute();
            m_textBox.SpecialEnter = true;
            MyTextBox.SetupCallbacks(drawWindow1, m_textBox);
        }

        ILocalizationEngine m_localizer;

        ILocalizedStringParameter m_parameter;
        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as ILocalizedStringParameter;
            m_localizer = data.Localizer;
            if (!m_parameter.Corrupted)
                m_textBox.Text = m_localizer.Localize(m_parameter.Value);
            else
                m_textBox.Text = m_localizer.Localize(null);
            if (!m_localizer.CanLocalize)
                m_textBox.InputForm = MyTextBox.InputFormEnum.None;
            m_autoCompleteSuggestions = data.AutoCompleteSuggestions;
        }

        public Control AsControl
        {
            get { return this; }
        }

        public UpdateParameterData UpdateParameterAction()
        {
            var localized = m_parameter.Corrupted ? null : m_localizer.Localize(m_parameter.Value);
            if (m_textBox.Text != localized)
            {
                if (m_parameter.Corrupted)
                {
                    Id<LocalizedText> id = new Id<LocalizedText>();
                    var parameterAction = m_parameter.SetValueAction(id);
                    var localizerAction = m_localizer.SetLocalizationAction(id, m_textBox.Text);
                    return new SimpleUndoPair
                    {
                        //m_parameter.Corrupted implies m_parameter.SetValueAction(_)!=null
                        Undo = () => { parameterAction.Value.Undo(); localizerAction.Undo(); },
                        Redo = () => { parameterAction.Value.Redo(); localizerAction.Redo(); }
                    };
                }
                else
                {
                    return m_localizer.SetLocalizationAction(m_parameter.Value, m_textBox.Text);
                }
            }
            else
            {
                return new UpdateParameterData();
            }
        }

        public string IsValid()
        {
            return null;
        }

        public event Action Ok;

        IColorScheme m_scheme;
        private Func<string, IEnumerable<string>> m_autoCompleteSuggestions;

        public IColorScheme Scheme
        {
            get { return m_scheme; }
            set
            {
                m_scheme = value;
                m_textBox.Colors.BorderPen = value.ControlBorder;
                drawWindow1.ColorScheme = value;
            }
        }
    }
}

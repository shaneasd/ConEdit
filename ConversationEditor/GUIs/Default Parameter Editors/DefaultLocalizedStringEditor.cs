﻿using System;
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

namespace ConversationEditor
{
    public partial class DefaultLocalizedStringEditor : UserControl, IParameterEditor<DefaultLocalizedStringEditor>
    {
        private MyTextBox m_textBox;

        public DefaultLocalizedStringEditor()
        {
            InitializeComponent();

            m_textBox = new MyTextBox(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), MyTextBox.InputFormEnum.Text);
            m_textBox.Colors.BorderPen = ColorScheme.ControlBorder;
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

        LocalizationEngine m_localizer;

        public bool WillEdit(ID<ParameterType> type, WillEdit willEdit)
        {
            return type == BaseTypeLocalizedString.PARAMETER_TYPE;
        }

        ILocalizedStringParameter m_parameter;
        public void Setup(IParameter parameter, LocalizationEngine localizer, IAudioProvider audioProvider)
        {
            m_parameter = parameter as ILocalizedStringParameter;
            m_localizer = localizer;
            if (!m_parameter.Corrupted)
                m_textBox.Text = m_localizer.Localize(m_parameter.Value) ?? "Missing Localization";
            else
                m_textBox.Text = "Missing Localization";
        }

        public DefaultLocalizedStringEditor AsControl
        {
            get { return this; }
        }

        public SimpleUndoPair? UpdateParameterAction()
        {
            var localized = m_parameter.Corrupted ? null : m_localizer.Localize(m_parameter.Value);
            if (m_textBox.Text != localized)
            {
                if (m_parameter.Corrupted)
                {
                    ID<LocalizedText> id = new ID<LocalizedText>();
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
                return null;
            }
        }

        public string DisplayName
        {
            get { return "Default String Editor"; }
        }

        public bool IsValid()
        {
            return true;
        }

        public event Action Ok;
    }
}

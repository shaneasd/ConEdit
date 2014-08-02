using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ConversationEditor;
using PluginPack.Properties;
using Conversation;
using Utilities;

namespace PluginPack
{
    public partial class CharacterEditor : UserControl, IParameterEditor<CharacterEditor>
    {
        public CharacterEditor()
        {
            InitializeComponent();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            panel1.Width = panel1.Height;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem.ToString() == "Shane")
                panel1.BackgroundImage = Resources.shane;
            else if (comboBox1.SelectedItem.ToString() == "Jonas")
                panel1.BackgroundImage = Resources.jonas;
            else if (comboBox1.SelectedItem.ToString() == "Neil")
                panel1.BackgroundImage = Resources.neil;
            else if (comboBox1.SelectedItem.ToString() == "Tammy")
                panel1.BackgroundImage = Resources.tammy;
            else if (comboBox1.SelectedItem.ToString() == "Player")
                panel1.BackgroundImage = Resources.Player;
            else
                panel1.BackgroundImage = new Bitmap(1, 1);
        }

        public bool WillEdit(ID<ParameterType> type, WillEdit willEdit)
        {
            return type == ID<ParameterType>.Parse("65ff02e9-3474-4892-b54d-6abbbfadd667");
        }

        private IEnumParameter m_parameter;
        public void Setup(IParameter parameter, LocalizationEngine localizer, IAudioProvider audioProvider)
        {
            m_parameter = parameter as IEnumParameter;
            foreach (var ch in m_parameter.Options)
                comboBox1.Items.Add(ch);
            if (comboBox1.Items.Contains(m_parameter.Value))
                comboBox1.SelectedItem = m_parameter.Value;
            else
                comboBox1.SelectedText = m_parameter.Value.ToString();
        }

        public CharacterEditor AsControl
        {
            get { return this; }
        }

        public SimpleUndoPair? UpdateParameterAction()
        {
            if (!IsValid())
                throw new Exception("Current character selection is invalid");
            var value = m_parameter.Value;
            return m_parameter.SetValueAction((Guid)comboBox1.SelectedItem);
        }

        public string DisplayName
        {
            get { return "Shane's Character Editor"; }
        }

        public bool IsValid()
        {
            return m_parameter.Options.Contains((Guid)comboBox1.SelectedItem);
        }

        public event Action Ok
        {
            add { }
            remove { }
        }
    }
}

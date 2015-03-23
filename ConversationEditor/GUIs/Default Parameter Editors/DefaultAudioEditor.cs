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
using System.IO;

namespace ConversationEditor
{
    public partial class DefaultAudioEditor : UserControl, IParameterEditor<DefaultAudioEditor>
    {
        public class Factory : IParameterEditorFactory
        {
            public static readonly Guid GUID = Guid.Parse("b5d1b3ea-5998-4e53-bf78-f09311f81405");
            public bool WillEdit(ParameterType type, WillEdit willEdit)
            {
                return type == BaseTypeAudio.PARAMETER_TYPE;
            }

            public string Name
            {
                get { return "Default Audio Editor"; }
            }

            public Guid Guid
            {
                get { return GUID; }
            }

            public IParameterEditor<Control> Make(ColorScheme scheme)
            {
                var result =  new DefaultAudioEditor();
                result.Scheme = scheme;
                return result;
            }
        }

        private MyTextBox m_textBox;

        public DefaultAudioEditor()
        {
            InitializeComponent();

            m_textBox = new MyTextBox(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), MyTextBox.InputFormEnum.Path);
            m_textBox.RequestedAreaChanged += () =>
            {
                int extraHeight = (Size.Height - drawWindow1.Size.Height).Clamp(0, int.MaxValue);
                drawWindow1.MinimumSize = new Size(0, (int)m_textBox.RequestedArea.Height);
                MinimumSize = new Size(MinimumSize.Width, (int)m_textBox.RequestedArea.Height + extraHeight);
                drawWindow1.Size = m_textBox.RequestedArea.ToSize();
                Size = new Size(Width, m_textBox.RequestedArea.ToSize().Height + extraHeight);
            };
            m_textBox.EnterPressed += () => Ok.Execute();
            m_textBox.SpecialEnter = true;
            MyTextBox.SetupCallbacks(drawWindow1, m_textBox);
            drawWindow1.SizeChanged += new EventHandler(drawWindow1_SizeChanged);
        }

        void drawWindow1_SizeChanged(object sender, EventArgs e)
        {
        }

        public event Action Ok;
        AudioGenerationParameters m_audioGenerationParameters;
        IAudioParameter m_parameter;
        IAudioProvider m_audioProvider;
        public void Setup(ParameterEditorSetupData data)
        {
            m_parameter = data.Parameter as IAudioParameter;
            m_audioProvider = data.AudioProvider;
            m_audioGenerationParameters = data.AudioGenerationParameters;
            if (!data.Parameter.Corrupted)
                m_textBox.Text = m_parameter.Value.Value;
            else if (m_parameter.Value.Value == null)
                Generate();
        }

        public DefaultAudioEditor AsControl
        {
            get { return this; }
        }

        public UpdateParameterData UpdateParameterAction()
        {
            if (!IsValid())
                throw new Exception("Current path invalid");

            Audio audio = new Audio(m_textBox.Text);
            UpdateParameterData result = m_parameter.SetValueAction(audio);
            if (result.Actions != null)
                result.Audio = audio;
            return result;
        }

        public bool IsValid()
        {
            try
            {
                new FileInfo(m_textBox.Text);
                return true;
            }
            catch
            {
                return false;
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (!IsValid())
                MessageBox.Show("Current path invalid");
            else
                m_audioProvider.Play(new Audio(m_textBox.Text));
        }

        private void Generate()
        {
            m_textBox.Text = m_audioProvider.Generate(m_audioGenerationParameters).Value;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            Generate();
        }

        private ColorScheme m_scheme;
        public ColorScheme Scheme
        {
            get { return m_scheme; }
            set
            {
                m_scheme = value;
                m_textBox.Colors.BorderPen = value.ControlBorder;
                ForeColor = value.Foreground;
            }
        }
    }
}

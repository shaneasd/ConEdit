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
using Utilities.UI;

namespace ConversationEditor
{
    public class DefaultAudioEditorFactory : IParameterEditorFactory
    {
        public static readonly Guid StaticId = Guid.Parse("b5d1b3ea-5998-4e53-bf78-f09311f81405");
        public bool WillEdit(ParameterType type, WillEdit queries)
        {
            return type == BaseTypeAudio.ParameterType;
        }

        public string Name
        {
            get { return "Default Audio Editor"; }
        }

        public Guid Guid
        {
            get { return StaticId; }
        }

        public IParameterEditor Make(IColorScheme scheme)
        {
            return new DefaultAudioEditor(scheme);
        }
    }

    internal partial class DefaultAudioEditor : UserControl, IParameterEditor
    {
        private MyTextBox m_textBox;

        public DefaultAudioEditor(IColorScheme scheme) : this()
        {
            Scheme = scheme;
        }

        public DefaultAudioEditor()
        {
            InitializeComponent();

            //TODO: Suggest paths that exist for autoCompleteSuggestions
            m_textBox = new MyTextBox(drawWindow1, () => new RectangleF(0, 0, drawWindow1.Width, drawWindow1.Height), MyTextBox.InputFormEnum.Path, null);
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
        IAudioParameterEditorCallbacks m_audioProvider;
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

        public Control AsControl
        {
            get { return this; }
        }

        public UpdateParameterData UpdateParameterAction()
        {
            if (IsValid() != null)
                throw new InvalidOperationException("Current path invalid");

            Audio audio = new Audio(m_textBox.Text);
            UpdateParameterData result = m_parameter.SetValueAction(audio);
            if (result.Actions != null)
                result.Audio = audio;
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "System.IO.FileInfo", Justification ="FileInfo is created to see if the path is valid but we dont actually need to use it")]
        public string IsValid()
        {
            try
            {
                new FileInfo(m_textBox.Text);
                return null;
            }
            catch (System.Security.SecurityException) { }
            catch (System.ArgumentException) { }
            catch (System.UnauthorizedAccessException) { }
            catch (System.IO.PathTooLongException) { }
            catch (System.NotSupportedException) { }
            return "Path invalid";
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (IsValid() != null)
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

        private IColorScheme m_scheme;
        public IColorScheme Scheme
        {
            get { return m_scheme; }
            set
            {
                m_scheme = value;
                m_textBox.Colors.BorderPen = value.ControlBorder;
                ForeColor = value.Foreground;
                drawWindow1.ColorScheme = value;
            }
        }
    }
}

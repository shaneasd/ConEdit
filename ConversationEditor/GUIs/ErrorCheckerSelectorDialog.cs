using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using Conversation;
using Utilities;

namespace ConversationEditor
{
    internal partial class ErrorCheckerSelectorDialog : Form
    {
        private CheckList<ErrorCheckerController.ErrorCheckerData> m_list;
        private ErrorCheckersConfig m_config;
        private PluginsConfig m_pluginsConfig;

        IColorScheme m_scheme;
        public IColorScheme Scheme
        {
            get { return m_scheme; }
            set
            {
                m_scheme = value;
                BackColor = value.FormBackground;
                btnOk.BackColor = value.Background;
                btnOk.ForeColor = value.Foreground;
                //greyScrollBar1.ColorScheme = value;
                drawWindow1.ColorScheme = value;
            }
        }

        public ErrorCheckerSelectorDialog()
        {
            InitializeComponent();

            m_list = new CheckList<ErrorCheckerController.ErrorCheckerData>(item => item.DisplayName);

            this.greyScrollBar1.Scrolled += () => { drawWindow1.Invalidate(true); };
            Resize += (a, b) => m_list.UpdateScrollBar(greyScrollBar1, drawWindow1.Height);

            FontChanged += (a, b) => m_list.Font = Font;
            m_list.Font = Font;
        }

        private void PopulateListBox()
        {
            foreach (var pluginAssembly in m_pluginsConfig.UnfilteredAssemblies(MainAssembly.Include))
            {
                ErrorCheckerAssembly configECA = m_config.GetAssembly(pluginAssembly);
                ErrorCheckerAssembly eca = new ErrorCheckerAssembly(pluginAssembly);

                foreach (var checker in eca.Types)
                {
                    if (configECA != null && configECA.Types.Any(c => c.SerializeName == checker.SerializeName))
                    {
                        m_list.AddItem(checker, configECA.Types[checker.SerializeName].Enabled);
                    }
                    else
                    {
                        m_list.AddItem(checker, true);
                    }
                }
            }
        }

        internal void Init(ErrorCheckersConfig checkersConfig, PluginsConfig pluginsConfig)
        {
            m_config = checkersConfig;
            m_pluginsConfig = pluginsConfig;
            PopulateListBox();
            drawWindow1.Invalidate(true);
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            foreach (var pluginAssembly in m_pluginsConfig.UnfilteredAssemblies(MainAssembly.Include))
            {
                ErrorCheckerAssembly configECA = m_config.GetAssembly(pluginAssembly);
                if (configECA == null)
                {
                    configECA = new ErrorCheckerAssembly(pluginAssembly);
                    m_config.AddAssembly(configECA);
                }
                foreach (ErrorCheckerController.ErrorCheckerData checker in configECA.Types)
                {
                    var enabledCheckers = m_list.Items.Where(i => i.Checked);
                    checker.Enabled = enabledCheckers.Any(c => c.Element.SerializeName == checker.SerializeName);
                }
            }
            m_config.Save();
            Close();
        }

        private void drawWindow1_Paint(object sender, PaintEventArgs e)
        {
            m_list.DrawItems(m_scheme, e.Graphics, (int)greyScrollBar1.Value);
            e.Graphics.DrawRectangle(Scheme.ControlBorder, new Rectangle(0, 0, drawWindow1.Width - 1, drawWindow1.Height - 1));
        }

        private void drawWindow1_MouseClick(object sender, MouseEventArgs e)
        {
            m_list.MouseClick(e.Location, (int)greyScrollBar1.Value);
            drawWindow1.Invalidate(true);
        }

        private void drawWindow1_MouseDown(object sender, MouseEventArgs e)
        {
            m_list.MouseDown(e.Location, (int)greyScrollBar1.Value);
            drawWindow1.Invalidate(true);
        }

        private void drawWindow1_MouseUp(object sender, MouseEventArgs e)
        {
            m_list.MouseUp();
            drawWindow1.Invalidate(true);
        }

        private void drawWindow1_MouseCaptureChanged(object sender, EventArgs e)
        {
            m_list.MouseCaptureChanged();
            drawWindow1.Invalidate(true);
        }

        private void drawWindow1_MouseMove(object sender, MouseEventArgs e)
        {
            m_list.MouseMove(e.Location, (int)greyScrollBar1.Value);
            drawWindow1.Invalidate(true);
        }
    }
}

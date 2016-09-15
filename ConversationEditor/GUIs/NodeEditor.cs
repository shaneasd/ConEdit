using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Conversation;
using System.IO;
using System.Reflection;
using Utilities;
using System.Diagnostics;

namespace ConversationEditor
{
    internal partial class NodeEditor : UserControl
    {
        IEditable m_data;

        ColorScheme m_scheme;
        ColorScheme Scheme
        {
            get { return m_scheme; }
            set
            {
                m_scheme = value;
                this.BackColor = value.FormBackground;
                this.okButton.BackColor = value.Background;
                this.cancelButton.BackColor = value.Background;
                okButton.ForeColor = value.Foreground;
                cancelButton.ForeColor = value.Foreground;
            }
        }

        public NodeEditor()
        {
            InitializeComponent();
        }

        public static ConfigureResult2 Edit(ColorScheme scheme, IEditable data, AudioGenerationParameters audioContext, Func<ParameterType, ParameterEditorSetupData, IParameterEditor<Control>> config, ILocalizationEngine localizer, IAudioParameterEditorCallbacks audioProvider, Func<IParameter, string, IEnumerable<string>> autoCompleteSuggestions)
        {
            using (Form f = new Form())
            {
                NodeEditor editor = new NodeEditor(scheme, data, audioContext, config, localizer, audioProvider, autoCompleteSuggestions);

                f.Text = editor.Title;
                bool oked = false;
                editor.Ok += () =>
                {
                    var invalid = editor.m_parameterEditors.Select(e => new { Name = e.Item2.Name, Message = e.Item1.IsValid() }).Where(e => e.Message != null);
                    if (!invalid.Any())
                    {
                        oked = true;
                        f.Close();
                    }
                    else
                    {
                        string message = "Invalid values specified for the following parameters:";
                        foreach (var i in invalid)
                        {
                            message += "\n";
                            message += i.Name + ": ";
                            message += i.Message;
                        }

                        MessageBox.Show(message);
                    }
                };
                editor.Cancel += () => { oked = false; f.Close(); };
                editor.Dock = DockStyle.Fill;
                int maxHeight = editor.MaximumHeight + f.Height - f.ClientSize.Height;
                f.MaximumSize = new Size(999999, maxHeight);
                editor.NeedsResize += () =>
                {
                    bool resize = false;
                    if (f.Size.Height == f.MaximumSize.Height)
                    {
                        resize = true;
                    }
                    int m = editor.MaximumHeight + f.Height - f.ClientSize.Height;
                    f.MaximumSize = new Size(999999, m);
                    if (resize)
                    {
                        f.Size = new Size(f.Size.Width, m);
                    }
                };
                f.Size = new System.Drawing.Size(500, 478);
                f.Controls.Add(editor);
                f.ShowDialog();

                if (oked)
                {
                    var updates = editor.m_parameterEditors.Select(e => e.Item1.UpdateParameterAction());
                    return new ConfigureResult2(updates.ToArray());
                }
                else
                {
                    return ConfigureResult.Cancel;
                }
            }
        }

        public string Title { get; private set; }

        public NodeEditor(ColorScheme scheme, IEditable data, AudioGenerationParameters audioContext, Func<ParameterType, ParameterEditorSetupData, IParameterEditor<Control>> config, ILocalizationEngine localizer, IAudioParameterEditorCallbacks audioProvider, Func<IParameter, string, IEnumerable<string>> autoCompleteSuggestions)
            : this()
        {
            Scheme = scheme;
            m_data = data;

            this.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            flowLayoutPanel2.SuspendLayout();

            //Make the panels really tall so they can visibly contain all the parameter editors.
            //Note the whole control won't be visible as we'll scroll them by shifting them in Y.
            flowLayoutPanel1.Height = 10000;
            flowLayoutPanel2.Height = 10000;

            Title = m_data.Name;

            foreach (Parameter p in m_data.Parameters.OrderByDescending(p => p.Name))
            {
                var editorData = new ParameterEditorSetupData(p, localizer, audioProvider, audioContext, (s) => autoCompleteSuggestions(p, s));
                var unknown = p as UnknownParameter;
                if (unknown != null)
                {
                    UnknownParameterEditor ed = null;
                    Label label = null;
                    ed = UnknownParameterEditor.Make(Scheme, editorData, m_data.RemoveUnknownParameter(unknown),
                    () =>
                    {
                        flowLayoutPanel2.Controls.Remove(ed);
                        flowLayoutPanel1.Controls.Remove(label);
                        SetupScrollbar();
                    });
                    label = AddParameter(p, ed.AsControl);
                }
                else
                {
                    AddParameter(p, config(p.TypeId, editorData));
                }
            }

            flowLayoutPanel1.ResumeLayout();
            flowLayoutPanel2.ResumeLayout();

            SetupScrollbar();

            if (flowLayoutPanel1.Controls.Count > 0)
            {
                flowLayoutPanel2.Controls[0].LocationChanged += (a, b) =>
                {
                    NeedsResize.Execute();
                    DoResize();
                };
                flowLayoutPanel2.Controls[0].SizeChanged += (a, b) =>
                {
                    NeedsResize.Execute();
                    DoResize();
                };
            };

            this.ResumeLayout();

            this.splitContainer1.Panel2.SizeChanged += Panel2_SizeChanged;

            if (flowLayoutPanel2.Controls.Count > 0)
            {
                for (int i = flowLayoutPanel2.Controls.Count - 1; i >= 0; i--)
                {
                    (flowLayoutPanel2.Controls[i] as Panel).TabStop = true;
                    (flowLayoutPanel2.Controls[i] as Panel).TabIndex = flowLayoutPanel2.Controls.Count - i - 1;
                }
            }
        }

        public event Action NeedsResize;
        public void DoResize()
        {
            Height = 999999;
            SetupScrollbar();
        }

        private void SetupScrollbar()
        {
            if (flowLayoutPanel1.Controls.Count > 0)
            {
                greyScrollBar1.PercentageCovered = Util.Clamp((float)WindowSize / TotalSize, 0.0f, 1.0f);
                greyScrollBar1.Minimum = 0;
                greyScrollBar1.Maximum = Math.Max(0, TotalSize - WindowSize);
                greyScrollBar1.Scrolled += GreyScrollBar1_Scrolled;
            }
            else
            {
                greyScrollBar1.PercentageCovered = 1.0f;
                greyScrollBar1.Minimum = 0;
                greyScrollBar1.Maximum = 0;
            }
            //Make sure the current state of the scrollbar is reflected in case this was called as a result of a child control resize
            GreyScrollBar1_Scrolled();
        }

        private int MaximumHeight
        {
            get
            {
                return TotalSize + 47;
            }
        }

        private void Panel2_SizeChanged(object sender, EventArgs e)
        {
            SetupScrollbar();
        }

        private void GreyScrollBar1_Scrolled()
        {
            int pos = -(int)(greyScrollBar1.Value);
            flowLayoutPanel1.Top = pos + 3;
            flowLayoutPanel2.Top = pos + 3;
        }

        int TotalSize { get { return flowLayoutPanel2.Controls[0]?.Bottom ?? 0; } }
        int WindowSize { get { return splitContainer1.Panel2.Height; } }

        public Label AddParameter(IParameter parameter, IParameterEditor<Control> editor)
        {
            Panel p = new Panel();
            p.Size = new Size(flowLayoutPanel2.Width, editor.AsControl.Height + 3);
            p.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            p.Dock = DockStyle.Top;
            editor.AsControl.Width = p.Width;
            editor.AsControl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            p.Size = new Size(flowLayoutPanel2.Width, editor.AsControl.Height + 3);

            Label label = new Label();
            label.AutoSize = false;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Dock = DockStyle.Top;
            label.Text = parameter.Name;
            label.ForeColor = Scheme.Foreground;
            label.Size = new Size(40, p.Height);

            editor.AsControl.SizeChanged += (a, args) =>
            {
                p.Height = editor.AsControl.Height + 3;
                label.Height = p.Height;
            };
            p.Controls.Add(editor.AsControl);

            m_parameterEditors.Add(Tuple.Create(editor, parameter));

            flowLayoutPanel1.Controls.Add(label);
            flowLayoutPanel2.Controls.Add(p);

            return label;
        }

        List<Tuple<IParameterEditor<Control>, IParameter>> m_parameterEditors = new List<Tuple<IParameterEditor<Control>, IParameter>>();

        public event Action Cancel;
        public event Action Ok
        {
            add
            {
                foreach (var a in m_parameterEditors)
                    a.Item1.Ok += value;
                m_ok += value;
            }
            remove
            {
                foreach (var a in m_parameterEditors)
                    a.Item1.Ok -= value;
                m_ok += value;
            }
        }
        private event Action m_ok;

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Cancel();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            m_ok();
        }
    }

    public class DefaultNodeEditorFactory : NodeEditorFactory
    {
        public override bool WillEdit(Id<NodeTypeTemp> typeId)
        {
            //The default node editor can handle any node. You may not want it to.
            return true;
        }

        public override ConfigureResult2 Edit(ColorScheme scheme, IEditable node, AudioGenerationParameters audioContext, Func<ParameterType, ParameterEditorSetupData, IParameterEditor<Control>> config, ILocalizationEngine localizer, IAudioParameterEditorCallbacks audioProvider, Func<IParameter, string, IEnumerable<string>> autoCompleteSuggestions)
        {
            if (node.Parameters.Any())
                return NodeEditor.Edit(scheme, node, audioContext, config, localizer, audioProvider, autoCompleteSuggestions);
            else
                return ConfigureResult2.NotApplicable;
        }

        public override string DisplayName
        {
            get { return "Default Node Editor"; }
        }
    }
}

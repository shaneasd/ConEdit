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
                f.Controls.Add(editor);
                f.AutoSize = true;
                f.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
                f.FormBorderStyle = FormBorderStyle.FixedSingle;
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

        //private static ConfigureResult OnOk(IAudioProvider2 audioProvider, IEnumerable<UpdateParameterData> updates)
        //{
        //    List<Action> undo = new List<Action>();
        //    List<Action> redo = new List<Action>();
        //    foreach (UpdateParameterData updateParameterData in updates)
        //    {
        //        if (updateParameterData != null)
        //        {
        //            SimpleUndoPair? actions = updateParameterData.Actions;
        //            if (actions != null)
        //            {
        //                undo.Add(actions.Value.Undo);
        //                redo.Add(actions.Value.Redo);
        //            }
        //            if (updateParameterData.Audio != null)
        //            {
        //                undo.Add(() => audioProvider.UpdateUsage(updateParameterData.Audio.Value));
        //                redo.Add(() => audioProvider.UpdateUsage(updateParameterData.Audio.Value));
        //            }
        //        }
        //    }
        //    if (undo.Any())
        //    {
        //        return new SimpleUndoPair { Redo = () => redo.ForEach(a => a()), Undo = () => undo.ForEach(a => a()) };
        //    }
        //    else
        //    {
        //        return ConfigureResult.NotApplicable; //This isn't exactly what NotApplicable was intended for but it's the closest match and I can't see a functional difference
        //    }
        //}

        public string Title { get; private set; }

        public NodeEditor(ColorScheme scheme, IEditable data, AudioGenerationParameters audioContext, Func<ParameterType, ParameterEditorSetupData, IParameterEditor<Control>> config, ILocalizationEngine localizer, IAudioParameterEditorCallbacks audioProvider, Func<IParameter, string, IEnumerable<string>> autoCompleteSuggestions)
            : this()
        {
            Scheme = scheme;
            m_data = data;

            this.SuspendLayout();
            Title = m_data.Name;
            int parameterCount = 0;
            foreach (Parameter p in m_data.Parameters.OrderBy(p => p.Name))
            {
                var editorData = new ParameterEditorSetupData(p, localizer, audioProvider, audioContext, (s)=> autoCompleteSuggestions(p, s));
                var unknown = p as UnknownParameter;
                if (unknown != null)
                {
                    UnknownParameterEditor ed = null;
                    ed = UnknownParameterEditor.Make(Scheme, editorData, m_data.RemoveUnknownParameter(unknown),
                    () =>
                    {
                        int row = tableLayoutPanel1.GetRow(ed);
                        tableLayoutPanel1.RowStyles[row].SizeType = SizeType.Absolute;
                        tableLayoutPanel1.RowStyles[row].Height = 0;
                        tableLayoutPanel1.Controls.Remove(ed);
                    });
                    AddParameter(p, ed.AsControl);
                }
                else
                {
                    AddParameter(p, config(p.TypeId, editorData));
                }
                parameterCount++;
            }

            //if (parameterCount > 15)
            //{
            //    tableLayoutPanel3.ColumnStyles[4].Width = 105;
            //    tableLayoutPanel3.Controls.Add(new Button() { Width = 100 }, 4, 0);
            //}

            //Add a buffer to fill up the space
            tableLayoutPanel1.RowCount++;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100));


            this.ResumeLayout();
        }

        public void AddParameter(IParameter parameter, IParameterEditor<Control> editor)
        {
            Label label = new Label();
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.AutoSize = true;
            label.Dock = DockStyle.Fill;
            label.Text = parameter.Name;
            label.ForeColor = Scheme.Foreground;

            editor.AsControl.Dock = DockStyle.Top;
            m_parameterEditors.Add(Tuple.Create(editor, parameter));

            tableLayoutPanel1.RowCount++;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            tableLayoutPanel1.Controls.Add(label, 0, tableLayoutPanel1.RowCount - 1);
            tableLayoutPanel1.Controls.Add(editor.AsControl, 1, tableLayoutPanel1.RowCount - 1);
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

        private void tableLayoutPanel3_Resize(object sender, EventArgs e)
        {
            //tableLayoutPanel1.MaximumSize = new Size(int.MaxValue, Math.Max(0, tableLayoutPanel3.Height - 37));
            //tableLayoutPanel1.MaximumSize = new Size(int.MaxValue, 50);
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

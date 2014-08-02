using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Conversation;
using System.Text.RegularExpressions;
using Utilities;
using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI>;

namespace ConversationEditor
{
    using TData = IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>;
    public partial class FindAndReplaceDialog : Form
    {
        private IEnumerable<TData> m_search;
        private LocalizationEngine m_localizer;
        private Func<IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>> m_currentDocument;
        public FindAndReplaceDialog(IEnumerable<TData> search, LocalizationEngine localizer, Func<IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>> currentDocument)
            : this()
        {
            m_search = search;
            m_localizer = localizer;
            m_currentDocument = currentDocument;
        }

        public FindAndReplaceDialog()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FindAndReplace();
            Close();
        }

        private bool Replace(string input, string replace, string with, out string output)
        {
            bool matchCase = chkMatchCase.Checked;
            bool wholeWord = chkWholeWord.Checked;
            bool cleverCase = chkPreserveCase.Checked;
            Func<string, string> casedReplace = s =>
                {
                    if (char.IsUpper(s[0]))
                    {
                        if (s.All(c => char.IsUpper(c)))
                            return with.ToUpper();
                        else if (s.Skip(1).All(c => !char.IsUpper(c)))
                        {
                            return char.ToUpper(with[0]) + with.Substring(1);
                        }
                    }
                    return with;
                };
            if (wholeWord)
                replace = @"\b" + replace + @"\b";
            output = Regex.Replace(input, replace, m => cleverCase ? casedReplace(m.Value) : with, matchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
            return output != input;
        }

        private void FindAndReplace()
        {
            do
            {
                m_currentItem.Current.Execute();
            }
            while (m_currentItem.MoveNext());
        }

        struct ReplaceAction
        {
            public ReplaceAction(SimpleUndoPair undoredo, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> file, ConversationNode node)
                : this(undoredo.Undo, undoredo.Redo, file, node)
            {
            }

            public ReplaceAction(Action undo, Action redo, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> file, ConversationNode node)
            {
                m_undo = undo;
                m_redo = redo;
                File = file;
                Node = node;
                m_done = false;
            }

            Action m_undo;
            Action m_redo;
            public readonly IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> File;
            public readonly ConversationNode Node;
            bool m_done;
            public bool Done { get { return m_done; } }
            public void Execute()
            {
                m_done = true;
                File.UndoableFile.Change(new GenericUndoAction(m_undo, m_redo, "Replaced text"));
            }
        }

        private IEnumerable<ReplaceAction> FindAll()
        {
            IEnumerable<TData> search = chkCurrentConversationOnly.Checked ? m_currentDocument().Only<TData>() : m_search;

            foreach (var file in search)
            {
                foreach (var node in file.Nodes.Evaluate())
                {

                    foreach (var parameter in node.Parameters.OfType<IStringParameter>())
                    {
                        if (!radLocalizedTextOnly.Checked)
                        {
                            string original = parameter.Value;
                            string output;
                            if (Replace(original, txtFind.Text, txtReplace.Text, out output))
                            {
                                //Treat replace actions as true actions even if they don't actually alter the value of the parameter
                                yield return new ReplaceAction(parameter.SetValueAction(output) ?? new SimpleUndoPair() { Redo = () => {}, Undo = ()=> {}}, file, node);
                            };
                        }
                    }

                    foreach (var parameter in node.Parameters.OfType<ILocalizedStringParameter>())
                    {
                        if (!radNonLocalizedTextOnly.Checked)
                        {
                            var original = m_localizer.Localize(parameter.Value);
                            if (original != null)
                            {
                                string output;
                                if (Replace(original, txtFind.Text, txtReplace.Text, out output))
                                {
                                    SimpleUndoPair redoUndo = m_localizer.SetLocalizationAction(parameter.Value, output);
                                    Action redo = () => { redoUndo.Redo(); UpdateDisplay.Execute(); };
                                    Action undo = () => { redoUndo.Undo(); UpdateDisplay.Execute(); };
                                    yield return new ReplaceAction(undo, redo, file, node);
                                }
                            }
                        }
                    }
                }
            }
        }

        IEnumerator<ReplaceAction> m_currentItem = null;

        public event Action<ConversationNode, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>> FocusNode;
        public event Action UpdateDisplay;

        private void btnFindNext_Click(object sender, EventArgs e)
        {
            if (m_currentItem == null)
            {
                m_currentItem = FindAll().GetEnumerator();
            }
            MoveNext();
        }

        private void MoveNext()
        {
            if (m_currentItem.MoveNext())
            {
                FocusNode.Execute(m_currentItem.Current.Node, m_currentItem.Current.File);
            }
            else
            {
                MessageBox.Show("Find reached the starting point of the search");
                m_currentItem = null;
            }
        }

        private void btnReplace_Click(object sender, EventArgs e)
        {
            if (m_currentItem == null)
                return;
            if (m_currentItem.Current.Done)
                return;
            m_currentItem.Current.Execute();
            MoveNext();
        }
    }
}

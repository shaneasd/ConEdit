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
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;

namespace ConversationEditor
{
    using TData = IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>;
    using System.Globalization;
    using System.Diagnostics;

    internal partial class FindAndReplaceDialog : Form
    {
        private class ResultListElement : IErrorListElement
        {
            public ResultListElement(TData file, string message, ConversationNode<INodeGui> node)
            {
                File = file;
                Message = message;
                Nodes = node.Only();
            }

            public TData File { get; }

            public string Message { get; }

            public IEnumerable<ConversationNode<INodeGui>> Nodes { get; }

            public IEnumerator<Tuple<ConversationNode<INodeGui>, TData>> MakeEnumerator()
            {
                return Nodes.Select(n => new Tuple<ConversationNode, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>>(n, File)).InfiniteRepeat().GetEnumerator();
            }
        }

        private IEnumerable<TData> m_search;
        private LocalizationEngine m_localizer;
        private Func<IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>> m_currentDocument;
        private Action<IEnumerable<IErrorListElement>> m_showResults;
        public FindAndReplaceDialog(IEnumerable<TData> search, LocalizationEngine localizer, Func<IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>> currentDocument, Action<IEnumerable<IErrorListElement>> showResults)
            : this()
        {
            m_search = search;
            m_localizer = localizer;
            m_currentDocument = currentDocument;
            m_showResults = showResults;
        }

        public FindAndReplaceDialog()
        {
            InitializeComponent();
        }

        private void ReplaceAllClicked(object sender, EventArgs e)
        {
            FindAndReplace();
            Close();
        }

        Regex GenerateRegex(string replace)
        {
            bool matchCase = chkMatchCase.Checked;
            bool wholeWord = chkWholeWord.Checked;

            if (!chkRegex.Checked)
                replace = Regex.Escape(replace);
            if (wholeWord)
                replace = @"\b" + replace + @"\b";
            try
            {
                return new Regex(replace, matchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private bool Replace(string input, Regex r, string with, out string output)
        {
            bool cleverCase = chkPreserveCase.Checked;
            Func<string, string> casedReplace = s =>
                {
                    if (char.IsUpper(s[0]))
                    {
                        if (s.All(c => char.IsUpper(c)))
                            return with.ToUpper(CultureInfo.CurrentCulture);
                        else if (s.Skip(1).All(c => !char.IsUpper(c)))
                        {
                            return char.ToUpper(with[0], CultureInfo.CurrentCulture) + with.Substring(1);
                        }
                    }
                    return with;
                };
            output = r.Replace(input, m => cleverCase ? casedReplace(m.Value) : with);
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
            public ReplaceAction(SimpleUndoPair undoredo, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> file, ConversationNode node, IParameter parameter)
                : this(undoredo.Undo, undoredo.Redo, file, node, parameter)
            {
            }

            public ReplaceAction(Action undo, Action redo, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> file, ConversationNode node, IParameter parameter)
            {
                m_undo = undo;
                m_redo = redo;
                File = file;
                Node = node;
                Parameter = parameter;
                m_done = false;
            }

            Action m_undo;
            Action m_redo;
            public readonly IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> File;
            public readonly ConversationNode Node;
            public readonly IParameter Parameter;
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

            Regex find = GenerateRegex(txtFind.Text);

            if (find == null)
            {
                MessageBox.Show("Invalid regular expression");
                yield break;
            }

            foreach (var file in search)
            {
                foreach (var node in file.Nodes.Evaluate())
                {

                    if (chkStrings.Checked)
                    {
                        foreach (var parameter in node.Data.Parameters.OfType<IStringParameter>())
                        {
                            string original = parameter.Value;
                            string output;
                            if (Replace(original, find, txtReplace.Text, out output))
                            {
                                //Treat replace actions as true actions even if they don't actually alter the value of the parameter
                                yield return new ReplaceAction(parameter.SetValueAction(output) ?? new SimpleUndoPair() { Redo = () => { }, Undo = () => { } }, file, node, parameter);
                            };
                        }
                    }

                    if (chkLocalizedStrings.Checked)
                    {
                        foreach (var parameter in node.Data.Parameters.OfType<ILocalizedStringParameter>())
                        {
                            var original = m_localizer.Localize(Id<LocalizedStringType>.FromGuid(parameter.TypeId.Guid), parameter.Value);
                            if (original != null)
                            {
                                string output;
                                if (Replace(original, find, txtReplace.Text, out output))
                                {
                                    SimpleUndoPair redoUndo = m_localizer.SetLocalizationAction(Id<LocalizedStringType>.ConvertFrom(parameter.TypeId), parameter.Value, output);
                                    Action redo = () => { redoUndo.Redo(); UpdateDisplay.Execute(); };
                                    Action undo = () => { redoUndo.Undo(); UpdateDisplay.Execute(); };
                                    yield return new ReplaceAction(undo, redo, file, node, parameter);
                                }
                            }
                        }
                    }

                    if (chkDynamicEnumerations.Checked)
                    {
                        foreach (var parameter in node.Data.Parameters.OfType<IDynamicEnumParameter>().Where(x => !x.Corrupted))
                        {
                            string original = parameter.Value;
                            string output;
                            if (original != null) //DynamicEnumParameters shouldn't have a value of null typically but DomainDomain.EnumDefaultParameter can
                            {
                                if (Replace(original, find, txtReplace.Text, out output))
                                {
                                    //Treat replace actions as true actions even if they don't actually alter the value of the parameter
                                    yield return new ReplaceAction(parameter.SetValueAction(output) ?? new SimpleUndoPair() { Redo = () => { }, Undo = () => { } }, file, node, parameter);
                                };
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
                //TODO: if any search parameter changes reset m_currentItem so we don't get this message
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

        private void btnFindAll_Click(object sender, EventArgs e)
        {
            m_showResults(FindAll().Select(x => new ResultListElement(x.File, x.Node.Data.Name + ": " + x.Parameter.Name, x.Node)));
        }

        private void SettingChanged(object sender, EventArgs e)
        {
            m_currentItem = null;
        }
    }
}

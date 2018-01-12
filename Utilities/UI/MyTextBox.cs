using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Utilities.UI
{
    public class MyTextBox : MyControl, IDisposable
    {
        public static ImageBorderDrawer TextBoxBorderDaniel { get; }
        public static ImageBorderDrawer ComboBoxBorderDaniel { get; }

        private IBorderDrawer Border { get; }

        static MyTextBox()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.InputField.png"))
            {
                TextBoxBorderDaniel = new ImageBorderDrawer(new Bitmap(stream), 4);
            }
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ComboBoxDropdown.png"))
            {
                ComboBoxBorderDaniel = new ImageBorderDrawer(new Bitmap(stream), 4);
            }
        }

        private class UndoAction : Utilities.UndoAction
        {
            private State m_undoState;
            private Action<State> SetState;
            private State m_redoState;
            public UndoAction(State undoState, Action<State> setState)
            {
                m_undoState = undoState;
                SetState = setState;
            }

            public void SetRedo(State redoState)
            {
                m_redoState = redoState;
            }

            public override void Undo()
            {
                SetState(m_undoState);
            }

            public override void Redo()
            {
                SetState(m_redoState);
            }

            public override string Description => "Edited text box";

            public override string LogDescription => Description;
        }

        private struct State
        {
            public string Text;
            public CP CursorPos;
            public int SelectionLength;
        }

        //TODO: Replace ColorOptions concept with something more akin to the BorderDrawer concept so that we can avoid redundancy in things like background color
        public class ColorOptions
        {
            public ColorOptions()
            {
                Text = Color.FromArgb(205, 205, 205);
                SelectedText = Color.Cyan;
                SelectedBackgroundBrush = Brushes.Navy;
                Background = Color.FromArgb(56, 56, 56);
                BorderPen = new Pen(Color.FromArgb(205, 205, 205));
                AutoCompleteText = Color.Cyan;
                AutoCompleteBackground = Color.Black;
                TokenText = Color.AntiqueWhite;
                TokenBackground = Color.Black;
            }

            Color m_text;
            public Color Text
            {
                get { return m_text; }
                set
                {
                    m_text = value;
                    if (TextBrush != null)
                        TextBrush.Dispose();
                    TextBrush = new SolidBrush(m_text);
                }
            }

            Color m_seletedText;
            public Color SelectedText
            {
                get { return m_seletedText; }
                set
                {
                    m_seletedText = value;
                    if (SelectedTextBrush != null)
                        SelectedTextBrush.Dispose();
                    SelectedTextBrush = new SolidBrush(m_seletedText);
                }
            }

            public Color AutoCompleteText { get; set; }
            public Color AutoCompleteBackground { get; set; }

            public Color TokenText { get; set; }
            public Color TokenBackground { get; set; }

            public Pen BorderPen { get; set; }

            public Brush TextBrush { get; protected set; }
            public Brush SelectedTextBrush { get; protected set; }

            private Color m_background;
            public Color Background { get { return m_background; } set { m_background = value; BackgroundBrush = new SolidBrush(Background); } }
            public Brush BackgroundBrush { get; protected set; }

            private Color m_selectedBackground;
            public Color SelectedBackground { get { return m_selectedBackground; } set { m_selectedBackground = value; SelectedBackgroundBrush = new SolidBrush(m_selectedBackground); } }
            public Brush SelectedBackgroundBrush { get; protected set; }
        }

        public ColorOptions Colors { get; set; }

        State m_state = new State { Text = "", CursorPos = new CP(0), SelectionLength = 0 };

        bool m_uninitializedText = true;
        public string Text
        {
            get { return m_state.Text; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value), "Textbox Text cannot be null");
                if (m_state.Text != value || m_uninitializedText)
                {
                    m_uninitializedText = false;
                    var oldText = m_state.Text;
                    m_state.Text = value;

                    if (m_autoCompleteRange != null)
                    {
                        m_autoCompleteRange = new Tuple<CP, CP>(m_autoCompleteRange.Item1, new CP(m_autoCompleteRange.Item2.Pos + value.Length - oldText.Length, true));
                    }

                    TextChanged.Execute(oldText);
                    UpdateRequestedArea();
                    this.Redraw();
                }
            }
        }

        /// <summary>
        /// Represents a cursor position within a text UI element
        /// </summary>
        private struct CP
        {
            /// <summary>
            /// The index of the cursor. 0 is before the first character. 1 is after the first character.
            /// </summary>
            public int Pos;

            /// <summary>
            /// Put the cursor at the end of line i instead of the start of line i+1
            /// </summary>
            private bool m_preferEnd;

            public CP(int pos, bool preferEnd) { Pos = pos; m_preferEnd = preferEnd; }
            public CP(int pos) : this(pos, false) { }

            /// <summary>
            /// Given a list of strings representing a series of lines determine the U (cursor position within a line with 0 being before the first character)
            /// V (line number with 0 being the first line) coordinate of the cursor.
            /// </summary>
            /// <param name="lines">The lines making up the block of text</param>
            /// <returns>UV coordinate of the cursor within the block of text</returns>
            internal Point GetUV(IList<string> lines)
            {
                int before = 0;
                int i = 0;
                while (i < lines.Count && lines[i].Length + before + (m_preferEnd ? 1 : 0) <= Pos)
                {
                    before += lines[i].Length;
                    i++;
                }

                if (i == lines.Count)
                {
                    return new Point(lines.Last().Length, lines.Count - 1);
                }
                return new Point(Pos - before, i);
            }

            /// <summary>
            /// Given a list of strings representing a series of lines determine the cursor position for the end of the line this cursor position exists on.
            /// </summary>
            /// <param name="lines">The lines making up the block of text</param>
            /// <returns>The position of the end of the line this position is on</returns>
            public CP End(IList<string> lines)
            {
                var uv = GetUV(lines);
                int before = lines.Take(uv.Y).Select(l => l.Length).Concat(0.Only()).Sum();
                if (lines[uv.Y].EndsWith("\n", StringComparison.Ordinal))
                    return new CP(before + lines[uv.Y].Length - 1);
                else
                    return new CP(before + lines[uv.Y].Length, true);
            }
        }

        public void SetCursorPosition(int position)
        {
            CursorPos = new CP(position);
        }

        private CP CursorPos
        {
            get { return m_state.CursorPos; }
            set
            {
                if (value.Pos < 0)
                    value.Pos = 0;
                if (value.Pos > Text.Length)
                    value.Pos = Text.Length;
                m_state.CursorPos = value;

                UpdateDropdown();

                if (m_autoCompleteRange != null)
                {
                    if (value.Pos < m_autoCompleteRange.Item1.Pos || value.Pos > m_autoCompleteRange.Item2.Pos - 1)
                        m_autoCompleteRange = null;
                }

                MoveCaret();
                Redraw();
            }
        }

        private int GetCursorPosInt()
        {
            return CursorPos.Pos;
        }

        public int SelectionLength { get { return m_state.SelectionLength; } set { m_state.SelectionLength = value; } }

        UndoQueue m_undoQueue = new UndoQueue("Textbox");
        UndoAction m_additionUndoAction = null;
        private UndoAction MakeUndoAction()
        {
            var result = new UndoAction(m_state, s => { Text = s.Text; CursorPos = s.CursorPos; SelectionLength = s.SelectionLength; });
            m_undoQueue.Queue(result);
            return result;
        }

        public event Action<string> TextChanged;

        public string SelectedText => Text.Substring(SelectionStart, Math.Abs(SelectionLength));

        public Font Font
        {
            get; set;
        } = SystemFonts.MessageBoxFont;
        private Func<RectangleF> m_area;
        public RectangleF Area => m_area();
        private void UpdateRequestedArea()
        {
            RectangleF size = GetTextBounds();
            size.Inflate(Margin, Margin);
            RequestedArea = new SizeF(Area.Width, size.Height);
        }

        public override event Action RequestedAreaChanged;
        private SizeF m_requestedSize;
        public SizeF RequestedArea
        {
            get { return m_requestedSize; }
            private set
            {
                if (m_requestedSize != value)
                {
                    m_requestedSize = value;
                    RequestedAreaChanged.Execute();
                }
            }
        }

        private Control m_control;
        private Caret m_caret;

        const int CARET_HEIGHT = 13;

        public RectangleF TextRectangle => RectangleF.Inflate(Area, -Margin, -Margin);

        public InputFormEnum InputForm { get; set; }

        public enum InputFormEnum { Text, Decimal, Integer, Path, FileName, None }

        #region DropDown
        Func<string, IEnumerable<string>> AutoCompleteSuggestions;
        ToolStripDropDown m_dropDown;
        private bool m_dropDownOpen = false;
        int m_dropDownWindow = 0;
        const int WINDOW_SIZE = 4;
        string[] m_suggestions = new string[] { "shane", "tammy", "shane again" };
        int m_itemIndex = -1;
        private int ItemIndex
        {
            get { return m_itemIndex; }
            set
            {
                //Debug.WriteLine("Index changed to " + value + " m_dropDownWindow: " + m_dropDownWindow);
                if (SelectedToolStripItem != null)
                    SelectedToolStripItem.BackColor = m_dropDown.BackColor;
                if (value >= 0 && value < m_suggestions.Length)
                    m_itemIndex = value;

                int newDropDownWindow = m_itemIndex - WINDOW_SIZE + 1;
                if (newDropDownWindow > m_dropDownWindow)
                {
                    m_dropDownWindow = newDropDownWindow;
                    UpdateDropdown();
                }
                if (m_itemIndex < m_dropDownWindow)
                {
                    m_dropDownWindow = m_itemIndex;
                    UpdateDropdown();
                }

                if (SelectedToolStripItem != null)
                {
                    //TODO: this seems... odd. Should this be using a ColorScheme?
                    //This definitely does show up as green when you select an item from the autocomplete suggestions...
                    SelectedToolStripItem.BackColor = Color.Green;
                }
            }
        }
        private ToolStripItem SelectedToolStripItem
        {
            get
            {
                if (ItemIndex - m_dropDownWindow < 0)
                    return null;
                if (ItemIndex - m_dropDownWindow >= m_dropDown.Items.Count - 2)
                    return null;
                return m_dropDown.Items[ItemIndex - m_dropDownWindow + 1];
            }
        }
        private void UpdateDropdown()
        {
            if (m_dropDown == null)
                return;

            var tokens = FindTokens().ToArray();
            var matchingToken = tokens.Where(t => t.Item1.Pos < GetCursorPosInt() && (t.Item2.Pos > GetCursorPosInt() || (Text[t.Item2.Pos - 1] != ']' && t.Item2.Pos == Text.Length))).FirstOrDefault();

            if (matchingToken != null)
            {
                if (!m_dropDownOpen)
                {
                    m_itemIndex = -1;
                    m_dropDownWindow = 0;
                }

                m_suggestions = AutoCompleteSuggestions(Text.Substring(matchingToken.Item1.Pos + 1, GetCursorPosInt() - matchingToken.Item1.Pos - 1)).ToArray();
            }
            else
            {
                CloseDropdown();
                return;
            }

            m_dropDown.SuspendLayout();
            var width = (int)Area.Width - 2;
            var height = m_dropDown.Font.Height + 4;
            m_dropDown.Items.Clear();
            m_dropDown.Items.Add(new UpArrowItem(width, () => { m_dropDownWindow--; UpdateDropdown(); }, true));
            if (m_dropDownWindow + WINDOW_SIZE > m_suggestions.Length)
                m_dropDownWindow = m_suggestions.Length - WINDOW_SIZE;
            if (m_dropDownWindow < 0)
                m_dropDownWindow = 0;
            var limited = m_suggestions.Skip(m_dropDownWindow).TakeUpTo(WINDOW_SIZE);
            foreach (var item in limited)
            {
                var itemitem = item;
                var element = m_dropDown.Items.Add(item, null, (a, b) => { AutoCompleteTo(itemitem); });
                element.TextAlign = ContentAlignment.MiddleLeft;
                element.AutoSize = false;
                element.Height = height;
                element.Width = width;
            }
            m_dropDown.Items.Add(new UpArrowItem(width, () => { m_dropDownWindow++; UpdateDropdown(); }, false));
            m_dropDown.AutoClose = false;

            if (m_dropDownWindow <= 0)
                m_dropDown.Items[0].Visible = false;
            if (m_dropDownWindow >= m_suggestions.Length - WINDOW_SIZE)
                m_dropDown.Items.OfType<ToolStripItem>().Last().Visible = false;

            m_dropDown.ResumeLayout();

            if (!m_dropDownOpen)
                m_dropDown.Show(m_control.PointToScreen(new Point((int)Area.Left, (int)Area.Bottom)));
        }

        public System.Windows.Forms.ToolStripRenderer Renderer { get { return m_dropDown.Renderer; } set { m_dropDown.Renderer = value; } }
        private void CloseDropdown()
        {
            m_dropDown?.Close();
        }

        private void AutoCompleteTo(string item)
        {
            var tokens = FindTokens().ToArray();
            var matchingToken = tokens.Where(t => t.Item1.Pos < GetCursorPosInt() && (t.Item2.Pos > GetCursorPosInt() || (Text[t.Item2.Pos - 1] != ']' && t.Item2.Pos == Text.Length))).FirstOrDefault();

            Contract.Assert(matchingToken != null, "attempting string autocompletion when there is nothing to complete");

            InsertText("[" + item + "]", matchingToken.Item1.Pos, matchingToken.Item2.Pos - matchingToken.Item1.Pos);
        }

        #endregion

        public MyTextBox(Control control, Func<RectangleF> area, InputFormEnum inputForm, Func<string, IEnumerable<string>> autoCompleteSuggestions, Func<MyTextBox, IBorderDrawer> borderDrawer, int margin = 4)
        {
            if (autoCompleteSuggestions != null)
            {
                m_dropDown = new ToolStripDropDown();
                m_dropDown.Opened += (a, b) => { m_dropDownOpen = true; };
                m_dropDown.Closed += (a, b) => { m_dropDownOpen = false; m_itemIndex = -1; m_dropDownWindow = 0; };
            }

            m_area = area;
            m_requestedSize = SizeF.Empty;
            m_control = control;
            Colors = new ColorOptions();
            CursorPos = new CP(0);
            SelectionLength = 0;
            control.SizeChanged += (a, b) => UpdateRequestedArea();
            InputForm = inputForm;
            AutoCompleteSuggestions = autoCompleteSuggestions;

            m_disposeActions.Add(Mouse.MouseUp.Register(this, (me, point) => me.GlobalMouseUp()));

            Border = borderDrawer(this);
            Margin = margin;
        }

        int Margin { get; }

        List<Action> m_disposeActions = new List<Action>();

        public void Redraw()
        {
            m_control.Invalidate(true);
        }

        public int SelectionStart { get { var a = GetCursorPosInt(); return Math.Min(a, a + SelectionLength).Clamp(0, Text.Length); } }
        public int SelectionEnd { get { var a = GetCursorPosInt(); return Math.Max(a, a + SelectionLength).Clamp(0, Text.Length); } }

        #region Text Measurement

        private IEnumerable<string> GetLines()
        {
            if (TextRectangle.Width <= 0)
                return Enumerable.Empty<string>();
            return StringUtil.GetLines(Text, Font, TextRectangle.Width, TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding);
        }

        private RectangleF GetTextBounds()
        {
            var lines = GetLines().ToList();
            return new RectangleF(0, 0, lines.Select(l => MeasureText(l)).Concat(0.Only()).Max(), lines.Count * Font.Height);
        }

        private int MeasureText(string text)
        {
            return TextRenderer.MeasureText(text + ".", Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width
                 - TextRenderer.MeasureText(".", Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;
        }

        public PointF CursorToXY(Point characterPos)
        {
            float y = (characterPos.Y + 0.5f) * Font.Height + TextRectangle.Top;
            var lines = GetLines().ToList();
            float x = TextRectangle.Left;
            if (characterPos.X != 0)
                x += MeasureText(lines[characterPos.Y].Substring(0, characterPos.X));
            return new PointF(x, y);
        }

        private CP XYToCursor(float x, float y)
        {
            if (Text.Length == 0)
                return new CP(0);

            x -= TextRectangle.Left;
            y -= TextRectangle.Top;

            int v = (int)Math.Floor(y / Font.Height);
            if (v < 0)
                return new CP(0);
            var lines = GetLines().ToList();
            if (v > lines.Count - 1)
                return new CP(int.MaxValue);
            var line = lines[v];
            var before = lines.Take(v).Sum(l => l.Length);
            int u = 0;
            while (u < line.Length && line[u] != '\n' && MeasureText(line.Substring(0, u)) < x)
                u++;
            return new CP(before + u, u == line.Length && x > 0);
        }

        #endregion

        Tuple<CP, CP> m_autoCompleteRange = null;

        private IEnumerable<Tuple<CP, CP>> FindTokens()
        {
            int openIndex = int.MaxValue;
            for (int i = 0; i < Text.Length; i++)
            {
                if (Text[i] == '[')
                {
                    if (openIndex == int.MaxValue)
                        openIndex = Math.Min(i, openIndex);
                }
                else if (Text[i] == ']' && openIndex < int.MaxValue)
                {
                    yield return Tuple.Create(new CP(openIndex, false), new CP(i + 1, true));
                    openIndex = int.MaxValue;
                }
            }
            if (openIndex < int.MaxValue)
                yield return Tuple.Create(new CP(openIndex, false), new CP(Text.Length, true));
        }

        public override void Paint(Graphics g)
        {
            var area = Area;
            Border.Draw(g, area.Round());
            DrawText(g, TextRectangle);
        }

        private void DrawText(Graphics g, RectangleF area)
        {
            using (g.SaveState())
            {
                g.Clip = new Region(TextRectangle);
                var lines = GetLines().ToList();
                for (int line = 0; line < lines.Count; line++)
                {
                    var text = lines[line];
                    for (int i = 0; i <= text.Length; i++)
                    {
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                        TextRenderer.DrawText(g, text.Substring(0, i), Font, TextRectangle.Location.Round().Plus(0, Font.Height * line), Colors.Text, Border.BackColor, TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    }
                }

                foreach (var token in FindTokens())
                {
                    RenderOverText(g, area, lines, Colors.TokenText, Colors.TokenBackground, token.Item1, token.Item2);
                }

                if (Math.Abs(SelectionLength) > 0)
                {
                    RenderOverText(g, area, lines, Colors.SelectedText, Colors.SelectedBackground, CursorPos, new CP(CursorPos.Pos + SelectionLength));
                }

                if (m_autoCompleteRange != null)
                {
                    RenderOverText(g, area, lines, Colors.AutoCompleteText, Colors.AutoCompleteBackground, m_autoCompleteRange.Item1, m_autoCompleteRange.Item2);
                }
            }
        }

        private void RenderOverText(Graphics g, RectangleF area, List<string> lines, Color textColor, Color backgroundColor, CP start, CP end)
        {
            Point p1 = start.GetUV(lines);
            Point p2 = end.GetUV(lines);

            if (p1.Y > p2.Y || (p1.Y == p2.Y && p1.X > p2.X))
                Util.Swap(ref p1, ref p2);

            for (int y = p1.Y; y <= p2.Y; y++)
            {
                int minx = p1.Y == y ? p1.X : 0;
                int maxx = p2.Y == y ? p2.X : lines[y].Length;

                float startx = CursorToXY(new Point(minx, y)).X;
                float endx = CursorToXY(new Point(maxx, y)).X;
                using (g.SaveState())
                {
                    Region r = new Region(RectangleF.FromLTRB(startx, y * Font.Height + TextRectangle.Y, endx, (1 + y) * Font.Height + TextRectangle.Y));
                    g.Clip = r;
                    g.FillRectangle(Colors.SelectedBackgroundBrush, area);
                    TextRenderer.DrawText(g, lines[y], Font, TextRectangle.Location.Round().Plus(0, Font.Height * y), textColor, backgroundColor, TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping);
                }
            }
        }

        bool m_selecting = false;

        public override void MouseDown(MouseEventArgs args)
        {
            if (Area.Contains(args.Location))
            {
                CursorPos = XYToCursor(args.X, args.Y);
                SelectionLength = 0;
                m_selecting = true;
            }
        }

        private void GlobalMouseUp()
        {
            m_selecting = false;
        }

        public override void MouseUp(MouseEventArgs args) { }

        public override void MouseMove(MouseEventArgs args)
        {
            if (m_selecting)
            {
                var selectionEnd = GetCursorPosInt() + SelectionLength;
                CursorPos = XYToCursor(args.X, args.Y);
                SelectionLength = selectionEnd - GetCursorPosInt();
                Redraw();
            }
        }

        public override void MouseLeave() { }

        public override void MouseClick(System.Windows.Forms.MouseEventArgs args)
        {
            if (Area.Contains(args.Location))
            {
                if (args.Button == MouseButtons.Left)
                {
                    CursorPos = XYToCursor(args.X, args.Y);
                }
            }
        }

        private void MoveCaret()
        {
            //if (InputForm != InputFormEnum.None)
            {
                var pos = CursorToXY(CursorPos.GetUV(GetLines().ToList()));
                if (m_caret != null)
                    m_caret.MoveTo((int)(pos.X), (int)pos.Y - Font.Height / 2);
            }
        }

        public void DeleteSelection()
        {
            if (Math.Abs(SelectionLength) > 0)
            {
                Text = Text.Remove(SelectionStart, Math.Abs(SelectionLength));
                CursorPos = new CP(SelectionStart);
                SelectionLength = 0;
                Redraw();
            }
        }

        public override void KeyPress(KeyPressEventArgs args)
        {
            bool isEnter = args.KeyChar == '\n' || args.KeyChar == '\r';
            bool canEnter = m_dropDownOpen || ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) || !SpecialEnter;
            if (!char.IsControl(args.KeyChar) || (isEnter && canEnter))
            {
                if (isEnter && m_dropDownOpen)
                {
                    if (SelectedToolStripItem != null)
                    {
                        SelectedToolStripItem.PerformClick();
                    }
                    m_dropDown.Close();
                }
                else
                {
                    string newText = Text.Remove(SelectionStart, Math.Abs(SelectionLength)).Insert(SelectionStart, args.KeyChar + "");

                    bool acceptable = InputForm == MyTextBox.InputFormEnum.Text ||
                                      InputForm == MyTextBox.InputFormEnum.Decimal && decimal.TryParse(newText, NumberStyles.Number, CultureInfo.CurrentCulture, out var _) ||
                                      InputForm == MyTextBox.InputFormEnum.Integer && int.TryParse(newText, NumberStyles.Number, CultureInfo.CurrentCulture, out var _) ||
                                      InputForm == MyTextBox.InputFormEnum.Decimal && decimal.TryParse(newText + "0", NumberStyles.Number, CultureInfo.CurrentCulture, out var _) || //To catch "-" and "+"
                                      InputForm == MyTextBox.InputFormEnum.Integer && int.TryParse(newText + "0", NumberStyles.Number, CultureInfo.CurrentCulture, out var _) || //To catch "-" and "+"
                                      InputForm == MyTextBox.InputFormEnum.Path && StringUtil.IsAcceptablePathChar(args.KeyChar) ||
                                      InputForm == MyTextBox.InputFormEnum.FileName && IsAcceptableFileNameChar(args.KeyChar);

                    if (acceptable)
                    {
                        string textToInsert = "";
                        if (!char.IsControl(args.KeyChar))
                            textToInsert = args.KeyChar.ToString();
                        else if (args.KeyChar == '\r')
                            textToInsert = "\n"; //Currently newlines are considered like any other text in terms of resetting undo state

                        InsertText(textToInsert, GetCursorPosInt(), SelectionLength);
                    }
                }
            }
        }

        private void InsertText(string textToInsert, int from, int length)
        {
            if (Math.Abs(SelectionLength) > 0 || m_additionUndoAction == null) //If we've got text selected or we're not already adding text the replacement of that text becomes the start of a new addition
                m_additionUndoAction = MakeUndoAction();

            CursorPos = new CP(from);
            SelectionLength = length;


            if (length != 0)
            {
                DeleteSelection();
            }
            if (textToInsert != null)
            {
                Text = Text.Insert(GetCursorPosInt(), textToInsert); //Currently newlines are considered like any other text in terms of resetting undo state
                CursorPos = new CP(CursorPos.Pos + textToInsert.Length);
            }

            m_additionUndoAction.SetRedo(m_state);
        }

        private static bool IsAcceptableFileNameChar(char c)
        {
            return !Path.GetInvalidFileNameChars().Contains(c);
        }

        public override void GotFocus()
        {
            if (InputForm != InputFormEnum.None)
            {
                m_caret = new Caret(m_control, 2, CARET_HEIGHT);
                MoveCaret();
                m_caret.Show();
            }
        }

        public override void LostFocus()
        {
            ClearCaret();
            CloseDropdown();
        }

        private void ClearCaret()
        {
            if (m_caret != null)
            {
                m_caret.Dispose();
                m_caret = null;
            }
        }

        public bool SpecialEnter { get; set; }
        public event Action EnterPressed;

        public override void KeyDown(KeyEventArgs args)
        {
            if (args.KeyCode.IsSet(Keys.A) && args.Control)
            {
                SelectAll();
            }
            else if (args.KeyCode.IsSet(Keys.C) && args.Control)
            {
                Copy();
            }
            else if (args.KeyCode.IsSet(Keys.X) && args.Control)
            {
                Cut();
            }
            else if (args.KeyCode.IsSet(Keys.V) && args.Control)
            {
                Paste();
            }
            else if (args.KeyCode.IsSet(Keys.Z) && args.Control)
            {
                Undo();
            }
            else if (args.KeyCode.IsSet(Keys.Y) && args.Control)
            {
                Redo();
            }
            else if (args.KeyCode.IsSet(Keys.Right))
            {
                MoveRight(args);
            }
            else if (args.KeyCode.IsSet(Keys.Left))
            {
                MoveLeft(args);
            }
            else if (args.KeyCode.IsSet(Keys.Up))
            {
                MoveUp(args);
            }
            else if (args.KeyCode.IsSet(Keys.Down))
            {
                MoveDown(args);
            }
            else if (args.KeyCode.IsSet(Keys.Delete))
            {
                Delete(args);
            }
            else if (args.KeyCode.IsSet(Keys.Back))
            {
                Backspace(args);
            }
            else if (args.KeyCode == Keys.Home)
            {
                Home(args);
            }
            else if (args.KeyCode == Keys.End)
            {
                End(args);
            }
            else if (args.KeyCode.IsSet(Keys.Enter))
            {
                if (SpecialEnter && !args.Shift && !m_dropDownOpen)
                {
                    args.Handled = true;
                    args.SuppressKeyPress = true;
                    EnterPressed.Execute();
                }
            }
        }

        private void End(KeyEventArgs args)
        {
            if (args.Shift)
            {
                var start = GetCursorPosInt() + SelectionLength;
                var lines = GetLines().ToList();
                var pos = CursorPos.End(lines);
                SelectionLength = start - pos.Pos;
            }
            else
            {
                SelectionLength = 0;
            }

            if (args.Control)
            {
                CursorPos = new CP(int.MaxValue);
            }
            else
            {
                CursorPos = CursorPos.End(GetLines().ToList());
            }
            m_additionUndoAction = null;
        }

        private void Home(KeyEventArgs args)
        {
            if (args.Shift)
            {
                var end = GetCursorPosInt() + SelectionLength;
                var lines = GetLines().ToList();
                var uv = CursorPos.GetUV(lines);
                CursorPos = new CP(lines.Take(uv.Y).Select(l => l.Length).Concat(0.Only()).Sum());
                SelectionLength = end - CursorPos.Pos;
            }
            else
            {
                SelectionLength = 0;
            }

            if (args.Control)
            {
                CursorPos = new CP(0);
            }
            else
            {
                var lines = GetLines().ToList();
                var uv = CursorPos.GetUV(lines);
                CursorPos = new CP(lines.Take(uv.Y).Select(l => l.Length).Concat(0.Only()).Sum());
            }

            m_additionUndoAction = null;
        }

        private void Backspace(KeyEventArgs args)
        {
            if (InputForm != InputFormEnum.None)
            {
                var undo = MakeUndoAction();
                if (SelectionLength != 0)
                    DeleteSelection();
                else if (GetCursorPosInt() > 0)
                {
                    if (args.Control)
                    {
                        Regex r = new Regex(@"\S*\s*$");
                        string before = Text.Substring(0, GetCursorPosInt());
                        var match = r.Match(before);
                        CursorPos = new CP(match.Index);
                        SelectionLength = match.Length;
                        DeleteSelection();
                    }
                    else
                    {
                        Text = Text.Remove(GetCursorPosInt() - 1, 1);
                        var lines = GetLines().ToList();
                        CursorPos = new CP(Math.Max(0, CursorPos.Pos - 1), CursorPos.GetUV(lines).X == 0);
                    }
                    Redraw();
                }
                m_additionUndoAction = null;
                undo.SetRedo(m_state);
            }
        }

        private void Delete(KeyEventArgs args)
        {
            if (InputForm != InputFormEnum.None)
            {
                var undo = MakeUndoAction();
                if (SelectionLength != 0)
                    DeleteSelection();
                else if (GetCursorPosInt() < Text.Length)
                {
                    if (args.Control)
                    {
                        Regex r = new Regex(@"\s*\S*");
                        SelectionLength = r.Match(Text, GetCursorPosInt()).Length;
                        DeleteSelection();
                    }
                    else
                    {
                        Text = Text.Remove(GetCursorPosInt(), 1);
                    }
                    Redraw();
                }
                m_additionUndoAction = null;
                undo.SetRedo(m_state);
            }
        }

        private void MoveDown(KeyEventArgs args)
        {
            if (m_dropDownOpen)
                ItemIndex++;
            else
            {
                if (args.Shift)
                {
                    var selectionEnd = GetCursorPosInt() + SelectionLength;
                    var point = CursorToXY(CursorPos.GetUV(GetLines().ToList()));
                    point.Y += Font.Height;
                    CursorPos = XYToCursor(point.X, point.Y);
                    SelectionLength = selectionEnd - GetCursorPosInt();
                }
                else
                {
                    SelectionLength = 0;
                    var point = CursorToXY(CursorPos.GetUV(GetLines().ToList()));
                    point.Y += Font.Height;
                    CursorPos = XYToCursor(point.X, point.Y);
                }
                Redraw();
                m_additionUndoAction = null;
            }
        }

        private void MoveUp(KeyEventArgs args)
        {
            if (m_dropDownOpen)
                ItemIndex--;
            else
            {
                if (args.Shift)
                {
                    var selectionEnd = GetCursorPosInt() + SelectionLength;
                    var point = CursorToXY(CursorPos.GetUV(GetLines().ToList()));
                    point.Y -= Font.Height;
                    CursorPos = XYToCursor(point.X, point.Y);
                    SelectionLength = selectionEnd - GetCursorPosInt();
                }
                else
                {
                    SelectionLength = 0;
                    var point = CursorToXY(CursorPos.GetUV(GetLines().ToList()));
                    point.Y -= Font.Height;
                    CursorPos = XYToCursor(point.X, point.Y);
                }
                Redraw();
                m_additionUndoAction = null;
            }
        }

        private void MoveLeft(KeyEventArgs args)
        {
            var oldPos = GetCursorPosInt();
            int nextPos = Math.Max(0, GetCursorPosInt() - 1);
            if (args.Control)
            {
                Regex r = new Regex(@"\S*\s*$");
                string before = Text.Substring(0, GetCursorPosInt());
                nextPos = r.Match(before).Index;
            }
            CursorPos = new CP(nextPos);

            if (args.Shift)
            {
                if (SelectionLength <= 0)
                    SelectionLength -= GetCursorPosInt() - oldPos;
                else
                    SelectionLength -= GetCursorPosInt() - oldPos;
            }
            else
            {
                SelectionLength = 0;
            }
            Redraw();
            m_additionUndoAction = null;
        }

        private void MoveRight(KeyEventArgs args)
        {
            var oldPos = GetCursorPosInt();
            int nextPos = 1;
            if (args.Control)
            {
                Regex r = new Regex(@"\s*\S*");
                nextPos = r.Match(Text, GetCursorPosInt()).Length;
            }
            CursorPos = new CP(CursorPos.Pos + nextPos);

            if (args.Shift)
            {
                if (SelectionLength <= 0)
                    SelectionLength -= GetCursorPosInt() - oldPos;
                else
                    SelectionLength -= GetCursorPosInt() - oldPos;
            }
            else
            {
                SelectionLength = 0;
            }
            Redraw();
            m_additionUndoAction = null;
        }

        private void Redo()
        {
            m_undoQueue.Redo();
            m_additionUndoAction = null;
        }

        private void Undo()
        {
            m_undoQueue.Undo();
            m_additionUndoAction = null;
        }

        private void Paste()
        {
            if (Clipboard.ContainsText() && InputForm != InputFormEnum.None)
            {
                var undo = MakeUndoAction();
                if (SelectionLength != 0)
                    DeleteSelection();
                var insertText = Clipboard.GetText();
                Text = Text.Insert(GetCursorPosInt(), insertText);
                CursorPos = new CP(CursorPos.Pos + insertText.Length);
                m_additionUndoAction = null;
                undo.SetRedo(m_state);
            }
        }

        private void Cut()
        {
            if (InputForm != InputFormEnum.None)
            {
                if (!string.IsNullOrEmpty(SelectedText))
                {
                    var undo = MakeUndoAction();
                    if (!string.IsNullOrEmpty(SelectedText))
                        Clipboard.SetText(SelectedText);
                    DeleteSelection();
                    m_additionUndoAction = null;
                    undo.SetRedo(m_state);
                }
            }
        }

        private void Copy()
        {
            if (!string.IsNullOrEmpty(SelectedText))
                Clipboard.SetText(SelectedText);
            m_additionUndoAction = null;
        }

        private void SelectAll()
        {
            SelectionLength = -Text.Length;
            CursorPos = new CP(int.MaxValue);
            m_additionUndoAction = null;
        }

        public static void SetupCallbacks(Control control, MyTextBox textBox)
        {
            MouseEventHandler MouseDown = (a, args) => textBox.MouseDown(args);
            MouseEventHandler MouseUp = (a, args) => textBox.MouseUp(args);
            MouseEventHandler MouseMove = (a, args) => textBox.MouseMove(args);
            MouseEventHandler MouseClick = (a, args) => textBox.MouseClick(args);
            KeyPressEventHandler KeyPress = (a, args) => textBox.KeyPress(args);
            KeyEventHandler KeyDown = (a, args) => textBox.KeyDown(args);
            PaintEventHandler Paint = (a, args) => textBox.Paint(args.Graphics);
            EventHandler GotFocus = (a, args) => textBox.GotFocus();
            EventHandler LostFocus = (a, args) => textBox.LostFocus();

            control.MouseDown += MouseDown;
            control.MouseUp += MouseUp;
            control.MouseMove += MouseMove;
            control.MouseClick += MouseClick;
            control.KeyPress += KeyPress;
            control.KeyDown += KeyDown;
            control.Paint += Paint;
            control.GotFocus += GotFocus;
            control.LostFocus += LostFocus;

            textBox.PushDisposeActions(() =>
            {
                control.MouseDown -= MouseDown;
                control.MouseUp -= MouseUp;
                control.MouseMove -= MouseMove;
                control.MouseClick -= MouseClick;
                control.KeyPress -= KeyPress;
                control.KeyDown -= KeyDown;
                control.Paint -= Paint;
                control.GotFocus -= GotFocus;
                control.LostFocus -= LostFocus;
            });
        }

        public override void MouseWheel(MouseEventArgs args)
        {
        }

        public override void MouseCaptureChanged()
        {
        }

        public override bool Contains(PointF point)
        {
            return Area.Contains(point);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (m_dropDown != null)
                    m_dropDown.Dispose();
                foreach (var action in m_disposeActions)
                    action.Execute();
            }
            ClearCaret();
        }
    }
}

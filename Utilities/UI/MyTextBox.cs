using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.IO;

namespace Utilities
{
    public class MyTextBox : MyControl, IDisposable
    {
        public const uint BORDER_SIZE = 4;
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

            public override string Description
            {
                get { return "Edited text box"; }
            }
        }

        private struct State
        {
            public string Text;
            public CP CursorPos;
            public int SelectionLength;
        }

        public class ColorOptions
        {
            public ColorOptions()
            {
                Text = Color.FromArgb(205, 205, 205);
                SelectedText = Color.Cyan;
                SelectedBackgroundBrush = Brushes.Navy;
                Background = Color.FromArgb(56, 56, 56);
                BorderPen = new Pen(Color.FromArgb(205, 205, 205));
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
                    throw new Exception("Textbox Text cannot be null");
                if (m_state.Text != value || m_uninitializedText)
                {
                    m_uninitializedText = false;
                    var oldText = m_state.Text;
                    m_state.Text = value;
                    TextChanged.Execute(oldText);
                    UpdateRequestedArea();
                    this.Redraw();
                }
            }
        }

        public struct CP
        {
            public int Pos;
            public bool PreferEnd; //Put the cursor at the end of line i instead of the start of line i+1
            public CP(int pos, bool preferEnd = false) { Pos = pos; PreferEnd = preferEnd; }

            public Point GetUV(List<string> lines)
            {
                int before = 0;
                int i = 0;
                while (i < lines.Count && lines[i].Length + before + (PreferEnd ? 1 : 0) <= Pos)
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

            public CP End(List<string> lines)
            {
                var uv = GetUV(lines);
                int before = lines.Take(uv.Y).Select(l => l.Length).Concat(0.Only()).Sum();
                if (lines[uv.Y].EndsWith("\n"))
                    return new CP(before + lines[uv.Y].Length - 1);
                else
                    return new CP(before + lines[uv.Y].Length, true);
            }
        }

        public CP CursorPos
        {
            get { return m_state.CursorPos; }
            set
            {
                if (value.Pos < 0)
                    value.Pos = 0;
                if (value.Pos > Text.Length)
                    value.Pos = Text.Length;
                m_state.CursorPos = value;
                using (var g = m_control.CreateGraphics())
                    MoveCaret(g);
                Redraw();
            }
        }

        private int GetCursorPosInt()
        {
            return CursorPos.Pos;
        }

        public int SelectionLength { get { return m_state.SelectionLength; } set { m_state.SelectionLength = value; } }

        UndoQueue m_undoQueue = new UndoQueue();
        UndoAction m_additionUndoAction = null;
        private UndoAction MakeUndoAction()
        {
            var result = new UndoAction(m_state, s => { Text = s.Text; CursorPos = s.CursorPos; SelectionLength = s.SelectionLength; });
            m_undoQueue.Queue(result);
            return result;
        }

        public event Action<string> TextChanged;

        public string SelectedText { get { return Text.Substring(SelectionStart, Math.Abs(SelectionLength)); } }

        public Font Font = SystemFonts.MessageBoxFont;
        private Func<RectangleF> m_area;
        public RectangleF Area { get { return m_area(); } }
        private void UpdateRequestedArea()
        {
            using (Graphics g = m_control.CreateGraphics())
            {
                RectangleF size = GetTextBounds(g);
                size.Inflate(BORDER_SIZE, BORDER_SIZE);
                RequestedArea = new SizeF(Area.Width, size.Height);
            }
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

        const int CARET_HEIGHT = 13;

        public RectangleF TextRectangle { get { return RectangleF.Inflate(Area, -BORDER_SIZE, -BORDER_SIZE); } }

        public InputFormEnum InputForm;

        public enum InputFormEnum { Text, Decimal, Integer, Path, FileName, None }

        public MyTextBox(Control control, Func<RectangleF> area, InputFormEnum inputForm)
        {
            m_area = area;
            m_requestedSize = SizeF.Empty;
            m_control = control;
            Colors = new ColorOptions();
            CursorPos = new CP(0);
            SelectionLength = 0;
            control.SizeChanged += (a, b) => UpdateRequestedArea();
            InputForm = inputForm;
        }

        public void Redraw()
        {
            m_control.Invalidate(true);
        }

        public int SelectionStart { get { var a = GetCursorPosInt(); return Math.Min(a, a + SelectionLength).Clamp(0, Text.Length); } }
        public int SelectionEnd { get { var a = GetCursorPosInt(); return Math.Max(a, a + SelectionLength).Clamp(0, Text.Length); } }

        #region Text Measurement

        private IEnumerable<string> GetLines()
        {
            return StringUtil.GetLines(Text, Font, TextRectangle.Width, TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding);
        }

        private RectangleF GetTextBounds(Graphics g)
        {
            var lines = GetLines().ToList();
            return new RectangleF(0, 0, lines.Select(l => MeasureText(l)).Concat(0.Only()).Max(), lines.Count * Font.Height);
        }

        private int MeasureText(string text)
        {
            return TextRenderer.MeasureText(text + ".", Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width
                 - TextRenderer.MeasureText(".", Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix).Width;
        }

        public PointF CursorToXY(Point characterPos, Graphics g)
        {
            float y = (characterPos.Y + 0.5f) * Font.Height + TextRectangle.Top;
            var lines = GetLines().ToList();
            float x = TextRectangle.Left;
            if (characterPos.X != 0)
                x += MeasureText(lines[characterPos.Y].Substring(0, characterPos.X));
            return new PointF(x, y);
        }

        public CP XYToCursor(float x, float y, Graphics g)
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

        public override void Paint(Graphics g)
        {
            var area = Area;
            g.FillRectangle(Colors.BackgroundBrush, area);
            g.DrawRectangle(Colors.BorderPen, new Rectangle((int)area.X, (int)area.Y, (int)Math.Floor(area.Width) - 1, (int)Math.Floor(area.Height) - 1));

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
                        TextRenderer.DrawText(g, text.Substring(0, i), Font, TextRectangle.Location.Round().Plus(0, Font.Height * line), Colors.Text, Colors.Background, TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
                    }
                }

                if (Math.Abs(SelectionLength) > 0)
                {
                    Point p1 = CursorPos.GetUV(lines);
                    Point p2 = new CP(CursorPos.Pos + SelectionLength).GetUV(lines);

                    if (p1.Y > p2.Y || (p1.Y == p2.Y && p1.X > p2.X))
                        Util.Swap(ref p1, ref p2);

                    for (int y = p1.Y; y <= p2.Y; y++)
                    {
                        int minx = p1.Y == y ? p1.X : 0;
                        int maxx = p2.Y == y ? p2.X : lines[y].Length;

                        float startx = CursorToXY(new Point(minx, y), g).X;
                        float endx = CursorToXY(new Point(maxx, y), g).X;
                        using (g.SaveState())
                        {
                            Region r = new Region(RectangleF.FromLTRB(startx, y * Font.Height + TextRectangle.Y, endx, (1 + y) * Font.Height + TextRectangle.Y));
                            g.Clip = r;
                            g.FillRectangle(Colors.SelectedBackgroundBrush, area);
                            TextRenderer.DrawText(g, lines[y], Font, TextRectangle.Location.Round().Plus(0, Font.Height * y), Colors.SelectedText, Colors.SelectedBackground, TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix | TextFormatFlags.PreserveGraphicsClipping);
                        }
                    }
                }
            }
        }

        bool m_selecting = false;

        public override void MouseDown(MouseEventArgs args)
        {
            if (Area.Contains(args.Location))
            {
                using (var g = m_control.CreateGraphics())
                {
                    var loc = args.Location.Take(Area.Location.Round());
                    CursorPos = XYToCursor(args.X, args.Y, g);
                    SelectionLength = 0;
                }
                m_selecting = true;
            }
        }

        public override void MouseUp(MouseEventArgs args)
        {
            m_selecting = false;
        }

        public override void MouseMove(MouseEventArgs args)
        {
            if (m_selecting)
            {
                using (var g = m_control.CreateGraphics())
                {
                    var selectionEnd = GetCursorPosInt() + SelectionLength;
                    CursorPos = XYToCursor(args.X, args.Y, g);
                    SelectionLength = selectionEnd - GetCursorPosInt();
                }
                Redraw();
            }
        }

        public override void MouseClick(System.Windows.Forms.MouseEventArgs args)
        {
            if (m_area().Contains(args.Location))
            {
                if (args.Button == MouseButtons.Left)
                {
                    using (var g = m_control.CreateGraphics())
                    {
                        CursorPos = XYToCursor(args.X, args.Y, g);
                    }
                }
            }
        }

        private void MoveCaret(Graphics g)
        {
            //if (InputForm != InputFormEnum.None)
            {
                var pos = CursorToXY(CursorPos.GetUV(GetLines().ToList()), g);
                Caret.SetCaretPos((int)(pos.X), (int)pos.Y - Font.Height / 2);
            }
        }

        private int PosForChar(float x, float y, RectangleF charBound, int i)
        {
            if (x * 2 < charBound.Left + charBound.Right)
                return i;
            else
                return i + 1;
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
            bool canEnter = ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) || !SpecialEnter;
            if (!char.IsControl(args.KeyChar) || (isEnter && canEnter))
            {
                bool acceptable = InputForm == MyTextBox.InputFormEnum.Text ||
                                  InputForm == MyTextBox.InputFormEnum.Decimal && IsAcceptableNumeric(args.KeyChar) ||
                                  InputForm == MyTextBox.InputFormEnum.Integer && IsAcceptableNumeric(args.KeyChar) ||
                                  InputForm == MyTextBox.InputFormEnum.Path && IsAcceptablePathChar(args.KeyChar) ||
                                  InputForm == MyTextBox.InputFormEnum.FileName && IsAcceptableFileNameChar(args.KeyChar);
                if (acceptable)
                {
                    if (Math.Abs(SelectionLength) > 0 || m_additionUndoAction == null) //If we've got text selected or we're not already adding text the replacement of that text becomes the start of a new addition
                        m_additionUndoAction = MakeUndoAction();

                    DeleteSelection();
                    if (!char.IsControl(args.KeyChar))
                        Text = Text.Insert(GetCursorPosInt(), args.KeyChar.ToString());
                    else if (args.KeyChar == '\r')
                        Text = Text.Insert(GetCursorPosInt(), "\n"); //Currently newlines are considered like any other text in terms of resetting undo state
                    CursorPos = new CP(CursorPos.Pos + 1);
                    m_additionUndoAction.SetRedo(m_state);
                }
            }
        }

        private bool IsAcceptableNumeric(char p)
        {
            if (char.IsNumber(p))
                return true;
            if (InputForm == InputFormEnum.Decimal && p == '.' && !Text.Contains('.'))
                return true;
            if (p == '-' && SelectionStart == 0)
                return true;
            return false;
        }

        private bool IsAcceptablePathChar(char c)
        {
            return !Path.GetInvalidPathChars().Contains(c);
        }

        private bool IsAcceptableFileNameChar(char c)
        {
            return !Path.GetInvalidFileNameChars().Contains(c);
        }

        public override void GotFocus()
        {
            if (InputForm != InputFormEnum.None)
            {
                Caret.CreateCaret(m_control.Handle, IntPtr.Zero, 2, CARET_HEIGHT);
                using (var g = m_control.CreateGraphics())
                    MoveCaret(g);
                Caret.ShowCaret(m_control.Handle);
            }
        }

        public override void LostFocus()
        {
            Caret.DestroyCaret();
        }

        public bool SpecialEnter { get; set; }
        public event Action EnterPressed;

        public override void KeyDown(KeyEventArgs e)
        {
            if (e.KeyCode.IsSet(Keys.A) && e.Control)
            {
                SelectionLength = -Text.Length;
                CursorPos = new CP(int.MaxValue);
                m_additionUndoAction = null;
            }
            else if (e.KeyCode.IsSet(Keys.C) && e.Control)
            {
                if (!string.IsNullOrEmpty(SelectedText))
                    Clipboard.SetText(SelectedText);
                m_additionUndoAction = null;
            }
            else if (e.KeyCode.IsSet(Keys.X) && e.Control)
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
            else if (e.KeyCode.IsSet(Keys.V) && e.Control)
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
            else if (e.KeyCode.IsSet(Keys.Z) && e.Control)
            {
                m_undoQueue.Undo();
                m_additionUndoAction = null;
            }
            else if (e.KeyCode.IsSet(Keys.Y) && e.Control)
            {
                m_undoQueue.Redo();
                m_additionUndoAction = null;
            }
            else if (e.KeyCode.IsSet(Keys.Right))
            {
                var oldPos = GetCursorPosInt();
                int nextPos = 1;
                if (e.Control)
                {
                    Regex r = new Regex(@"\s*\S*");
                    nextPos = r.Match(Text, GetCursorPosInt()).Length;
                }
                CursorPos = new CP(CursorPos.Pos + nextPos);

                if (e.Shift)
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
            else if (e.KeyCode.IsSet(Keys.Left))
            {
                var oldPos = GetCursorPosInt();
                int nextPos = Math.Max(0, GetCursorPosInt() - 1);
                if (e.Control)
                {
                    Regex r = new Regex(@"\S*\s*$");
                    string before = Text.Substring(0, GetCursorPosInt());
                    nextPos = r.Match(before).Index;
                }
                CursorPos = new CP(nextPos);

                if (e.Shift)
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
            else if (e.KeyCode.IsSet(Keys.Up))
            {
                using (Graphics g = m_control.CreateGraphics())
                {
                    if (e.Shift)
                    {
                        var selectionEnd = GetCursorPosInt() + SelectionLength;
                        var point = CursorToXY(CursorPos.GetUV(GetLines().ToList()), g);
                        point.Y -= Font.Height;
                        CursorPos = XYToCursor(point.X, point.Y, g);
                        SelectionLength = selectionEnd - GetCursorPosInt();
                    }
                    else
                    {
                        SelectionLength = 0;
                        var point = CursorToXY(CursorPos.GetUV(GetLines().ToList()), g);
                        point.Y -= Font.Height;
                        CursorPos = XYToCursor(point.X, point.Y, g);
                    }
                    Redraw();
                }
                m_additionUndoAction = null;
            }
            else if (e.KeyCode.IsSet(Keys.Down))
            {
                using (Graphics g = m_control.CreateGraphics())
                {
                    if (e.Shift)
                    {
                        var selectionEnd = GetCursorPosInt() + SelectionLength;
                        var point = CursorToXY(CursorPos.GetUV(GetLines().ToList()), g);
                        point.Y += Font.Height;
                        CursorPos = XYToCursor(point.X, point.Y, g);
                        SelectionLength = selectionEnd - GetCursorPosInt();
                    }
                    else
                    {
                        SelectionLength = 0;
                        var point = CursorToXY(CursorPos.GetUV(GetLines().ToList()), g);
                        point.Y += Font.Height;
                        CursorPos = XYToCursor(point.X, point.Y, g);
                    }
                    Redraw();
                }
                m_additionUndoAction = null;
            }
            else if (e.KeyCode.IsSet(Keys.Delete))
            {
                if (InputForm != InputFormEnum.None)
                {
                    var undo = MakeUndoAction();
                    if (SelectionLength != 0)
                        DeleteSelection();
                    else if (GetCursorPosInt() < Text.Length)
                    {
                        if (e.Control)
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
            else if (e.KeyCode.IsSet(Keys.Back))
            {
                if (InputForm != InputFormEnum.None)
                {
                    var undo = MakeUndoAction();
                    if (SelectionLength != 0)
                        DeleteSelection();
                    else if (GetCursorPosInt() > 0)
                    {
                        if (e.Control)
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
            else if (e.KeyCode == Keys.Home)
            {
                if (e.Shift)
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

                if (e.Control)
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
            else if (e.KeyCode == Keys.End)
            {
                if (e.Shift)
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

                if (e.Control)
                {
                    CursorPos = new CP(int.MaxValue);
                }
                else
                {
                    CursorPos = CursorPos.End(GetLines().ToList());
                }
                m_additionUndoAction = null;
            }
            else if (e.KeyCode.IsSet(Keys.Enter))
            {
                if (SpecialEnter && !e.Shift)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    EnterPressed.Execute();
                }
            }
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

            textBox.m_disposeActions.Push(() =>
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

        public override void Dispose()
        {
            base.Dispose();
            Caret.DestroyCaret();
        }
    }
}

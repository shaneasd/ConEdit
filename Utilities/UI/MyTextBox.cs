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
            public int CursorPos;
            public int SelectionLength;
        }

        public class ColorOptions
        {
            public ColorOptions()
            {
                Text = Color.FromArgb(205, 205, 205);
                SelectedText = Color.Cyan;
                SelectedBackgroundBrush = Brushes.Navy;
                BackgroundBrush = new SolidBrush(Color.FromArgb(56, 56, 56));
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

            public Brush BackgroundBrush { get; set; }
            public Pen BorderPen { get; set; }
            public Brush SelectedBackgroundBrush { get; set; }

            public Brush TextBrush { get; protected set; }
            public Brush SelectedTextBrush { get; protected set; }
        }

        public ColorOptions Colors { get; set; }

        State m_state = new State { Text = "", CursorPos = 0, SelectionLength = 0 };

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
        public int CursorPos
        {
            get { return m_state.CursorPos; }
            set
            {
                m_state.CursorPos = value.Clamp(0, Text.Length);
                using (var g = m_control.CreateGraphics())
                    MoveCaret(g);
                Redraw();
            }
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

        //private string MeasureText { get { return Text.EndsWith("\n") ? Text + " " : Text; } }
        private string MeasureText { get { return Text + "."; } } //Doesn't seem to handle trailing whitespace well
        public string SelectedText { get { return Text.Substring(SelectionStart, Math.Abs(SelectionLength)); } }

        public Font Font = SystemFonts.MessageBoxFont;
        private Func<RectangleF> m_area;
        public RectangleF Area { get { return m_area(); } }
        private void UpdateRequestedArea()
        {
            using (Graphics g = m_control.CreateGraphics())
            {
                var measureText = MeasureText.Length != 0 ? MeasureText : "|";
                Format.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, measureText.Length) });
                var size = g.MeasureCharacterRanges(measureText, Font, new RectangleF(0, 0, TextRectangle.Width, int.MaxValue), Format)[0].GetBounds(g);
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

        //public StringFormat Format = new StringFormat(StringFormat.GenericTypographic);
        public StringFormat Format = new StringFormat(StringFormat.GenericDefault) { FormatFlags = StringFormatFlags.MeasureTrailingSpaces, Trimming = StringTrimming.None };

        public readonly InputFormEnum InputForm;

        public enum InputFormEnum { Text, Decimal, Integer, Path, FileName, None }

        public MyTextBox(Control control, Func<RectangleF> area, InputFormEnum inputForm)
        {
            m_area = area;
            m_requestedSize = SizeF.Empty;
            m_control = control;
            Colors = new ColorOptions();
            CursorPos = 0;
            SelectionLength = 0;
            control.SizeChanged += (a, b) => UpdateRequestedArea();
            InputForm = inputForm;
        }

        public void Redraw()
        {
            m_control.Invalidate(true);
        }

        public int SelectionStart { get { return Math.Min(CursorPos, CursorPos + SelectionLength).Clamp(0, Text.Length); } }
        public int SelectionEnd { get { return Math.Max(CursorPos, CursorPos + SelectionLength).Clamp(0, Text.Length); } }

        CharacterRange MakeCharacterRange(int start, int length)
        {
            if (length < 0)
            {
                start = start + length;
                length = -length;
            }
            if (start < 0)
                start = 0;
            if (start + length > MeasureText.Length)
                length = MeasureText.Length - start;
            return new CharacterRange(start, length);
        }

        Region[] MeasureCharacterRanges(Graphics g)
        {
            return MeasureCharacterRanges(g, Enumerable.Range(0, MeasureText.Length).Select(i => MakeCharacterRange(i, 1)).ToArray());
        }

        Region[] MeasureCharacterRanges(Graphics g, params CharacterRange[] ranges)
        {
            Region[] result;
            if (ranges.Length <= 32)
            {
                Format.SetMeasurableCharacterRanges(ranges);
                result = g.MeasureCharacterRanges(MeasureText, Font, TextRectangle, Format);
            }
            else
            {
                result = new Region[ranges.Length];
                for (int start = 0; start < ranges.Length; start += 32)
                {
                    var length = Math.Min(32, ranges.Length - start);
                    CharacterRange[] charRanges = new CharacterRange[length];
                    Array.Copy(ranges, start, charRanges, 0, length);
                    Format.SetMeasurableCharacterRanges(charRanges);
                    var newRegions = g.MeasureCharacterRanges(MeasureText, Font, TextRectangle, Format);
                    Array.Copy(newRegions, 0, result, start, length);
                }
            }

            return result;
        }

        public RectangleF PosToXY(int characterPos, Graphics g)
        {
            if (characterPos == 0)
            {
                if (MeasureText.Length == 0)
                    return RectangleF.FromLTRB(TextRectangle.X, TextRectangle.Top, TextRectangle.X, TextRectangle.Top + CARET_HEIGHT);
                else
                {
                    var char0 = MeasureCharacterRanges(g, MakeCharacterRange(0, 1))[0].GetBounds(g);
                    return RectangleF.FromLTRB(char0.Left, char0.Top, char0.Left, char0.Bottom);
                }
            }
            if (characterPos < MeasureText.Length && MeasureText[characterPos - 1] == '\n')
            {
                var next = MeasureCharacterRanges(g, MakeCharacterRange(characterPos, 1))[0].GetBounds(g);
                return RectangleF.FromLTRB(next.Left, next.Top, next.Left, next.Bottom);
            }
            else if (MeasureText.Length > characterPos)
            {
                var regions = MeasureCharacterRanges(g, MakeCharacterRange(characterPos - 1, 1), MakeCharacterRange(characterPos, 1));
                var rectangles = Array.ConvertAll(regions, r => r.GetBounds(g));
                if (rectangles[0].Top < rectangles[1].Top)
                    return RectangleF.FromLTRB(rectangles[1].Left, rectangles[1].Top, rectangles[1].Left, rectangles[1].Bottom);
                else
                    return RectangleF.FromLTRB(rectangles[0].Right, rectangles[0].Top, rectangles[0].Right, rectangles[0].Bottom);
            }
            else
            {
                return MeasureCharacterRanges(g, MakeCharacterRange(characterPos - 1, 1))[0].GetBounds(g);
            }
        }

        public override void Paint(Graphics g)
        {
            //TODO:
            //using (Bitmap bmp = new Bitmap(100, 100))
            //{
            //    using (Graphics gg = Graphics.FromImage(bmp))
            //    {
            //        Format.SetMeasurableCharacterRanges(new[] { MakeCharacterRange(14, 1) });
            //        Region rrrr = gg.MeasureCharacterRanges("shane's test       ", Font, new RectangleF(0, 0, 1000, 1000), Format)[0];
            //        var b = rrrr.GetBounds(gg);
            //        gg.FillRegion(Brushes.Green, rrrr);
            //        gg.DrawString("shane's test ", Font, Brushes.Red, new PointF(0, 0));
            //    }
            //    bmp.Save(@"C:\test.png");
            //}

            var area = Area;
            g.FillRectangle(Colors.BackgroundBrush, area);
            g.DrawRectangle(Colors.BorderPen, new Rectangle((int)area.X, (int)area.Y, (int)Math.Floor(area.Width) - 1, (int)Math.Floor(area.Height) - 1));

            using (g.SaveState())
            {
                g.Clip = new Region(TextRectangle);
                //TextRenderer.DrawText(g, Text, Font, TextRectangle.Round(), Color.Green, TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
                g.DrawString(Text, Font, Colors.TextBrush, TextRectangle, Format);
            }

            using (g.SaveState())
            {
                Region r = new Region();
                r.MakeEmpty();
                var ranges = MeasureCharacterRanges(g, Enumerable.Range(SelectionStart, SelectionEnd - SelectionStart).Where(i => MeasureText[i] != '\n').Select(i => MakeCharacterRange(i, 1)).ToArray());
                for (int i = 0; i < ranges.Length; i++)
                    r.Union(ranges[i]);
                r.Intersect(TextRectangle);
                g.Clip = r;
                g.FillRectangle(Colors.SelectedBackgroundBrush, area);
                g.DrawString(Text, Font, Colors.SelectedTextBrush, TextRectangle, Format);
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
                    CursorPos = GetCursorPosition(args.X, args.Y, g);
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
                    var selectionEnd = CursorPos + SelectionLength;
                    CursorPos = GetCursorPosition(args.X, args.Y, g);
                    SelectionLength = selectionEnd - CursorPos;
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
                        CursorPos = GetCursorPosition(args.X, args.Y, g);
                    }
                }
            }
        }

        private void MoveCaret(Graphics g)
        {
            //if (InputForm != InputFormEnum.None)
            {
                if (CursorPos == 0)
                {
                    Caret.SetCaretPos((int)(TextRectangle.X), (int)TextRectangle.Y + 2);
                }
                else
                {
                    var pos = PosToXY(CursorPos, g);
                    Caret.SetCaretPos((int)(pos.Right), (int)pos.Top);
                }
            }
        }

        private int PosForChar(float x, float y, RectangleF charBound, int i)
        {
            if (x * 2 < charBound.Left + charBound.Right)
                return i;
            else
                return i + 1;
        }

        public int GetCursorPosition(float x, float y, Graphics g)
        {
            if (Text.Length == 0)
                return 0;

            var bounds = Array.ConvertAll(MeasureCharacterRanges(g), r => r.GetBounds(g));
            var topLeftChar = bounds[0];
            var lastChar = bounds.Last(b => !b.IsEmpty);
            if (topLeftChar.Top > y) //Query location is above the text
            {
                for (int i = 0; i < bounds.Length; i++)
                {
                    if (bounds[i].Top > topLeftChar.Top)
                    {
                        if (MeasureText[i - 1] == '\n')
                            return PosForChar(x, y, bounds[i - 2], i - 2);
                        else
                            return PosForChar(x, y, bounds[i - 1], i - 1);
                    }
                    else if (x < bounds[i].Right)
                    {
                        return PosForChar(x, y, bounds[i], i);
                    }
                }
                return PosForChar(x, y, bounds[bounds.Length - 1], bounds.Length);
            }
            else if (lastChar.Bottom < y) //Query location is below the text
            {
                for (int i = bounds.Length - 1; i >= 0; i--)
                {
                    if (!bounds[i].IsEmpty)
                    {
                        if (bounds[i].Top < lastChar.Top)
                        {
                            return PosForChar(x, y, bounds[i + 1], i + 1);
                        }
                        else if (x > bounds[i].Left)
                        {
                            return PosForChar(x, y, bounds[i], i);
                        }
                    }
                }
                return PosForChar(x, y, bounds[0], 0);
            }
            else if (topLeftChar.Left > x)
            {
                for (int i = 0; i < bounds.Length; i++)
                {
                    if (bounds[i].Bottom >= y)
                    {
                        return PosForChar(x, y, bounds[i], i);
                    }
                }
            }
            else
            {
                for (int i = bounds.Length - 1; i >= 0; i--)
                {
                    if (!bounds[i].IsEmpty)
                    {
                        if (bounds[i].Top <= y && bounds[i].Left <= x)
                            if (MeasureText[i] != '\n')
                                return PosForChar(x, y, bounds[i], i);
                            else
                                return i;
                        if (bounds[i].Bottom < y)
                            return PosForChar(x, y, bounds[i - 1], i - 1);
                    }
                }
            }
            return 0;
        }

        public void DeleteSelection()
        {
            if (Math.Abs(SelectionLength) > 0)
            {
                Text = Text.Remove(SelectionStart, Math.Abs(SelectionLength));
                CursorPos = SelectionStart;
                SelectionLength = 0;
                Redraw();
            }
        }

        public override void KeyPress(KeyPressEventArgs args)
        {
            if (!char.IsControl(args.KeyChar) || args.KeyChar == '\r')
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
                        Text = Text.Insert(CursorPos, args.KeyChar.ToString());
                    else if (args.KeyChar == '\r')
                        Text = Text.Insert(CursorPos, "\n"); //Currently newlines are considered like any other text in terms of resetting undo state
                    CursorPos++;
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
            //if (InputForm != InputFormEnum.None)
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
                CursorPos = Text.Length;
                m_additionUndoAction = null;
            }
            else if (e.KeyCode.IsSet(Keys.C) && e.Control)
            {
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
                    Text = Text.Insert(CursorPos, insertText);
                    CursorPos += insertText.Length;
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
                var oldPos = CursorPos;
                int nextPos = CursorPos + 1;
                if (e.Control)
                {
                    Regex r = new Regex(@"\s*\S*");
                    nextPos = r.Match(Text, CursorPos).Length + CursorPos;
                }
                CursorPos = nextPos;

                if (e.Shift)
                {
                    if (SelectionLength <= 0)
                        SelectionLength -= CursorPos - oldPos;
                    else
                        SelectionLength -= CursorPos - oldPos;
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
                var oldPos = CursorPos;
                int nextPos = CursorPos - 1;
                if (e.Control)
                {
                    Regex r = new Regex(@"\S*\s*$");
                    string before = Text.Substring(0, CursorPos);
                    var match = r.Match(before);
                    nextPos = match.Index;
                }
                CursorPos = nextPos;

                if (e.Shift)
                {
                    if (SelectionLength <= 0)
                        SelectionLength -= CursorPos - oldPos;
                    else
                        SelectionLength -= CursorPos - oldPos;
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
                    PointF pos = PosToXY(CursorPos, g).RightCenter();
                    PointF higherPos = pos;
                    int i;
                    for (i = CursorPos - 1; i > 0 && higherPos.Y == pos.Y; i--)
                    {
                        var rect = PosToXY(i, g);
                        higherPos = rect.RightCenter();
                        System.Diagnostics.Debug.WriteLine(rect);
                    }
                    PointF newPos = new PointF(pos.X, higherPos.Y);
                    if (e.Shift)
                    {
                        var selectionEnd = CursorPos + SelectionLength;
                        CursorPos = GetCursorPosition(newPos.X, newPos.Y, g);
                        SelectionLength = selectionEnd - CursorPos;
                    }
                    else
                    {
                        SelectionLength = 0;
                        CursorPos = GetCursorPosition(newPos.X, newPos.Y, g);
                    }
                    Redraw();
                }
                m_additionUndoAction = null;
            }
            else if (e.KeyCode.IsSet(Keys.Down))
            {
                using (Graphics g = m_control.CreateGraphics())
                {
                    PointF pos = PosToXY(CursorPos, g).RightCenter();
                    PointF lowerPos = pos;
                    for (int i = CursorPos + 1; i <= MeasureText.Length && lowerPos.Y == pos.Y; i++)
                    {
                        lowerPos = PosToXY(i, g).RightCenter();
                    }
                    PointF newPos = new PointF(pos.X, lowerPos.Y);
                    if (e.Shift)
                    {
                        var selectionEnd = CursorPos + SelectionLength;
                        CursorPos = GetCursorPosition(newPos.X, newPos.Y, g);
                        SelectionLength = selectionEnd - CursorPos;
                    }
                    else
                    {
                        SelectionLength = 0;
                        CursorPos = GetCursorPosition(newPos.X, newPos.Y, g);
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
                    else if (CursorPos < Text.Length)
                    {
                        if (e.Control)
                        {
                            Regex r = new Regex(@"\s*\S*");
                            SelectionLength = r.Match(Text, CursorPos).Length;
                            DeleteSelection();
                        }
                        else
                        {
                            Text = Text.Remove(CursorPos, 1);
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
                    else if (CursorPos > 0)
                    {
                        if (e.Control)
                        {
                            Regex r = new Regex(@"\S*\s*$");
                            string before = Text.Substring(0, CursorPos);
                            var match = r.Match(before);
                            CursorPos = match.Index;
                            SelectionLength = match.Length;
                            DeleteSelection();
                        }
                        else
                        {
                            Text = Text.Remove(CursorPos - 1, 1);
                            CursorPos--;
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
                    var end = CursorPos + SelectionLength;
                    SelectionLength = end;
                }
                else
                {
                    SelectionLength = 0;
                }

                if (e.Control)
                {
                    CursorPos = 0;
                }
                else
                {
                    using (var g = m_control.CreateGraphics())
                    {
                        RectangleF cursorXY = PosToXY(CursorPos, g);
                        PointF pos = cursorXY.Center();
                        pos.X = int.MinValue;
                        CursorPos = GetCursorPosition(pos.X, pos.Y, g);
                    }
                }

                m_additionUndoAction = null;
            }
            else if (e.KeyCode == Keys.End)
            {
                if (e.Shift)
                {
                    var start = CursorPos + SelectionLength;
                    SelectionLength = start - Text.Length;
                }
                else
                {
                    SelectionLength = 0;
                }

                if (e.Control)
                {
                    CursorPos = Text.Length;
                }
                else
                {
                    using (var g = m_control.CreateGraphics())
                    {
                        RectangleF cursorXY = PosToXY(CursorPos, g);
                        PointF pos = cursorXY.Center();
                        pos.X = int.MaxValue;
                        CursorPos = GetCursorPosition(pos.X, pos.Y, g);
                    }
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
            Caret.DestroyCaret();
        }
    }
}

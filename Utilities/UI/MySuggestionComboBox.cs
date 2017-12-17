using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Utilities.UI
{
    public class UpArrowItem : ToolStripButton
    {
        private static Bitmap UpArrow;
        private static Bitmap DownArrow;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "I believe this would actually be slower due to the inability to cache Assembly.GetExecutingAssembly() and UpArrow")]
        static UpArrowItem()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.UpArrow.png"))
                UpArrow = new Bitmap(stream);
            DownArrow = new Bitmap(UpArrow);
            DownArrow.RotateFlip(RotateFlipType.RotateNoneFlipY);
        }

        public override Size GetPreferredSize(Size constrainingSize)
        {
            return new Size(m_width, 10);
        }

        protected override void OnClick(EventArgs e)
        {
            m_clicked();
        }

        int m_width;
        private Action m_clicked;
        public UpArrowItem(int width, Action clicked, bool up)
        {
            m_width = width;
            m_clicked = clicked;
            Image = up ? UpArrow : DownArrow;
            this.ImageTransparentColor = Color.Magenta;
            this.ImageAlign = ContentAlignment.MiddleCenter;
            this.ImageScaling = ToolStripItemImageScaling.None;
            this.Padding = new Padding { All = 0 };
            this.Margin = new Padding { All = 0 };
        }

        public override ToolStripItemDisplayStyle DisplayStyle
        {
            get
            {
                return ToolStripItemDisplayStyle.Image;
            }
            set
            {
                //Ignore that shit
            }
        }

        //public override Size GetPreferredSize(Size constrainingSize)
        //{
        //    Size result = base.GetPreferredSize(constrainingSize);
        //    //if (result.Height > 10)
        //        result.Height = 100;
        //    return result;
        //}

    }

    public class MyComboBoxItem<T>
    {
        private readonly string m_displayString;
        private readonly T m_contents;

        public string DisplayString { get { return m_displayString; } }
        public T Contents { get { return m_contents; } }
        private bool m_sourcedValue;
        public MyComboBoxItem(string displayString, T contents)
        {
            m_displayString = displayString;
            m_contents = contents;
            m_sourcedValue = true;
        }
        public MyComboBoxItem(string displayString)
        {
            m_displayString = displayString;
            m_contents = default(T);
            m_sourcedValue = false;
        }
        public override string ToString()
        {
            return DisplayString;
        }
        public override bool Equals(object obj)
        {
            var other = obj as MyComboBoxItem<T>;
            if (other == null)
                return false;
            else if (m_sourcedValue != other.m_sourcedValue)
                return false;
            else if (m_sourcedValue)
                return object.Equals(Contents, other.Contents);
            else
                return object.Equals(DisplayString, other.DisplayString);
        }
        public override int GetHashCode()
        {
            return 0;
        }
    }

    public class MySuggestionBox<T> : MyControl
    {
        private SizeF m_requestedSize;
        private MyTextBox m_textBox;
        private Control m_control;
        private bool m_allowCustomText;
        private Func<RectangleF> m_buttonArea;
        private Func<RectangleF> m_area;
        private Func<RectangleF> m_textBoxArea;
        private ToolStripDropDown m_dropDown;
        private readonly bool m_HasDropDownButton = true;
        private MyComboBoxItem<T> m_selectedItem = new MyComboBoxItem<T>("Uninitialized default", default(T));

        public ToolStripRenderer Renderer { get { return m_dropDown.Renderer; } set { m_dropDown.Renderer = value; } }
        public IEnumerable<MyComboBoxItem<T>> Items { get; }

        public override event Action RequestedAreaChanged;
        public event Action SelectionChanged;

        public void ExecuteSelectionChanged() { SelectionChanged.Execute(); }

        public SizeF RequestedArea
        {
            get { return m_requestedSize; }
            protected set
            {
                if (m_requestedSize != value)
                {
                    m_requestedSize = value;
                    RequestedAreaChanged.Execute();
                }
            }
        }

        private const int ButtonWidth = 20;

        public MySuggestionBox(Control control, Func<RectangleF> area, bool allowCustomText, IEnumerable<MyComboBoxItem<T>> items)
        {
            m_allowCustomText = allowCustomText;
            m_control = control;
            m_area = area;
            Items = items;

            m_textBoxArea = m_area;
            if (m_HasDropDownButton)
            {
                m_buttonArea = () => { var a = area(); return RectangleF.FromLTRB(a.Right - ButtonWidth, a.Top, a.Right, a.Bottom); };
                m_textBoxArea = () => { var a = area(); return RectangleF.FromLTRB(a.Left, a.Top, a.Right - ButtonWidth, a.Bottom); };
            }

            m_dropDown = new ToolStripDropDown();
            m_dropDown.Opened += (a, b) => { m_dropDownOpen = true; };
            m_dropDown.Closed += (a, b) => { m_dropDownOpen = false; m_itemIndex = -1; m_dropDownWindow = 0; };

            m_textBox = new MyTextBox(control, m_textBoxArea, allowCustomText ? MyTextBox.InputFormEnum.Text : MyTextBox.InputFormEnum.None, null);
            m_textBox.Font = m_dropDown.Font;
            m_textBox.RequestedAreaChanged += () => { RequestedArea = new SizeF(Area.Width, m_textBox.RequestedArea.Height); };

            m_textBox.TextChanged += s => ExecuteSelectionChanged();
            m_textBox.TextChanged += s => { m_itemIndex = -1; m_dropDownWindow = 0; UpdateDropdown(); };
        }

        public void SetupCallbacks()
        {
            m_control.MouseDown += (a, args) => MouseDown(args);
            m_control.MouseUp += (a, args) => MouseUp(args);
            m_control.MouseMove += (a, args) => MouseMove(args);
            m_control.MouseClick += (a, args) => MouseClick(args);
            m_control.KeyPress += (a, args) => KeyPress(args);
            m_control.KeyDown += (a, args) => KeyDown(args);
            m_control.GotFocus += (a, args) => GotFocus();
            m_control.LostFocus += (a, args) => LostFocus();
            m_control.MouseWheel += (a, args) => MouseWheel(args);
            m_control.Paint += (a, args) => Paint(args.Graphics);
        }

        public RectangleF Area { get { return m_area(); } }

        public MyTextBox.ColorOptions TextBoxColors { get { return m_textBox.Colors; } set { m_textBox.Colors = value; } }

        public override void MouseUp(MouseEventArgs args)
        {
            if (m_textBox.Area.Contains(args.Location)) m_textBox.MouseUp(args);
        }

        public override void MouseMove(MouseEventArgs args)
        {
            if (m_textBox.Area.Contains(args.Location)) m_textBox.MouseMove(args);
        }

        public override void MouseClick(MouseEventArgs args)
        {
            if (m_textBox.Area.Contains(args.Location)) m_textBox.MouseClick(args);
        }

        public override void GotFocus()
        {
            m_textBox.GotFocus();
            m_textBox.SetCursorPosition(int.MaxValue);
        }

        public override void KeyPress(KeyPressEventArgs args)
        {
            bool isEnter = args.KeyChar == '\n' || args.KeyChar == '\r';
            if (!isEnter)
            {
                m_textBox.KeyPress(args);
            }
        }

        public override void Paint(Graphics g)
        {
            m_textBox.Paint(g);

            if (m_HasDropDownButton)
                DrawButton(g, m_buttonArea());
        }

        private void DrawButton(Graphics graphics, RectangleF buttonArea)
        {
            graphics.DrawRectangle(TextBoxColors.BorderPen, RectangleF.FromLTRB(buttonArea.Left, buttonArea.Top, buttonArea.Right - 1, buttonArea.Bottom - 1));
            using (var path = new GraphicsPath())
            {
                path.AddLines(new PointF[] { buttonArea.Location.Plus(buttonArea.Width/4,buttonArea.Height/3),
                                         buttonArea.Location.Plus(3*buttonArea.Width/4, buttonArea.Height/3),
                                         buttonArea.Location.Plus(buttonArea.Width/2, 2*buttonArea.Height/3)});
                graphics.FillPath(TextBoxColors.TextBrush, path);
            }
        }

        private MyComboBoxItem<T> MatchingItem()
        {
            return Items.FirstOrDefault(i => i.DisplayString == m_textBox.Text);
        }

        public override void MouseCaptureChanged()
        {
            m_textBox.MouseCaptureChanged();
        }

        public override bool Contains(PointF point)
        {
            return Area.Contains(point);
        }

        public MyComboBoxItem<T> SelectedItem
        {
            get
            {
                if (m_allowCustomText)
                {
                    return MatchingItem() ?? new MyComboBoxItem<T>(m_textBox.Text);
                }
                else
                {
                    return m_selectedItem;
                }
            }
            set
            {
                m_selectedItem = value;
                m_textBox.Text = m_selectedItem.DisplayString;
                m_textBox.SetCursorPosition(int.MaxValue);
                m_dropDown.Close();
            }
        }

        public override void LostFocus()
        {
            m_textBox.LostFocus();
            m_dropDown.Close();
            m_textBox.Text = SelectedItem.DisplayString;
        }

        public override void MouseDown(MouseEventArgs args)
        {
            if (m_buttonArea().Contains(args.Location))
                ButtonMouseDown();
            else if (m_textBox.Area.Contains(args.Location))
                m_textBox.MouseDown(args);
        }

        public override void KeyDown(KeyEventArgs args)
        {
            if (args.KeyCode == Keys.Escape)
            {
                if (m_allowCustomText || MatchingItem() != null)
                    m_dropDown.Close();
            }
            else if (args.KeyCode == Keys.Enter)
            {
                if (m_dropDownOpen)
                {
                    if (SelectedToolStripItem != null)
                    {
                        SelectedToolStripItem.PerformClick();
                    }
                    else if (!m_allowCustomText)
                    {
                        var best = Items.FirstOrDefault(i => i.DisplayString == m_textBox.Text);
                        if (best != null)
                        {
                            SelectedItem = best;
                        }
                    }
                    m_dropDown.Close();
                }
                else
                {
                    EnterPressed.Execute();
                }
            }
            else if (args.KeyCode == Keys.Down)
            {
                ItemIndex++;
            }
            else if (args.KeyCode == Keys.Up)
            {
                ItemIndex--;
            }
            else
            {
                m_textBox.KeyDown(args);
            }
        }


        private bool m_dropDownOpen = false;
        public Color SelectedBackgroundColor { get; set; } = Color.Green;
        private const int WINDOW_SIZE = 4;
        private int m_dropDownWindow = 0;
        public event Action EnterPressed;

        private void ButtonMouseDown()
        {
            UpdateDropdown();
        }

        private void UpdateDropdown()
        {
            m_dropDown.SuspendLayout();
            var width = (int)m_area().Width - 2;
            var height = m_dropDown.Font.Height + 4;
            m_dropDown.Items.Clear();
            m_dropDown.Items.Add(new UpArrowItem(width, () => { m_dropDownWindow--; UpdateDropdown(); }, true));
            var sorted = Items.OrderBy(i => !i.DisplayString.StartsWith(m_textBox.Text, StringComparison.CurrentCultureIgnoreCase))
                              .ThenByDescending(i => i.DisplayString.IndexOf(m_textBox.Text, StringComparison.CurrentCultureIgnoreCase) != -1)
                              .ThenBy(i => i.DisplayString.ToUpper(CultureInfo.CurrentCulture));
            var strictSorted = sorted.ToList();
            if (m_dropDownWindow + WINDOW_SIZE > strictSorted.Count)
                m_dropDownWindow = strictSorted.Count - WINDOW_SIZE;
            if (m_dropDownWindow < 0)
                m_dropDownWindow = 0;
            var limited = sorted.Skip(m_dropDownWindow).TakeUpTo(WINDOW_SIZE);
            foreach (var item in limited)
            {
                var itemitem = item;
                var element = m_dropDown.Items.Add(item.DisplayString, null, (a, b) => { SelectedItem = itemitem; });
                element.TextAlign = ContentAlignment.MiddleLeft;
                element.AutoSize = false;
                element.Height = height;
                element.Width = width;
            }
            m_dropDown.Items.Add(new UpArrowItem(width, () => { m_dropDownWindow++; UpdateDropdown(); }, false));
            m_dropDown.AutoClose = false;
            if (!m_dropDownOpen)
                m_dropDown.Show(m_control.PointToScreen(new Point((int)Area.Left, (int)Area.Bottom)));

            if (m_dropDownWindow <= 0)
                m_dropDown.Items[0].Visible = false;
            if (m_dropDownWindow >= strictSorted.Count - WINDOW_SIZE)
                m_dropDown.Items.OfType<ToolStripItem>().Last().Visible = false;

            m_dropDown.ResumeLayout();
        }

        private int m_itemIndex = -1;
        private int ItemIndex
        {
            get { return m_itemIndex; }
            set
            {
                if (SelectedToolStripItem != null)
                    SelectedToolStripItem.BackColor = m_dropDown.BackColor;
                if (value >= 0 && value < Items.Count())
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
                    SelectedToolStripItem.BackColor = SelectedBackgroundColor;
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

        public override void MouseWheel(MouseEventArgs args)
        {
            if (m_dropDownOpen)
            {
                if (args.Delta < 0)
                    m_dropDownWindow++;
                else
                    m_dropDownWindow--;
                m_itemIndex = -1;
                UpdateDropdown();
            }
        }

        //TODO: Awful hack
        public void ParentFormMouseActivatedHack()
        {
            m_dropDown.Close();
        }
    }
}

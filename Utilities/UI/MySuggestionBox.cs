using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Utilities
{
    //TODO: Refactor commonality out of MyComboBox
    public class MySuggestionBox<T> : MyControl
    {
        public class Item
        {
            public readonly string DisplayString;
            public readonly T Contents;
            public bool SourcedValue;
            public Item(string displayString, T contents)
            {
                DisplayString = displayString;
                Contents = contents;
                SourcedValue = true;
            }
            public Item(string displayString)
            {
                DisplayString = displayString;
                Contents = default(T);
                SourcedValue = false;
            }
            public override string ToString()
            {
                return DisplayString;
            }
            public override bool Equals(object obj)
            {
                var other = obj as Item;
                if (other == null)
                    return false;
                else if (SourcedValue != other.SourcedValue)
                    return false;
                else if (SourcedValue)
                    return object.Equals(Contents, other.Contents);
                else
                    return object.Equals(DisplayString, other.DisplayString);
            }
            public override int GetHashCode()
            {
                return 0;
            }
        }

        public override event Action RequestedAreaChanged;
        public event Action SelectionChanged;
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

        private MyTextBox m_textBox;
        private Control m_control;

        Func<RectangleF> m_area;
        public RectangleF Area { get { return m_area(); } }
        ToolStripDropDown m_dropDown;

        public class ColorOptions
        {
            public MyTextBox.ColorOptions TextBox;
            public Color SelectedBackground;
        }

        private ColorOptions m_colors;
        public ColorOptions Colors { get { return m_colors; } set { m_colors = value; m_textBox.Colors = value.TextBox; } }

        public MySuggestionBox(Control control, Func<RectangleF> area, bool allowCustomText, IEnumerable<Item> items)
        {
            m_allowCustomText = allowCustomText;
            m_control = control;
            m_area = area;
            Items = items;

            m_dropDown = new ToolStripDropDown();
            m_dropDown.Opened += (a, b) => { m_dropDownOpen = true; };
            m_dropDown.Closed += (a, b) => { m_dropDownOpen = false; m_itemIndex = -1; m_dropDownWindow = 0; };

            m_textBox = new MyTextBox(control, m_area, MyTextBox.InputFormEnum.Text);
            m_textBox.Font = m_dropDown.Font;
            m_textBox.RequestedAreaChanged += () => { RequestedArea = new SizeF(Area.Width, m_textBox.RequestedArea.Height); };

            m_textBox.TextChanged += s => SelectionChanged.Execute();
            m_textBox.TextChanged += s => { if (!m_ignoreTextChange) { m_itemIndex = -1; m_dropDownWindow = 0; UpdateDropdown(); } };

            m_colors = new ColorOptions() { TextBox = m_textBox.Colors, SelectedBackground = Color.Green };
        }

        public override void MouseDown(MouseEventArgs args)
        {
            UpdateDropdown();
            m_textBox.MouseDown(args);
        }

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
        }

        public override void LostFocus()
        {
            m_textBox.LostFocus();
            m_dropDown.Close();
            m_textBox.Text = SelectedItem.DisplayString;
        }

        bool m_ignoreTextChange = true;

        public override void KeyPress(KeyPressEventArgs args)
        {
            m_ignoreTextChange = false;
            bool isEnter = args.KeyChar == '\n' || args.KeyChar == '\r';
            if (!isEnter)
                m_textBox.KeyPress(args);
            m_ignoreTextChange = true;
        }

        int m_itemIndex = -1;
        private int ItemIndex
        {
            get { return m_itemIndex; }
            set
            {
                Debug.WriteLine("Index changed to " + value + " m_dropDownWindow: " + m_dropDownWindow);
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
                    SelectedToolStripItem.BackColor = Colors.SelectedBackground;
            }
        }
        private ToolStripItem SelectedToolStripItem
        {
            get
            {
                if (ItemIndex - m_dropDownWindow < 0)
                    return null;
                if (ItemIndex - m_dropDownWindow >= m_dropDown.Items.Count)
                    return null;
                return m_dropDown.Items[ItemIndex - m_dropDownWindow];
            }
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
                    if (m_allowCustomText)
                    {
                    }
                    else if (SelectedToolStripItem != null)
                    {
                        SelectedToolStripItem.PerformClick();
                    }
                    else
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
                m_ignoreTextChange = false;
                m_textBox.KeyDown(args);
                m_ignoreTextChange = true;
            }
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

        public override void Paint(Graphics graphics)
        {
            m_textBox.Paint(graphics);
        }

        private Item MatchingItem()
        {
            return Items.FirstOrDefault(i => i.DisplayString == m_textBox.Text);
        }

        private Item m_selectedItem = new Item("Uninitialized default", default(T));
        public Item SelectedItem
        {
            get
            {
                if (m_allowCustomText)
                {
                    return MatchingItem() ?? new Item(m_textBox.Text);
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
                m_textBox.CursorPos = new MyTextBox.CP(int.MaxValue);
                m_dropDown.Close();
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

        const int WINDOW_SIZE = 12;
        int m_dropDownWindow = 0;

        private void UpdateDropdown()
        {
            m_dropDown.SuspendLayout();
            var width = (int)m_area().Width - 2;
            var height = m_dropDown.Font.Height + 4;
            m_dropDown.Items.Clear();
            var matches = Items.Where(i => i.DisplayString.ToUpper().Contains(m_textBox.Text.ToUpper()));
            var sorted = matches.OrderBy(i => !i.DisplayString.ToUpper().StartsWith(m_textBox.Text.ToUpper())).ThenBy(i => i.DisplayString.ToUpper());
            var strictSorted = sorted.ToList();
            if (m_dropDownWindow + WINDOW_SIZE > strictSorted.Count)
                m_dropDownWindow -= strictSorted.Count - WINDOW_SIZE;
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
            m_dropDown.AutoClose = false;
            if (!m_dropDownOpen)
                m_dropDown.Show(m_control.PointToScreen(new Point((int)Area.Left, (int)Area.Bottom)));
            m_dropDown.ResumeLayout();
        }

        public override void MouseCaptureChanged()
        {
            m_textBox.MouseCaptureChanged();
        }

        public ToolStripRenderer Renderer { get { return m_dropDown.Renderer; } set { m_dropDown.Renderer = value; } }
        public readonly IEnumerable<Item> Items;
        private bool m_allowCustomText;
        private bool m_dropDownOpen = false;

        public event Action EnterPressed;

        public override bool Contains(PointF point)
        {
            return Area.Contains(point);
        }
    }
}

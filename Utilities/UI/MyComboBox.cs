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
    public class MyComboBox<T> : MyControl
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

        Func<RectangleF> m_buttonArea;
        const int BUTTON_WIDTH = 20;

        Func<RectangleF> m_area;
        public RectangleF Area { get { return m_area(); } }
        ToolStripDropDown m_dropDown;
        private bool m_allowCustomText;

        private readonly bool m_HasDropDownButton = true;

        public MyTextBox.ColorOptions TextBoxColors { get { return m_textBox.Colors; } set { m_textBox.Colors = value; } }

        public MyComboBox(Control control, Func<RectangleF> area, bool allowCustomText, IEnumerable<Item> items)
        {
            m_allowCustomText = allowCustomText;
            m_control = control;
            m_area = area;
            Items = items;

            Func<RectangleF> m_textBoxArea = m_area;
            if (m_HasDropDownButton)
            {
                m_buttonArea = () => { var a = area(); return RectangleF.FromLTRB(a.Right - BUTTON_WIDTH, a.Top, a.Right, a.Bottom); };
                m_textBoxArea = () => { var a = area(); return RectangleF.FromLTRB(a.Left, a.Top, a.Right - BUTTON_WIDTH, a.Bottom); };
            }

            m_dropDown = new ToolStripDropDown();

            m_textBox = new MyTextBox(control, m_textBoxArea, allowCustomText ? MyTextBox.InputFormEnum.Text : MyTextBox.InputFormEnum.None);
            m_textBox.Font = m_dropDown.Font;
            m_textBox.RequestedAreaChanged += () => { RequestedArea = new SizeF(Area.Width, m_textBox.RequestedArea.Height); };

            m_textBox.TextChanged += s => SelectionChanged.Execute();
        }

        public override void MouseDown(MouseEventArgs args)
        {
            Debug.Print(Name + "MouseDown");
            if (m_buttonArea().Contains(args.Location))
                ButtonMouseDown(args);
            else if (m_textBox.Area.Contains(args.Location))
                m_textBox.MouseDown(args);
        }

        private void ButtonMouseDown(MouseEventArgs args)
        {
            if (m_buttonArea().Contains(args.Location))
            {
                var width = (int)m_area().Width - 2;
                m_dropDown.Items.Clear();
                foreach (var item in Items)
                {
                    var itemitem = item;
                    var element = m_dropDown.Items.Add(item.DisplayString, null, (a, b) => { SelectedItem = itemitem; });
                    element.TextAlign = ContentAlignment.MiddleLeft;
                    element.AutoSize = false;
                    element.Width = width;
                }
                m_dropDown.Show(m_control.PointToScreen(new Point((int)Area.Left, (int)Area.Bottom)));
            }
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
            Debug.Print(Name + "GotFocus");
            m_textBox.GotFocus();
            m_textBox.CursorPos = new MyTextBox.CP(int.MaxValue);
        }

        public override void LostFocus()
        {
            Debug.Print(Name + "LostFocus");
            m_textBox.LostFocus();
        }

        public override void KeyPress(KeyPressEventArgs args)
        {
            bool isEnter = args.KeyChar == '\n' || args.KeyChar == '\r';
            if (!isEnter)
            {
                m_textBox.KeyPress(args);
            }
        }

        public override void KeyDown(KeyEventArgs args)
        {
            m_textBox.KeyDown(args);
        }

        public override void Paint(Graphics graphics)
        {
            m_textBox.Paint(graphics);

            if (m_HasDropDownButton)
                DrawButton(graphics, m_buttonArea());
        }

        private void DrawButton(Graphics graphics, RectangleF buttonArea)
        {
            graphics.DrawRectangle(TextBoxColors.BorderPen, RectangleF.FromLTRB(buttonArea.Left, buttonArea.Top, buttonArea.Right - 1, buttonArea.Bottom - 1));
            var path = new GraphicsPath();
            path.AddLines(new PointF[] { buttonArea.Location.Plus(buttonArea.Width/4,buttonArea.Height/3),
                                         buttonArea.Location.Plus(3*buttonArea.Width/4, buttonArea.Height/3),
                                         buttonArea.Location.Plus(buttonArea.Width/2, 2*buttonArea.Height/3)});
            graphics.FillPath(TextBoxColors.TextBrush, path);
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
            }
        }

        public override void MouseWheel(MouseEventArgs args)
        {
            //Do nothing I guess...
        }

        public override void MouseCaptureChanged()
        {
            m_textBox.MouseCaptureChanged();
        }

        public ToolStripRenderer Renderer { get { return m_dropDown.Renderer; } set { m_dropDown.Renderer = value; } }
        public readonly IEnumerable<Item> Items;

        public event Action EnterPressed { add { m_textBox.EnterPressed += value; } remove { m_textBox.EnterPressed -= value; } }

        public override bool Contains(PointF point)
        {
            return Area.Contains(point);
        }
    }
}

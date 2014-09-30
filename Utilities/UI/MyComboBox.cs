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
        Func<RectangleF> m_area;
        public RectangleF Area { get { return m_area(); } }
        ToolStripDropDown m_dropDown;
        const int BUTTON_WIDTH = 20;

        public MyTextBox.ColorOptions Colors { get { return m_textBox.Colors; } set { m_textBox.Colors = value; } }

        public MyComboBox(Control control, Func<RectangleF> area, bool allowTextEdit, IEnumerable<Item> items)
        {
            m_allowTextEdit = allowTextEdit;
            m_control = control;
            m_area = area;
            Items = items;

            m_buttonArea = () => { var a = area(); return RectangleF.FromLTRB(a.Right - BUTTON_WIDTH, a.Top, a.Right, a.Bottom); };
            Func<RectangleF> m_textBoxArea = () => { var a = area(); return RectangleF.FromLTRB(a.Left, a.Top, a.Right - BUTTON_WIDTH, a.Bottom); };

            m_dropDown = new ToolStripDropDown();

            m_textBox = new MyTextBox(control, m_textBoxArea, MyTextBox.InputFormEnum.Text);
            m_textBox.Font = m_dropDown.Font;
            m_textBox.RequestedAreaChanged += () => { RequestedArea = new SizeF(Area.Width, m_textBox.RequestedArea.Height); };

            m_textBox.TextChanged += s => SelectionChanged.Execute();
        }

        public override void MouseDown(MouseEventArgs args)
        {
            if (m_allowTextEdit)
            {
                if (m_buttonArea().Contains(args.Location))
                    ButtonMouseDown(args);
                else if (m_textBox.Area.Contains(args.Location))
                    m_textBox.MouseDown(args);
            }
            else
            {
                if (m_buttonArea().Contains(args.Location))
                    ButtonMouseDown(args);
            }
        }

        public override void MouseUp(MouseEventArgs args)
        {
            if (m_allowTextEdit)
            {
                if (m_textBox.Area.Contains(args.Location)) m_textBox.MouseUp(args);
            }
        }

        public override void MouseMove(MouseEventArgs args)
        {
            if (m_allowTextEdit)
            {
                if (m_textBox.Area.Contains(args.Location)) m_textBox.MouseMove(args);
            }
        }

        public override void MouseClick(MouseEventArgs args)
        {
            if (m_allowTextEdit)
            {
                if (m_textBox.Area.Contains(args.Location)) m_textBox.MouseClick(args);
            }
        }

        public override void GotFocus()
        {
            if (m_allowTextEdit)
            {
                m_textBox.GotFocus();
            }
        }

        public override void LostFocus()
        {
            if (m_allowTextEdit)
            {
                m_textBox.LostFocus();
            }
        }

        public override void KeyPress(KeyPressEventArgs args)
        {
            if (m_allowTextEdit)
            {
                m_textBox.KeyPress(args);
            }
        }

        public override void KeyDown(KeyEventArgs args)
        {
            if (m_allowTextEdit)
            {
                m_textBox.KeyDown(args);
            }
            else if ( args.KeyCode.IsSet(Keys.Enter))
            {
                m_textBox.KeyDown(args);
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

            m_control.Paint += (a, args) => Paint(args.Graphics);
        }

        public override void Paint(Graphics graphics)
        {
            m_textBox.Paint(graphics);

            DrawButton(graphics, m_buttonArea());
        }

        private void DrawButton(Graphics graphics, RectangleF buttonArea)
        {
            graphics.DrawRectangle(Colors.BorderPen, RectangleF.FromLTRB(buttonArea.Left, buttonArea.Top, buttonArea.Right - 1, buttonArea.Bottom - 1));
            var path = new GraphicsPath();
            path.AddLines(new PointF[] { buttonArea.Location.Plus(buttonArea.Width/4,buttonArea.Height/3),
                                         buttonArea.Location.Plus(3*buttonArea.Width/4, buttonArea.Height/3),
                                         buttonArea.Location.Plus(buttonArea.Width/2, 2*buttonArea.Height/3)});
            graphics.FillPath(Colors.TextBrush, path);
        }

        private Item m_selectedItem = new Item("Uninitialized default", default(T));
        public Item SelectedItem
        {
            get
            {
                if (m_allowTextEdit)
                {
                    return Items.FirstOrDefault(i => i.DisplayString == m_textBox.Text) ?? new Item(m_textBox.Text);
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
                m_textBox.CursorPos = int.MaxValue;
            }
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
        private bool m_allowTextEdit;

        public bool SpecialEnter { get { return m_textBox.SpecialEnter; } set { m_textBox.SpecialEnter = value; } }
        public event Action EnterPressed { add { m_textBox.EnterPressed += value; } remove { m_textBox.EnterPressed -= value; } }

        public override bool Contains(PointF point)
        {
            return Area.Contains(point);
        }
    }
}

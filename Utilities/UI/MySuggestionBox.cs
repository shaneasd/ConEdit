using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace Utilities.UI
{
    public class UpArrowItem : ToolStripButton
    {
        public static Bitmap UpArrow;
        public static Bitmap DownArrow;
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

    //TODO: Refactor commonality out of MyComboBox
    public class MySuggestionBox<T> : MyControl
    {
        //public class ToolStripRenderer : System.Windows.Forms.ToolStripRenderer
        //{
        //    public System.Windows.Forms.ToolStripRenderer Inner;

        //    public ToolStripRenderer(System.Windows.Forms.ToolStripRenderer inner)
        //    {
        //        Inner = inner;
        //    }
        //    protected override void InitializePanel(ToolStripPanel toolStripPanel)
        //    {
        //        //base.InitializePanel(toolStripPanel);
        //    }

        //    protected override void Initialize(ToolStrip toolStrip)
        //    {
        //        //base.Initialize(toolStrip);
        //    }

        //    protected override void InitializeContentPanel(ToolStripContentPanel contentPanel)
        //    {
        //        //base.InitializeContentPanel(contentPanel);
        //    }

        //    protected override void InitializeItem(ToolStripItem item)
        //    {
        //        //base.InitializeItem(item);
        //    }

        //    protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        //    {
        //        //Inner.DrawArrow(e);
        //    }

        //    protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        //    {
        //        //Inner.DrawButtonBackground(e);
        //    }

        //    protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        //    {
        //        //Inner.DrawDropDownButtonBackground(e);
        //    }

        //    protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
        //    {
        //        //Inner.DrawGrip(e);
        //    }

        //    protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        //    {
        //        //Inner.DrawImageMargin(e);
        //    }

        //    protected override void OnRenderItemBackground(ToolStripItemRenderEventArgs e)
        //    {
        //        //Inner.DrawItemBackground(e);
        //    }

        //    protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        //    {
        //        //Inner.DrawItemCheck(e);
        //    }

        //    protected override void OnRenderItemImage(ToolStripItemImageRenderEventArgs e)
        //    {
        //        //Inner.DrawItemImage(e);
        //    }

        //    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        //    {
        //        //ToolStripItemTextRenderEventArgs args = new ToolStripItemTextRenderEventArgs(e.Graphics, e.Item, e.Text, e.TextRectangle, e.TextColor, e.TextFont, e.TextFormat);
        //        //Inner.DrawItemText(args);
        //    }

        //    protected override void OnRenderLabelBackground(ToolStripItemRenderEventArgs e)
        //    {
        //        //Inner.DrawLabelBackground(e);
        //    }

        //    protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        //    {
        //        //Inner.DrawMenuItemBackground(e);
        //    }

        //    protected override void OnRenderOverflowButtonBackground(ToolStripItemRenderEventArgs e)
        //    {
        //        //Inner.DrawOverflowButtonBackground(e);
        //    }

        //    protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        //    {
        //        //Inner.DrawSeparator(e);
        //    }

        //    protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
        //    {
        //        //Inner.DrawSplitButton(e);
        //    }

        //    protected override void OnRenderStatusStripSizingGrip(ToolStripRenderEventArgs e)
        //    {
        //        //Inner.DrawStatusStripSizingGrip(e);
        //    }

        //    protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        //    {
        //        //Inner.DrawToolStripBackground(e);
        //    }

        //    protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        //    {
        //        //Inner.DrawToolStripBorder(e);
        //    }

        //    protected override void OnRenderToolStripContentPanelBackground(ToolStripContentPanelRenderEventArgs e)
        //    {
        //        //Inner.DrawToolStripContentPanelBackground(e);
        //    }

        //    protected override void OnRenderToolStripPanelBackground(ToolStripPanelRenderEventArgs e)
        //    {
        //        //Inner.DrawToolStripPanelBackground(e);
        //    }

        //    protected override void OnRenderToolStripStatusLabelBackground(ToolStripItemRenderEventArgs e)
        //    {
        //        //Inner.DrawToolStripStatusLabelBackground(e);
        //    }
        //}

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
        private bool m_dropDownOpen = false;
        private readonly bool m_HasDropDownButton = true;

        public MyTextBox.ColorOptions TextBoxColors { get { return m_textBox.Colors; } set { m_textBox.Colors = value; } }
        public Color SelectedBackgroundColor = Color.Green;

        public MySuggestionBox(Control control, Func<RectangleF> area, bool allowCustomText, IEnumerable<MyComboBoxItem<T>> items)
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
            m_dropDown.Opened += (a, b) => { m_dropDownOpen = true; };
            m_dropDown.Closed += (a, b) => { m_dropDownOpen = false; m_itemIndex = -1; m_dropDownWindow = 0; };
            //m_dropDown.Renderer = new ToolStripRenderer(m_dropDown.Renderer);
            //m_dropDown.RenderMode = ToolStripRenderMode.Custom;

            m_textBox = new MyTextBox(control, m_textBoxArea, allowCustomText ? MyTextBox.InputFormEnum.Text : MyTextBox.InputFormEnum.None, null);
            m_textBox.Font = m_dropDown.Font;
            m_textBox.RequestedAreaChanged += () => { RequestedArea = new SizeF(Area.Width, m_textBox.RequestedArea.Height); };

            m_textBox.TextChanged += s => SelectionChanged.Execute();
            m_textBox.TextChanged += s => { m_itemIndex = -1; m_dropDownWindow = 0; UpdateDropdown(); };
        }

        public override void MouseDown(MouseEventArgs args)
        {
            UpdateDropdown();
            Debug.Print(Name + "MouseDown");
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
            Debug.Print(Name + "Got Focus");
            m_textBox.GotFocus();
            m_textBox.CursorPos = new MyTextBox.CP(int.MaxValue);
            //UpdateDropdown();
        }

        public override void LostFocus()
        {
            Debug.Print(Name + "Lost Focus");
            m_textBox.LostFocus();
            m_dropDown.Close();
            m_textBox.Text = SelectedItem.DisplayString;
        }

        public override void KeyPress(KeyPressEventArgs args)
        {
            bool isEnter = args.KeyChar == '\n' || args.KeyChar == '\r';
            if (!isEnter)
            {
                m_textBox.KeyPress(args);
            }
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
                    SelectedToolStripItem.BackColor = SelectedBackgroundColor;
            }
        }
        private ToolStripItem SelectedToolStripItem
        {
            get
            {
                if (ItemIndex - m_dropDownWindow < 0)
                    return null;
                if (ItemIndex - m_dropDownWindow >= m_dropDown.Items.Count-2)
                    return null;
                return m_dropDown.Items[ItemIndex - m_dropDownWindow + 1];
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

        public override void Paint(Graphics g)
        {
            m_textBox.Paint(g);

            if (m_HasDropDownButton)
                DrawButton(g, m_buttonArea());
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

        private MyComboBoxItem<T> MatchingItem()
        {
            return Items.FirstOrDefault(i => i.DisplayString == m_textBox.Text);
        }

        private MyComboBoxItem<T> m_selectedItem = new MyComboBoxItem<T>("Uninitialized default", default(T));
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

        const int WINDOW_SIZE = 4;
        int m_dropDownWindow = 0;

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
            if ( m_dropDownWindow >= strictSorted.Count - WINDOW_SIZE )
                m_dropDown.Items.OfType<ToolStripItem>().Last().Visible = false;

            m_dropDown.ResumeLayout();
        }

        public override void MouseCaptureChanged()
        {
            m_textBox.MouseCaptureChanged();
        }

        public System.Windows.Forms.ToolStripRenderer Renderer { get { return m_dropDown.Renderer; } set { m_dropDown.Renderer = value; } }
        //public System.Windows.Forms.ToolStripRenderer Renderer { get { return (m_dropDown.Renderer as ToolStripRenderer).Inner; } set { (m_dropDown.Renderer as ToolStripRenderer).Inner = value; } }
        public readonly IEnumerable<MyComboBoxItem<T>> Items;

        public event Action EnterPressed;

        public override bool Contains(PointF point)
        {
            return Area.Contains(point);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utilities;
using Conversation;

namespace ConversationEditor.GUIs.TypeEditors
{
    public partial class EnumerationTypeEditor : UserControl
    {
        public const string TEXT = "Enumeration Type";

        MyTextBox m_name;
        MyComboBox<Or<string, Guid>> m_default;
        ControlSet<SizedControlSet> m_values = new ControlSet<SizedControlSet>();
        ControlSet MyControls;

        private float m_textHeight;
        private float m_textWidth;
        private float m_nameHeight;
        private float m_defaultHeight;

        private void ResetSize()
        {
            m_nameHeight = m_name.RequestedArea.Height;
            m_defaultHeight = m_default.RequestedArea.Height;
            var topHeight = Math.Max(m_nameHeight, m_defaultHeight);

            int elementsBottom = (int)ElementArea(m_values.Controls.Count - 1).Bottom;

            MinimumSize = new Size(drawWindow1.Width + Padding.Horizontal, elementsBottom + Padding.Vertical);
            Size = MinimumSize;
        }

        public EnumerationTypeEditor()
        {
            InitializeComponent();

            using (Graphics g = CreateGraphics())
            {
                var textSize = g.MeasureString(TEXT, Font);
                m_textHeight = textSize.Height;
                m_textWidth = textSize.Width;
            }

            m_name = new MyTextBox(drawWindow1, NameArea, MyTextBox.InputFormEnum.Text);
            m_name.Colors.BorderPen = Colors.ControlBorder;

            m_default = new MyComboBox<Or<string, Guid>>(drawWindow1, DefaultArea, true);
            m_default.Colors.BorderPen = Colors.ControlBorder;
            m_default.Renderer = Colors.ContextMenu;

            drawWindow1.MouseDown += (a, args) => drawWindow1.Focus(); //TODO: is this redundant?
            drawWindow1.Paint += (a, args) => Paint(args.Graphics);
            drawWindow1.GotFocus += (a, b) => { forwardTab.TabStop = false; backwardTab.TabStop = false; };
            forwardTab.GotFocus += (a, b) => { MyControls.ForwardFocus(); drawWindow1.Focus(); }; //Focus draw window so we dont keep giving focus to forwardTab
            backwardTab.GotFocus += (a, b) => { MyControls.BackwardFocus(); drawWindow1.Focus(); }; //Focus draw window so we dont keep giving focus to backwardTab
            this.Leave += (a, b) => { forwardTab.TabStop = true; backwardTab.TabStop = true; };

            forwardTab.Size = Size.Empty;
            forwardTab.Location = new Point(-1, -1);

            backwardTab.Size = Size.Empty;
            backwardTab.Location = new Point(-1, -1);

            MyControls = new ControlSet(m_name, m_default, m_values);
            MyControls.RegisterCallbacks(drawWindow1);
            MyControls.RequestedAreaChanged += ResetSize;

            ResetSize();

            this.PaddingChanged += (a, b) => ResetSize();
        }

        protected override bool ProcessTabKey(bool forward)
        {
            if (MyControls.ProcessTabKey(forward))
                return true;
            else
                return base.ProcessTabKey(forward);
        }

        EnumerationData m_data;
        public EnumerationData Data
        {
            get { return m_data; }
            set
            {
                m_data = value;
                m_name.Text = value.Name;
                m_default.Items.Clear();
                m_default.Items.AddRange(value.Elements.Select(e => new MyComboBox<Or<string, Guid>>.Item(e.Name, e.Guid)));
                m_values.Controls.Clear();
                for (int i = 0; i < m_data.Elements.Count; i++)
                {
                    AddElement(m_data.Elements[i], i);
                }

                SizedControlSet set = null;
                Func<RectangleF> area = () =>
                {
                    var vertical = ElementArea(m_values.Controls.IndexOf(set));
                    return new RectangleF(m_textWidth + 6, vertical.Top, 20, 20);
                };
                Action callback = () =>
                {
                    var element = AddElement(new EnumerationData.Element("", Guid.NewGuid()), m_values.Controls.Count - 1);
                    m_values.FocusOn(element);
                    element.FocusOn(element.Controls.First());
                    ResetSize();
                    drawWindow1.Refresh();
                };
                var btn = new MyPlusButton(area, callback, Colors.ForegroundPen, Colors.BackgroundBrush, true);
                set = new SizedControlSet(() => new SizeF(20,20), btn);
                m_values.Controls.Add(set);

                if (value.Default != null)
                {
                    m_default.SelectedItem = value.Default.Transformed(s => new MyComboBox<Or<string, Guid>>.Item(s), g => new MyComboBox<Or<string, Guid>>.Item(m_data.Elements.Where(e => e.Guid == g).Single().Name, g));
                }
                else
                {
                    m_default.SelectedItem = new MyComboBox<Or<string, Guid>>.Item("");
                }
            }
        }

        private SizedControlSet AddElement(EnumerationData.Element val, int index)
        {
            MyTextBox mtb = null;
            MyButton btn = null;
            SizedControlSet set = null;
            Func<RectangleF> textArea = () =>
            {
                var vertical = ElementArea(m_values.Controls.IndexOf(set));
                return new RectangleF(m_textWidth + 6, vertical.Top, 100, vertical.Height);
            };
            Func<RectangleF> buttonArea = () =>
            {
                var vertical = ElementArea(m_values.Controls.IndexOf(set));
                return new RectangleF(m_textWidth + 6 + 100 + 2, vertical.Top, 20, 20);
            };
            Action callback = () => { m_values.Controls.Remove(set); ResetSize(); drawWindow1.Refresh(); };
            mtb = new MyTextBox(drawWindow1, textArea, MyTextBox.InputFormEnum.Text);
            btn = new MyPlusButton(buttonArea, callback, Colors.ForegroundPen, Colors.BackgroundBrush, false);
            set = new SizedControlSet(() => mtb.RequestedArea, mtb, btn);
            m_values.Controls.Insert(index, set);
            mtb.Text = val.Name;
            return set;
        }

        RectangleF NameArea()
        {
            return new RectangleF(m_textWidth + 6, 0, 100, m_nameHeight);
        }

        RectangleF DefaultArea()
        {
            return new RectangleF(NameArea().Right + 2, 0, 100, m_defaultHeight);
        }

        Rectangle1DF ElementArea(int i)
        {
            var height = i == -1 ? 10 : m_values.Controls[i].RequestedSize.Height;
            var top = i <= 0 ? Math.Max(NameArea().Bottom, DefaultArea().Bottom) : ElementArea(i - 1).Bottom;
            return new Rectangle1DF(top + 2, height);
        }

        private new void Paint(Graphics g)
        {
            g.DrawString(TEXT, Font, Colors.ForegroundBrush, new PointF(3, (drawWindow1.Height - m_textHeight) / 2));
            g.DrawRectangle(Colors.ControlBorder, new Rectangle(0, 0, drawWindow1.Width - 1, drawWindow1.Height - 1));
        }
    }
}

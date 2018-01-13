using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Utilities.UI
{
    public class MyNumericUpDown<T> : MyControl
    {
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
        public RectangleF Area => m_area();

        private MyTextBox m_textBox;

        Func<RectangleF> m_buttonUpArea;
        Func<RectangleF> m_buttonDownArea;
        Func<RectangleF> m_area;
        const int BUTTON_WIDTH = 20;

        public MyTextBox.ColorOptions Colors { get { return m_textBox.Colors; } set { m_textBox.Colors = value; } }

        public MyNumericUpDown(Control control, Func<RectangleF> area, bool decimalAllowed, Font font)
        {
            m_area = area;

            m_buttonUpArea = () => { var a = area(); return new RectangleF(a.Right - BUTTON_WIDTH, a.Top, BUTTON_WIDTH, a.Height / 2); };
            m_buttonDownArea = () => { var a = area(); return new RectangleF(a.Right - BUTTON_WIDTH, a.Top + a.Height / 2, BUTTON_WIDTH, a.Height / 2); };
            Func<RectangleF> m_textBoxArea = () => { var a = area(); return RectangleF.FromLTRB(a.Left, a.Top, a.Right - BUTTON_WIDTH, a.Bottom); };

            m_textBox = new MyTextBox(control, m_textBoxArea, decimalAllowed ? MyTextBox.InputFormEnum.Decimal : MyTextBox.InputFormEnum.Integer, null, x => MyTextBox.ComboBoxBorderDaniel, 4, font);
            m_textBox.RequestedAreaChanged += () => { RequestedArea = new SizeF(Area.Width, m_textBox.RequestedArea.Height); };
            m_textBox.TextChanged += (string oldText) =>
                {
                    if (m_textBox.Text.Length == 0 || m_textBox.Text == "-")
                        m_value = 0;
                    else
                    {
                        if (typeof(T) == typeof(decimal))
                            if (!decimal.TryParse(m_textBox.Text, out m_value))
                                m_textBox.Text = oldText;
                        if (typeof(T) == typeof(int))
                        {
                            if (!int.TryParse(m_textBox.Text, out int value))
                                m_textBox.Text = oldText;
                            else
                                m_value = value;
                        }
                    }
                };

            m_mouseUpTimer.Tick += (a, b) =>
            {
                if (Value <= decimal.MaxValue - Increment)
                    Value += Increment;
                m_mouseUpTimer.Interval = (int)(m_mouseUpTimer.Interval * 0.95).Clamp(1, 10000);
            };
            m_mouseDownTimer.Tick += (a, b) =>
            {
                Value -= Increment;
                m_mouseDownTimer.Interval = (int)(m_mouseDownTimer.Interval * 0.95).Clamp(1, 10000); ;
            };

            Minimum = () => 0;
            Maximum = () => 0;
            Value = 0;
        }

        Timer m_mouseUpTimer = new Timer();
        Timer m_mouseDownTimer = new Timer();
        const int STARTING_INTERVAL = 200;

        public override void MouseDown(MouseEventArgs args)
        {
            if (m_buttonUpArea().Contains(args.Location))
            {
                if (Value <= decimal.MaxValue - Increment)
                    Value += Increment;
                m_mouseUpTimer.Interval = STARTING_INTERVAL;
                m_mouseUpTimer.Start();
            }
            else if (m_buttonDownArea().Contains(args.Location))
            {
                Value -= Increment;
                m_mouseDownTimer.Interval = STARTING_INTERVAL;
                m_mouseDownTimer.Start();
            }
            else if (m_textBox.Area.Contains(args.Location))
            {
                m_textBox.MouseDown(args);
            }
        }

        public override void MouseUp(MouseEventArgs args)
        {
            StopTimers();
            if (m_textBox.Area.Contains(args.Location))
                m_textBox.MouseUp(args);
        }

        public override void MouseMove(MouseEventArgs args)
        {
            if (m_textBox.Area.Contains(args.Location))
                m_textBox.MouseMove(args);
        }

        public override void MouseLeave()
        {
            m_textBox.MouseLeave();
        }

        public override void MouseClick(MouseEventArgs args)
        {
            if (m_textBox.Area.Contains(args.Location))
                m_textBox.MouseClick(args);
        }

        public override void KeyDown(KeyEventArgs args)
        {
            m_textBox.KeyDown(args);
        }

        public override void KeyPress(KeyPressEventArgs args)
        {
            m_textBox.KeyPress(args);
        }

        public override void MouseWheel(MouseEventArgs args)
        {
            Value += args.Delta > 0 ? Increment : -Increment;
        }

        public override void GotFocus()
        {
            m_textBox.GotFocus();
        }

        public override void LostFocus()
        {
            m_textBox.LostFocus();
        }

        public override void MouseCaptureChanged()
        {
            StopTimers();
        }

        public void SetupCallbacks(Control control)
        {
            control.MouseDown += (a, args) => MouseDown(args);
            control.MouseUp += (a, args) => MouseUp(args);
            control.MouseMove += (a, args) => MouseMove(args);
            control.MouseClick += (a, args) => MouseClick(args);
            control.MouseWheel += (a, args) => MouseWheel(args);
            control.KeyPress += (a, args) => KeyPress(args);
            control.KeyDown += (a, args) => KeyDown(args);
            control.GotFocus += (a, args) => GotFocus();
            control.LostFocus += (a, args) => LostFocus();
            control.MouseCaptureChanged += (a, args) => MouseCaptureChanged();
            control.Paint += (a, args) => Paint(args.Graphics);
        }

        private void StopTimers()
        {
            m_mouseUpTimer.Stop();
            m_mouseDownTimer.Stop();
        }

        public override void Paint(Graphics g)
        {
            m_textBox.Paint(g);

            DrawButton(g, m_buttonUpArea(), true);
            DrawButton(g, m_buttonDownArea(), false);
        }

        private void DrawButton(Graphics graphics, RectangleF buttonArea, bool up)
        {
            graphics.PixelOffsetMode = PixelOffsetMode.None;
            graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            graphics.DrawRectangle(Colors.BorderPen, RectangleF.FromLTRB(buttonArea.Left, buttonArea.Top, buttonArea.Right - 1, buttonArea.Bottom - 1));
            using (var path = new GraphicsPath())
            {
                if (up)
                {
                    path.AddLines(new PointF[] { buttonArea.Location.Plus(buttonArea.Width/4,2*buttonArea.Height/3),
                                         buttonArea.Location.Plus(3*buttonArea.Width/4, 2*buttonArea.Height/3),
                                         buttonArea.Location.Plus(buttonArea.Width/2, buttonArea.Height/3)});
                }
                else
                {
                    path.AddLines(new PointF[] { buttonArea.Location.Plus(buttonArea.Width/4,buttonArea.Height/3),
                                         buttonArea.Location.Plus(3*buttonArea.Width/4, buttonArea.Height/3),
                                         buttonArea.Location.Plus(buttonArea.Width/2, 2*buttonArea.Height/3)});
                }
                graphics.FillPath(Colors.TextBrush, path);
            }
        }

        public bool SpecialEnter { get { return m_textBox.SpecialEnter; } set { m_textBox.SpecialEnter = value; } }
        public event Action EnterPressed { add { m_textBox.EnterPressed += value; } remove { m_textBox.EnterPressed -= value; } }

        private decimal m_value = decimal.MinValue;
        public decimal Value
        {
            get { return m_value; }
            set
            {
                value = Util.Clamp(value, Minimum(), Maximum());
                if (value != m_value)
                {
                    m_value = value;
                    m_textBox.Text = m_value.ToString(CultureInfo.CurrentCulture);
                    m_textBox.SetCursorPosition(int.MaxValue);
                }
            }
        }

        public Func<decimal> Minimum { get; set; }
        public Func<decimal> Maximum { get; set; }

        public decimal Increment { get; set; } = 1;

        public override bool Contains(PointF point)
        {
            return Area.Contains(point);
        }
    }
}

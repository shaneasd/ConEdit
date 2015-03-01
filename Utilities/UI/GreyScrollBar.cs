using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Utilities;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace Utilities
{
    public partial class GreyScrollBar : UserControl
    {
        public static Bitmap ScrollbarUpIcon;
        public static Bitmap ScrollbarUpPressedIcon;
        public static Bitmap ScrollbarBackgroundVerticalIcon;
        public static Bitmap FolderIcon;

        public static Bitmap ScrollbarMiddleIcon;
        public static Bitmap ScrollbarMiddlePressedIcon;
        public static Bitmap ScrollbarTopIcon;
        public static Bitmap ScrollbarTopPressedIcon;
        public static Bitmap ScrollbarBottomIcon;
        public static Bitmap ScrollbarBottomPressedIcon;

        static GreyScrollBar()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarUp.png"))
                ScrollbarUpIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarUpPressed.png"))
                ScrollbarUpPressedIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarBackgroundVertical.png"))
                ScrollbarBackgroundVerticalIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarMiddle.png"))
                ScrollbarMiddleIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarMiddlePressed.png"))
                ScrollbarMiddlePressedIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarTop.png"))
                ScrollbarTopIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarTopPressed.png"))
                ScrollbarTopPressedIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarBottom.png"))
                ScrollbarBottomIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarBottomPressed.png"))
                ScrollbarBottomPressedIcon = new Bitmap(stream);
        }

        //TODO: Make sure greyscrollbars have the right backcolor
        //protected override void OnBackColorChanged(EventArgs e)
        //{
        //    BackColor = ColorScheme.FormBackground;
        //}

        const int BUTTON_SIZE = 15;
        public GreyScrollBar()
        {
            MinimumSize = new Size(BUTTON_SIZE, BUTTON_SIZE);
            Width = BUTTON_SIZE;
            InitializeComponent();
            Action CreateButtons = () =>
                {
                    //TODO: ColorScheme.FormBackground
                    TopButton = new Button(new Point(0, 0), !Horizontal ? Button.Direction.Up : Button.Direction.Left, () => m_state, () => BackColor);
                    var location = Horizontal ? new Point(Width - BUTTON_SIZE, 0) : new Point(0, Height - BUTTON_SIZE);
                    BottomButton = new Button(location, !Horizontal ? Button.Direction.Down : Button.Direction.Right, () => m_state, () => BackColor);
                };
            CreateButtons();
            SizeChanged += (a, b) => CreateButtons();
            drawWindow1.MouseDown += (a, b) => drawWindowMouseDown(b);
            drawWindow1.MouseUp += (a, b) => drawWindowMouseUp();
            drawWindow1.LostFocus += (a, b) => drawWindowMouseUp();
            drawWindow1.MouseMove += (a, b) => drawWindowMouseMove(b);

            timer1.Tick += (a, b) => smallScrollUpIncrement();
            timer2.Tick += (a, b) => smallScrollDownIncrement();
            timer3.Tick += (a, b) => largeScrollUpIncrement();
            timer4.Tick += (a, b) => largeScrollDownIncrement();
        }

        Button TopButton;
        Button BottomButton;

        public event Action Scrolled;

        public bool Horizontal { get; set; }

        float m_value;
        private float RatioValue
        {
            get { return m_value; }
            set
            {
                value = value.Clamp(0.0f, 1.0f);
                if (value != m_value)
                {
                    m_value = value;
                    Scrolled.Execute();
                    Invalidate(true);
                }
            }
        }
        float m_largeIncremement = 0.1f;
        [DefaultValue(0.1f)]
        public float LargeChange
        {
            get { return m_largeIncremement; }
            set { m_largeIncremement = value; }
        }

        float m_smallIncrement = 0.01f;
        [DefaultValue(0.01f)]
        public float SmallChange
        {
            get { return m_smallIncrement; }
            set { m_smallIncrement = value; }
        }

        public float Minimum
        {
            get;
            set;
        }

        public float Maximum
        {
            get;
            set;
        }

        public float Value
        {
            get { return RatioValue * (Maximum - Minimum) + Minimum; }
            set { RatioValue = (value - Minimum) / (Maximum - Minimum); }
        }

        /// <summary>
        /// What percentage of the associated document is visible [0, 1]
        /// Dictates the size of the bar. If the document is 50% visible then the bar should occupy 50% of the range
        /// </summary>
        private float m_percentageCovered = 1;
        public float PercentageCovered
        {
            get { return m_percentageCovered; }
            set { m_percentageCovered = value; Invalidate(true); }
        }

        public enum StateEnum { None, HoldingScrollDownButton, HoldingScrollUpButton, Dragging, EmptyScrollDown, EmptyScrollUp }
        StateEnum m_state;
        float m_dragStart;
        float m_dragValue;

        private float Distance(Point loc)
        {
            return Horizontal ? loc.X : loc.Y;
        }

        void drawWindowMouseDown(MouseEventArgs e)
        {
            Point loc = e.Location;
            float locDist = Distance(loc);
            if (TopButton.Area.Contains(loc))
            {
                m_state = StateEnum.HoldingScrollUpButton;
                smallScrollUpIncrement();
                timer1.Start();
            }
            else if (BottomButton.Area.Contains(loc))
            {
                m_state = StateEnum.HoldingScrollDownButton;
                smallScrollDownIncrement();
                timer2.Start();
            }
            else if (locDist >= LowHeight && locDist <= HighHeight)
            {
                m_state = StateEnum.Dragging;
                m_dragStart = locDist;
                m_dragValue = RatioValue;
            }
            else if (locDist > HighHeight)
            {
                m_state = StateEnum.EmptyScrollDown;
                timer4.Start();
            }
            else if (locDist < LowHeight)
            {
                m_state = StateEnum.EmptyScrollUp;
                timer3.Start();
            }
            Invalidate(true);
        }

        void smallScrollUpIncrement()
        {
            RatioValue -= SmallChange;
            Invalidate(true);
        }

        void smallScrollDownIncrement()
        {
            RatioValue += SmallChange;
            Invalidate(true);
        }

        private void largeScrollDownIncrement()
        {
            var loc = drawWindow1.PointToClient(MousePosition);
            if (Distance(loc) > HighHeight)
            {
                RatioValue += LargeChange;
                Invalidate(true);
            }
        }

        private void largeScrollUpIncrement()
        {
            var loc = drawWindow1.PointToClient(MousePosition);
            if (Distance(loc) < LowHeight)
            {
                RatioValue -= LargeChange;
                Invalidate(true);
            }
        }

        void drawWindowMouseUp()
        {
            m_state = StateEnum.None;
            Invalidate(true);
            timer1.Stop();
            timer2.Stop();
            timer3.Stop();
            timer4.Stop();
        }

        private void drawWindowMouseMove(MouseEventArgs e)
        {
            if (m_state == StateEnum.Dragging)
            {
                float loc = Distance(e.Location);
                float movementY = loc - m_dragStart;
                RatioValue = m_dragValue + movementY / (Range - BarLength);
            }
        }

        private class Button
        {
            public enum Direction { Up, Down, Left, Right }
            private Rectangle m_rectangle;
            private Direction m_direction;
            Func<StateEnum> m_state;
            private Func<Color> m_backColor;
            public Button(Point location, Direction direction, Func<StateEnum> state, Func<Color> backColor)
            {
                m_rectangle = new Rectangle(location, new Size(BUTTON_SIZE, BUTTON_SIZE));
                m_direction = direction;
                m_state = state;
                m_backColor = backColor;
            }

            public Rectangle Area { get { return m_rectangle; } }

            public void Draw(Graphics g, bool pushed)
            {
                var state = g.Save();
                var m = g.Transform;
                switch (m_direction)
                {
                    case Direction.Up:
                        break;
                    case Direction.Down:
                        m.Scale(1, -1);
                        m.Translate(0, -Area.Y * 2 - Area.Height);
                        break;
                    case Direction.Left:
                        m.RotateAt(90, ((RectangleF)Area).Center());
                        m.Scale(1, -1);
                        m.Translate(0, -Area.Height);
                        break;
                    case Direction.Right:
                        m.RotateAt(90, ((RectangleF)Area).Center());
                        break;
                }
                g.Transform = m;
                using (var brush = new SolidBrush(m_backColor()))
                    g.FillRectangle(brush, Area);
                g.DrawImage(pushed ? GreyScrollBar.ScrollbarUpPressedIcon : GreyScrollBar.ScrollbarUpIcon, Area);
                g.Restore(state);
            }
        }

        public float Length
        {
            get
            {
                if (Horizontal)
                    return Width;
                else
                    return Height;
            }
        }

        public float Range
        {
            get
            {
                return Length - BUTTON_SIZE - BUTTON_SIZE;
            }
        }

        public float BarLength
        {
            get
            {
                return (PercentageCovered * Range).Clamp(BUTTON_SIZE, Range);
                //return (LargeChange * Range).Clamp(BUTTON_SIZE, Range);
            }
        }

        public float LowHeight
        {
            get
            {
                return RatioValue * (Range - BarLength) + BUTTON_SIZE-1;
            }
        }

        public float HighHeight
        {
            get
            {
                return RatioValue * (Range - BarLength) + TopButton.Area.Height + BarLength+1;
            }
        }

        private void drawWindow1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

            //e.Graphics.DrawRectangle(new Pen(Colors.ControlBorder, 1), new Rectangle(new Point(0, 0), new Size(Width - 1, Height - 1)));
            TopButton.Draw(e.Graphics, m_state == StateEnum.HoldingScrollUpButton);
            BottomButton.Draw(e.Graphics, m_state == StateEnum.HoldingScrollDownButton);

            if (Horizontal)
            {
                var state = e.Graphics.Save();
                e.Graphics.RotateTransform(90);
                e.Graphics.DrawImage(ScrollbarBackgroundVerticalIcon, Rectangle.FromLTRB(0, -Width + BUTTON_SIZE, BUTTON_SIZE, -BUTTON_SIZE));
                e.Graphics.DrawImage(ScrollbarBackgroundVerticalIcon, Rectangle.FromLTRB(0, BUTTON_SIZE, Width, Height - BUTTON_SIZE));
                e.Graphics.Restore(state);
            }
            else
            {
                e.Graphics.DrawImage(ScrollbarBackgroundVerticalIcon, Rectangle.FromLTRB(0, BUTTON_SIZE, Width, Height - BUTTON_SIZE));
            }

            using (Bitmap scrollbar = new Bitmap(ScrollbarTopIcon.Width, (int)(HighHeight - LowHeight)))
            {
                using (Graphics g = Graphics.FromImage(scrollbar))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(m_state == StateEnum.Dragging ? ScrollbarTopPressedIcon : ScrollbarTopIcon, new RectangleF(new PointF(0, 0), ScrollbarTopIcon.Size));
                    g.DrawImage(m_state == StateEnum.Dragging ? ScrollbarMiddlePressedIcon : ScrollbarMiddleIcon, new RectangleF(new PointF(0, 0 + ScrollbarTopIcon.Height), new SizeF(ScrollbarMiddleIcon.Width, HighHeight - ScrollbarBottomIcon.Height - LowHeight - ScrollbarTopIcon.Height)));
                    g.DrawImage(m_state == StateEnum.Dragging ? ScrollbarBottomPressedIcon : ScrollbarBottomIcon, new RectangleF(new PointF(0, scrollbar.Height - ScrollbarBottomIcon.Height), ScrollbarBottomIcon.Size));
                }
                if (Horizontal)
                {
                    var area = RectangleF.FromLTRB(LowHeight, 0, HighHeight + 1, Height - 1);
                    var state = e.Graphics.Save();
                    var m = e.Graphics.Transform;
                    m.RotateAt(90, Point.Empty);
                    e.Graphics.Transform = m;
                    e.Graphics.DrawImage(scrollbar, RectangleF.FromLTRB(0, -LowHeight, BUTTON_SIZE, -HighHeight));
                    e.Graphics.Restore(state);
                }
                else
                {
                    e.Graphics.DrawImage(scrollbar, new RectangleF(new PointF(0, LowHeight), scrollbar.Size));
                }
            }
        }
    }
}

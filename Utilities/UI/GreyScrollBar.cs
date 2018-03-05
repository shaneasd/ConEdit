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

namespace Utilities.UI
{
    public partial class GreyScrollBar : UserControl
    {
        public interface IColorScheme
        {
            Color Background { get; }
            DrawWindow.IColorScheme DrawWindowColorScheme { get; }
        }

        public class DefaultColorScheme : IColorScheme
        {
            public Color Background { get; } = Color.FromArgb(56, 56, 56);

            public DrawWindow.IColorScheme DrawWindowColorScheme => new DrawWindow.DefaultColorScheme();
        }

        public static Bitmap ScrollBarUpIcon { get; }
        public static Bitmap ScrollBarUpPressedIcon { get; }
        public static Bitmap ScrollBarBackgroundVerticalIcon { get; }
        public static Bitmap FolderIcon { get; }

        public static Bitmap ScrollBarMiddleIcon { get; }
        public static Bitmap ScrollBarMiddlePressedIcon { get; }
        public static Bitmap ScrollBarTopIcon { get; }
        public static Bitmap ScrollBarTopPressedIcon { get; }
        public static Bitmap ScrollBarBottomIcon { get; }
        public static Bitmap ScrollBarBottomPressedIcon { get; }

        public static Bitmap ScrollBarReduceIcon { get; }
        public static Bitmap ScrollBarReducePressedIcon { get; }
        public static Bitmap ScrollBarIncreaseIcon { get; }
        public static Bitmap ScrollBarIncreasePressedIcon { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "I believe this would actually be slower due to the inability to cache Assembly.GetExecutingAssembly()")]
        static GreyScrollBar()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarUp.png"))
                ScrollBarUpIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarUpPressed.png"))
                ScrollBarUpPressedIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarBackgroundVertical.png"))
                ScrollBarBackgroundVerticalIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarMiddle.png"))
                ScrollBarMiddleIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarMiddlePressed.png"))
                ScrollBarMiddlePressedIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarTop.png"))
                ScrollBarTopIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarTopPressed.png"))
                ScrollBarTopPressedIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarBottom.png"))
                ScrollBarBottomIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ScrollbarBottomPressed.png"))
                ScrollBarBottomPressedIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ButtonCollapse15x15.png"))
                ScrollBarReduceIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ButtonCollapse15x15Pressed.png"))
                ScrollBarReducePressedIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ButtonExpand15x15.png"))
                ScrollBarIncreaseIcon = new Bitmap(stream);
            using (Stream stream = assembly.GetManifestResourceStream("Utilities.UI.Resources.ButtonExpand15x15Pressed.png"))
                ScrollBarIncreasePressedIcon = new Bitmap(stream);
        }

        void CreateButtons()
        {
            TopButton = new Button(new Point(0, 0), !Arrows ? Button.Direction.Decrease : (!Horizontal ? Button.Direction.Up : Button.Direction.Left), () => Color.Transparent);
            var location = Horizontal ? new Point(Width - BUTTON_SIZE, 0) : new Point(0, Height - BUTTON_SIZE);
            BottomButton = new Button(location, !Arrows ? Button.Direction.Increase : (!Horizontal ? Button.Direction.Down : Button.Direction.Right), () => Color.Transparent);
        }

        const int BUTTON_SIZE = 15;
        public GreyScrollBar()
        {
            MinimumSize = new Size(BUTTON_SIZE, BUTTON_SIZE);
            Width = BUTTON_SIZE;
            InitializeComponent();
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

        public int MinRenderHeight
        {
            get
            {
                if (Horizontal)
                    return Math.Max(TopButton.Area.Height, BottomButton.Area.Height);
                else
                    return TopButton.Area.Height + BottomButton.Area.Height + BUTTON_SIZE;
            }
        }

        public event Action Scrolled;

        private bool m_horizontal = false;
        public bool Horizontal
        {
            get { return m_horizontal; }
            set { m_horizontal = value; CreateButtons(); }
        }

        private bool m_arrows = true;
        public bool Arrows
        {
            get { return m_arrows; }
            set { m_arrows = value; CreateButtons(); }
        }
        
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
        public float LargeChange { get; set; } = 0.1f;

        public float SmallChange { get; set; } = 0.01f;

        public float Minimum { get; set; }

        public float Maximum { get; set; }

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

        //private IColorScheme m_colorScheme = new DefaultColorScheme();
        //[Browsable(false)]
        //public IColorScheme ColorScheme
        //{
        //    get { return m_colorScheme; }
        //    set
        //    {
        //        m_colorScheme = value;
        //        drawWindow1.ColorScheme = value.DrawWindowColorScheme;
        //    }
        //}

        private enum StateEnum { None, HoldingScrollDownButton, HoldingScrollUpButton, Dragging, EmptyScrollDown, EmptyScrollUp }
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
            public enum Direction { Up, Down, Left, Right, Increase, Decrease }
            private Rectangle m_rectangle;
            private Direction m_direction;
            private Func<Color> m_backColor;
            public Button(Point location, Direction direction, Func<Color> backColor)
            {
                m_rectangle = new Rectangle(location, new Size(BUTTON_SIZE, BUTTON_SIZE));
                m_direction = direction;
                m_backColor = backColor;
            }

            public Rectangle Area => m_rectangle;

            public void Draw(Graphics g, bool pushed)
            {
                var state = g.Save();
                var m = g.Transform;
                Bitmap image = pushed ? GreyScrollBar.ScrollBarUpPressedIcon : GreyScrollBar.ScrollBarUpIcon;
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
                    case Direction.Increase:
                        image = pushed ? GreyScrollBar.ScrollBarIncreasePressedIcon : GreyScrollBar.ScrollBarIncreaseIcon;
                        break;
                    case Direction.Decrease:
                        image = pushed ? GreyScrollBar.ScrollBarReducePressedIcon : GreyScrollBar.ScrollBarReduceIcon;
                        break;
                }
                g.Transform = m;
                using (var brush = new SolidBrush(m_backColor()))
                    g.FillRectangle(brush, Area);
                g.DrawImage(image, Area);
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

        public float Range => Length - BUTTON_SIZE - BUTTON_SIZE;

        public float BarLength => (PercentageCovered * Range).Clamp(BUTTON_SIZE, Range);//return (LargeChange * Range).Clamp(BUTTON_SIZE, Range);

        public float LowHeight => RatioValue * (Range - BarLength) + BUTTON_SIZE - 1;

        public float HighHeight => RatioValue * (Range - BarLength) + TopButton.Area.Height + BarLength + 1;

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
                e.Graphics.DrawImage(ScrollBarBackgroundVerticalIcon, Rectangle.FromLTRB(0, -Width + BUTTON_SIZE, BUTTON_SIZE, -BUTTON_SIZE));
                e.Graphics.DrawImage(ScrollBarBackgroundVerticalIcon, Rectangle.FromLTRB(0, BUTTON_SIZE, Width, Height - BUTTON_SIZE));
                e.Graphics.Restore(state);
            }
            else
            {
                e.Graphics.DrawImage(ScrollBarBackgroundVerticalIcon, Rectangle.FromLTRB(0, BUTTON_SIZE, Width, Height - BUTTON_SIZE));
            }

            using (Bitmap scrollbar = new Bitmap(ScrollBarTopIcon.Width, (int)(HighHeight - LowHeight)))
            {
                using (Graphics g = Graphics.FromImage(scrollbar))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(m_state == StateEnum.Dragging ? ScrollBarTopPressedIcon : ScrollBarTopIcon, new RectangleF(new PointF(0, 0), ScrollBarTopIcon.Size));
                    g.DrawImage(m_state == StateEnum.Dragging ? ScrollBarMiddlePressedIcon : ScrollBarMiddleIcon, new RectangleF(new PointF(0, 0 + ScrollBarTopIcon.Height), new SizeF(ScrollBarMiddleIcon.Width, HighHeight - ScrollBarBottomIcon.Height - LowHeight - ScrollBarTopIcon.Height)));
                    g.DrawImage(m_state == StateEnum.Dragging ? ScrollBarBottomPressedIcon : ScrollBarBottomIcon, new RectangleF(new PointF(0, scrollbar.Height - ScrollBarBottomIcon.Height), ScrollBarBottomIcon.Size));
                }
                if (Horizontal)
                {
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

        public void MouseWheeled(MouseEventArgs e)
        {
            const double WHEEL_SCALE = 0.25;
            Value -= (int)(e.Delta * WHEEL_SCALE);
        }
    }
}

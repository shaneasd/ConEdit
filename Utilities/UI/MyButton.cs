using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace Utilities.UI
{
    public class MyButton : MyControl
    {
        public MyButton(Func<RectangleF> area, Action callback)
        {
            m_area = area;
            m_callback = callback;
        }

        public override void MouseDown(System.Windows.Forms.MouseEventArgs args)
        {
        }

        public override void MouseUp(System.Windows.Forms.MouseEventArgs args)
        {
        }

        public override void MouseMove(System.Windows.Forms.MouseEventArgs args)
        {
        }

        public override void MouseClick(System.Windows.Forms.MouseEventArgs args)
        {
            m_callback();
        }

        public override void KeyDown(System.Windows.Forms.KeyEventArgs args)
        {
        }

        public override void KeyPress(System.Windows.Forms.KeyPressEventArgs args)
        {
        }

        public override void MouseWheel(System.Windows.Forms.MouseEventArgs args)
        {
        }

        public override void GotFocus()
        {
        }

        public override void LostFocus()
        {
        }

        public override void MouseCaptureChanged()
        {
        }

        public override void Paint(System.Drawing.Graphics g)
        {
        }

        public System.Drawing.RectangleF Area
        {
            get { return m_area(); }
        }

        public virtual System.Drawing.SizeF RequestedArea
        {
            get { return m_area().Size; }
        }

        public override event Action RequestedAreaChanged { add { } remove { } }
        private Func<RectangleF> m_area;
        private Action m_callback;

        public override bool Contains(PointF point)
        {
            return Area.Contains(point);
        }
    }

    public class MyPlusButton : MyButton
    {
        private Pen Foreground;
        private Brush Background;
        private bool m_plus;
        public MyPlusButton(Func<RectangleF> area, Action callback, Pen foreground, Brush background, bool plus)
            : base(area, callback)
        {
            Foreground = foreground;
            Background = background;
            m_plus = plus;
        }

        public override void Paint(Graphics g)
        {
            base.Paint(g);
            g.FillRectangle(Background, Area);
            g.DrawRectangle(Foreground, Area);
            g.DrawLine(Foreground, new PointF(Area.Left + 2, Area.Y + Area.Height / 2), new PointF(Area.Right - 2, Area.Y + Area.Height / 2));
            if (m_plus)
                g.DrawLine(Foreground, new PointF(Area.Left + Area.Width / 2, Area.Top + 2), new PointF(Area.Right - Area.Width / 2, Area.Bottom - 2));
        }
    }

    public class HighlightableImageButton : MyButton
    {
        public Pen SelectionPen { get; set; }
        public Brush HighlightBackground { get; set; }
        private Image Image;

        private bool m_highlighted;
        public bool Highlighted
        {
            get { return m_highlighted; }
            set
            {
                if (m_highlighted != value)
                {
                    m_highlighted = value;
                    ValueChanged.Execute();
                }
            }
        }

        public event Action ValueChanged;

        public static HighlightableImageButton Create(Func<RectangleF> area, Pen selectionPen, Brush highlightBackground, Image image)
        {
            HighlightableImageButton result = null;
            result = new HighlightableImageButton(area, () => { result.Highlighted = !result.Highlighted; }, selectionPen, highlightBackground, image);
            return result;
        }

        private HighlightableImageButton(Func<RectangleF> area, Action callback, Pen selectionPen, Brush highlightBackground, Image image)
            : base(area, callback)
        {
            SelectionPen = selectionPen;
            HighlightBackground = highlightBackground;
            Image = image;
        }

        private static ImageAttributes MakeNonHighlightedImageAttributes()
        {
            var mat = new ColorMatrix(new[] { new float[] { 1, 0, 0, 0, 0 }, new float[] { 0, 1, 0, 0, 0 }, new float[] { 0, 0, 1, 0, 0 }, new float[] { 0, 0, 0, 0.3f, 0 }, new float[] { 0, 0, 0, 0, 1 } });
            ImageAttributes attr = new ImageAttributes();
            try
            {
                attr.SetColorMatrix(mat);
            }
            catch
            {
                attr.Dispose();
                throw;
            }
            return attr;
        }

        ImageAttributes NonHighlightedImageAttributes = MakeNonHighlightedImageAttributes();

        public override void Paint(Graphics g)
        {
            base.Paint(g);
            //Test for Daniel
            //if (Highlighted)
            //    g.FillRectangle(HighlightBackground, Area);
            //g.DrawImage(Image, (int)Area.X + 2, (int)Area.Y + 2, Image.Width, Image.Height);
            //if (Highlighted)
            //    g.DrawRectangle(SelectionPen, Area);
            if (Highlighted)
            {
                g.DrawImage(Image, (int)Area.X + 2, (int)Area.Y + 2, Image.Width, Image.Height);
            }
            else
            {
                g.DrawImage(Image, new Rectangle((int)Area.X + 2, (int)Area.Y + 2, Image.Width, Image.Height), 0, 0, Image.Width, Image.Height, GraphicsUnit.Pixel, NonHighlightedImageAttributes);
            }
        }
    }

    public class CrossButton : MyButton
    {
        public Pen Foreground { get; set; }
        public Brush Background { get; set; }
        public CrossButton(Func<RectangleF> area, Action callback, Pen foreground, Brush background)
            : base(area, callback)
        {
            Foreground = foreground;
            Background = background;
        }

        public override void Paint(Graphics g)
        {
            base.Paint(g);
            g.FillRectangle(Background, Area);
            g.DrawRectangle(Foreground, Area);

            float top = Area.Top + 2;
            float bottom = Area.Bottom - 2;
            float left = Area.Left + 2;
            float right = Area.Right - 2;

            g.DrawLine(Foreground, new PointF(left, top), new PointF(right, bottom));
            g.DrawLine(Foreground, new PointF(left, bottom), new PointF(right, top));
        }
    }

    public class GenericButton : MyButton
    {
        private Action<RectangleF, Graphics> m_draw;
        public GenericButton(Func<RectangleF> area, Action<RectangleF, Graphics> draw, Action callback) : base(area, callback)
        {
            m_draw = draw;
        }

        public override void Paint(Graphics g)
        {
            base.Paint(g);
            m_draw(Area, g);
        }
    }
}


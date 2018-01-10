using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.UI
{
    public interface IBorderDrawer
    {
        Color BackColor { get; }
        void Draw(Graphics g, Rectangle area);
    }

    public class SimpleTextBoxBorderDrawer : IBorderDrawer
    {
        private MyTextBox m_textBox;

        public SimpleTextBoxBorderDrawer(MyTextBox textBox)
        {
            m_textBox = textBox;
        }

        public Color BackColor => m_textBox.Colors.Background;
        public void Draw(Graphics g, Rectangle area)
        {
            MyTextBox.ColorOptions colors = m_textBox.Colors;
            g.FillRectangle(colors.BackgroundBrush, area);
            g.DrawRectangle(colors.BorderPen, new Rectangle(area.X, area.Y, area.Width - 1, area.Height - 1));
        }
    }

    public class ImageBorderDrawer : IBorderDrawer
    {
        private Bitmap TopLeftCorner { get; }
        private Bitmap TopRightCorner { get; }
        private Bitmap BottomRightCorner { get; }
        private Bitmap BottomLeftCorner { get; }
        private Brush TopEdge { get; }
        private Brush RightEdge { get; }
        private Brush BottomEdge { get; }
        private Brush LeftEdge { get; }
        private SolidBrush Middle { get; }
        private int m_border;

        public Color BackColor { get; }

        public ImageBorderDrawer(Bitmap source, int border)
        {
            m_border = border;

            Bitmap MakeSubImage(Rectangle bounds)
            {
                Bitmap bmp = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
                using (var graphics = Graphics.FromImage(bmp))
                    graphics.DrawImage(source, new Rectangle(new Point(-bounds.X, -bounds.Y), source.Size));
                return bmp;
            }
            TopLeftCorner = MakeSubImage(new Rectangle(0, 0, border, border));
            TopEdge = new TextureBrush(MakeSubImage(new Rectangle(border, 0, 1, border)), System.Drawing.Drawing2D.WrapMode.Tile);
            TopRightCorner = MakeSubImage(new Rectangle(source.Width - border, 0, border, border));
            RightEdge = new TextureBrush(MakeSubImage(new Rectangle(source.Width - border, border, border, 1)), System.Drawing.Drawing2D.WrapMode.Tile);
            BottomRightCorner = MakeSubImage(new Rectangle(source.Width - border, source.Height - border, border, border));
            BottomEdge = new TextureBrush(MakeSubImage(new Rectangle(border, source.Height - border, 1, border)), System.Drawing.Drawing2D.WrapMode.Tile);
            BottomLeftCorner = MakeSubImage(new Rectangle(0, source.Height - border, border, border));
            LeftEdge = new TextureBrush(MakeSubImage(new Rectangle(0, border, border, 1)), System.Drawing.Drawing2D.WrapMode.Tile);
            BackColor = source.GetPixel(border + 1, border + 1);
            Middle = new SolidBrush(BackColor);
        }

        public void Draw(Graphics g, Rectangle area)
        {
            g.FillRectangle(Middle, Rectangle.FromLTRB(area.Left + m_border, area.Top + m_border, area.Right - m_border, area.Bottom - m_border));
            g.DrawImage(TopLeftCorner, new Rectangle(0, 0, m_border, m_border));
            g.FillRectangle(TopEdge, new Rectangle(m_border, 0, area.Width - m_border * 2, m_border));
            g.FillRectangle(LeftEdge, new Rectangle(0, m_border, m_border, area.Height - m_border * 2));
            using (g.SaveState())
            {
                g.TranslateTransform(area.Width - m_border, 0);
                g.DrawImage(TopRightCorner, new Rectangle(0, 0, m_border, m_border));
                g.FillRectangle(RightEdge, new Rectangle(0, m_border, m_border, area.Height - m_border * 2));
                g.TranslateTransform(0, area.Height - m_border);
                g.DrawImage(BottomRightCorner, new Rectangle(0, 0, m_border, m_border));
            }
            using (g.SaveState())
            {
                g.TranslateTransform(0, area.Height - m_border);
                g.FillRectangle(BottomEdge, new Rectangle(m_border, 0, area.Width - m_border * 2, m_border));
                g.DrawImage(BottomLeftCorner, new Rectangle(0, 0, m_border, m_border));
            }
        }
    }
}

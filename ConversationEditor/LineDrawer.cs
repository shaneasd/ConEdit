using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Utilities;
using System.Drawing.Drawing2D;

namespace ConversationEditor
{
    public class LineDrawer
    {
        public LineDrawer(ColorScheme scheme)
        {
            m_scheme = scheme;
            Outline = new Pen(new SolidBrush(Color.FromArgb(128, scheme.Connectors)), 2);
            SelectedOutline = new Pen(new SolidBrush(scheme.SelectedConnectors), 2);
        }
        private readonly Pen Outline;
        public readonly Pen SelectedOutline;
        private ColorScheme m_scheme;

        public void ConnectPoints(Graphics g, PointF p1, PointF p2, bool selected, IEnumerable<RectangleF> Obstacles = null)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            //TODO: Use Obstacles
            if (p1.Y <= p2.Y || Obstacles == null)
            {
                //g.DrawCurve(Outline, new Point[] { p1, new Point(p1.X, p1.Y + 10), new Point(p2.X, p2.Y - 10), p2 }, 0.1f);
                //DrawCurvedLine(g, new Point[] { p1, new Point(p1.X, (p1.Y + p2.Y) / 2), new Point(p2.X, (p1.Y + p2.Y)/2), p2 }, 20);
                DrawCurvyLine(g, p1, p2, selected);
            }
            else
            {
                DrawCurvyLine(g, p1, p2, selected, horizontal: false);
                //DrawReachAround(g, p1, p2, Obstacles);
            }
        }

        public static void DrawPoint(Graphics g, float x, float y)
        {
            g.DrawLine(Pens.Green, new PointF(x - 1, y), new PointF(x + 1, y));
            g.DrawLine(Pens.Green, new PointF(x, y - 1), new PointF(x, y + 1));
        }

        public void DrawCurvedLine(Graphics g, PointF[] points, int radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                PointF lastQ = new PointF(0, 0);
                PointF nextStart = new PointF(points[0].X, points[0].Y);
                for (int i = 0; i < points.Length - 2; i++)
                {
                    PointF Q = GetQ(points[i], points[i + 1], points[i + 2], radius);
                    PointF A = points[i + 1].Take(points[i]).Normalised();
                    PointF B = points[i + 2].Take(points[i + 1]).Normalised();

                    float toLeave = Math.Abs(Q.Dot(A));
                    PointF toDraw = A.ScaleBy(points[i].DistanceTo(points[i + 1]) - toLeave);

                    path.AddLine(nextStart.X, nextStart.Y, toDraw.X + points[i].X, toDraw.Y + points[i].Y);

                    nextStart = B.ScaleBy(Math.Abs(Q.Dot(B))).Plus(points[i + 1]);

                    //path.AddBezier(toDraw.X + points[i].X, toDraw.Y + points[i].Y, points[i + 1].X, points[i + 1].Y, points[i + 1].X, points[i + 1].Y, nextStart.X, nextStart.Y);

                    var arcLocation = Q.Plus(points[i + 1]).Plus(-radius, -radius);

                    float a1 = (float)(Math.Atan2(A.Y, A.X) * 180 / Math.PI);
                    float a2 = (float)(Math.Atan2(B.Y, B.X) * 180 / Math.PI);

                    float dif21 = a2 - a1; if (dif21 < 0) dif21 += 360;
                    float dif12 = a1 - a2; if (dif12 < 0) dif12 += 360;

                    path.AddArc(arcLocation.X, arcLocation.Y, radius * 2, radius * 2, a1 + dif12, Math.Abs(-dif12) < Math.Abs(dif21) ? -dif12 : dif21);
                    //path.AddArc(arcLocation.X, arcLocation.Y, radius, radius, a1, dif12);

                    lastQ = Q;
                }
                path.AddLine(path.GetLastPoint(), new PointF(points[points.Length - 1].X, points[points.Length - 1].Y));
                //g.DrawLines(Outline, points);
                //for (int i = 1; i < points.Length - 1; i++)
                //DrawArc(g, points[i - 1], points[i], points[i + 1], radius);

                g.DrawPath(Outline, path);
            }
        }

        public static void DrawArc(Graphics g, PointF p1, PointF p2, PointF p3, int radius)
        {
            PointF Q = GetQ(p1, p2, p3, radius);

            DrawPoint(g, p2.X + Q.X, p2.Y + Q.Y);
        }

        private static PointF GetQ(PointF p1, PointF p2, PointF p3, int radius)
        {
            PointF A = p1.Take(p2).Normalised();
            PointF B = p3.Take(p2).Normalised();
            PointF ApB = A.Plus(B);
            float apbls = ApB.LengthSquared();
            float hs = apbls * radius * radius / (float)(apbls - (1 - A.Dot(B)));
            float l = (float)Math.Sqrt(hs / apbls);
            PointF Q = new PointF(ApB.X * l, ApB.Y * l);
            return Q;
        }

        private void DrawReachAround(Graphics g, PointF p1, PointF p2, IEnumerable<Rectangle> Obstacles)
        {
            var a = Obstacles.ToArray();
            Rectangle obstacle = a[0];
            foreach (var b in a)
                obstacle = Rectangle.Union(obstacle, b);

            int BUFFER = 20;
            //BUFFER += (p1.X * 1951 + p1.Y * 1973 + p2.X * 1979 + p2.Y * 1987) % 2741 % 20;

            obstacle.Inflate(BUFFER, BUFFER);

            DrawCurvedLine(g, new PointF[] { p1, new PointF(p1.X, obstacle.Bottom), new PointF(obstacle.Right, obstacle.Bottom), new PointF(obstacle.Right, obstacle.Top), new PointF(p2.X, obstacle.Top), p2 }, 20);
        }

        private void DrawCurvyLine(Graphics g, PointF p1, PointF p2, bool selected, bool horizontal = true)
        {
            var b = GetBezier(p1, p2, horizontal);
            b.Draw(g, selected ? SelectedOutline : Outline);
        }

        public static Bezier GetBezier(PointF p1, PointF p2, bool horizontal = true)
        {
            float directness = 0.5f; // [0,1]. 0 is a straight line, 1 is flat in the middle
            if (horizontal)
            {
                PointF control1 = new PointF(p1.X, (p2.Y * directness + p1.Y * (1 - directness)));
                PointF control2 = new PointF(p2.X, (p1.Y * directness + p2.Y * (1 - directness)));
                return new Bezier(p1, control1, control2, p2);
            }
            else
            {
                PointF control1 = new PointF((p2.X * directness + p1.X * (1 - directness)), p1.Y);
                PointF control2 = new PointF((p1.X * directness + p2.X * (1 - directness)), p2.Y);
                return new Bezier(p1, control1, control2, p2);
            }
        }
    }
}

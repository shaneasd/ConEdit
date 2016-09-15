using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Utilities
{
    public static class RectangleUtil
    {
        public static IEnumerable<PointF> Corners(this RectangleF r)
        {
            yield return r.Location;
            yield return r.Location.Plus(r.Width - 1, 0);
            yield return r.Location.Plus(r.Width - 1, r.Height - 1);
            yield return r.Location.Plus(0, r.Height - 1);
        }

        public static IEnumerable<Rectangle> Split(this Rectangle r, Point p)
        {
            if (r.Contains(p))
            {
                var a = Rectangle.FromLTRB(r.Location.X, r.Location.Y, p.X, p.Y);
                var b = Rectangle.FromLTRB(p.X, r.Location.Y, r.Location.X + r.Width, p.Y);
                var c = Rectangle.FromLTRB(r.Location.X, p.Y, p.X, r.Location.Y + r.Height);
                var d = Rectangle.FromLTRB(p.X, p.Y, r.Location.X + r.Width, r.Location.Y + r.Height);
            }
            else
            {
                yield return r;
            }
        }

        public static IEnumerable<RectangleF> Split(this RectangleF a, RectangleF b)
        {
            if (a.IntersectsWith(b))
            {
                var left = a.Left <= b.Left ? a : b;
                var right = a.Right >= b.Right ? a : b;
                var intersect = a; intersect.Intersect(b);

                var maxY = Math.Max(a.Bottom, b.Bottom);
                var minY = Math.Min(a.Top, b.Top);

                var A = RectangleF.FromLTRB(left.Left, left.Top, intersect.Left, left.Bottom);
                var B = RectangleF.FromLTRB(intersect.Left, minY, intersect.Right, maxY);
                var C = RectangleF.FromLTRB(intersect.Right, right.Top, right.Right, right.Bottom);

                if (A.Width != 0 && A.Height != 0)
                    yield return A;
                yield return B;
                if (C.Width != 0 && C.Height != 0)
                    yield return C;
            }
            else
            {
                yield return a;
                yield return b;
            }
        }

        public static Edge[] Edges(this RectangleF a)
        {
            return new Edge[]
            {
                new Edge(a.Location, new PointF(a.Right, a.Top)),
                new Edge(new PointF(a.Right, a.Top), new PointF(a.Right, a.Bottom)),
                new Edge(new PointF(a.Right, a.Bottom), new PointF(a.Left, a.Bottom)),
                new Edge(new PointF(a.Left, a.Bottom), a.Location)
            };
        }
    }

    public class Edge
    {
        public PointF P1 { get; }
        public PointF P2 { get; }
        public Edge(PointF p1, PointF p2)
        {
            P1 = p1;
            P2 = p2;
        }

        static double Cross(PointF a, PointF b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public bool Crosses(PointF a, PointF b)
        {
            //double l;
            //double L;
            float dxdl = b.X - a.X;
            float dydl = b.Y - a.Y;
            //double dydx = dydl / dxdl;
            float dXdL = P2.X - P1.X;
            float dYdL = P2.Y - P1.Y;
            //double dYdX = dYdL / dXdL;

            //Point P1a = P1.Take(a);

            double denom = Cross(new PointF(dXdL, dYdL), new PointF(dxdl, dydl));

            if (denom == 0)
                return true;

            double l = Cross(a.Take(P1), new PointF(dxdl, dydl)) / denom;
            double L = Cross(a.Take(P1), new PointF(dXdL, dYdL)) / denom;

            return (l >= 0 && l <= 1 && L >= 0 && L <= 1);
        }
    }

    public class Shape
    {
        List<RectangleF> m_rectangles = new List<RectangleF>();

        public IEnumerable<RectangleF> AllRectangles
        {
            get
            {
                return m_rectangles;
            }
        }

        private float MinX
        {
            get
            {
                return AllRectangles.Select(r => r.Left).Concat(float.MaxValue.Only()).Min();
            }
        }

        private float MaxX
        {
            get
            {
                return float.MinValue.Only().Concat(AllRectangles.Select(rr => rr.Right)).Max();
            }
        }

        private void MergeRectangles(IEnumerable<RectangleF> r)
        {
            m_rectangles.AddRange(r);
            Split();
        }

        private void Split()
        {
            bool gotOne = true;
            while (gotOne)
            {
                gotOne = false;
                int count = m_rectangles.Count;
                for (int i = 0; i < count; i++)
                {
                    for (int j = i + 1; j < count; j++)
                    {
                        if (m_rectangles[i].IntersectsWith(m_rectangles[j]))
                        {
                            gotOne = true;
                            var a = m_rectangles[i].Split(m_rectangles[j]);
                            m_rectangles.RemoveAt(j);
                            m_rectangles.RemoveAt(i);
                            m_rectangles.AddRange(a);
                            i = int.MaxValue - 1;
                            j = int.MaxValue - 1;
                        }
                    }
                }
            }
        }

        public void AddRectangle(RectangleF r)
        {
            m_rectangles.Add(r);
        }

        public Shape(RectangleF r)
        {
            r.Inflate(2, 2);
            AddRectangle(r);
        }

        internal void Draw(Graphics g)
        {
            foreach (var rr in AllRectangles)
            {
                Color c = Color.FromKnownColor((KnownColor)((rr.Left * 1987 + rr.Top + 1) % 174));
                using (var brush = new SolidBrush(c))
                    g.FillRectangle(brush, rr);
            }

            //foreach (var r in AllRectangles)
            //{
            //    foreach (Edge e in r.Edges())
            //    {
            //        g.DrawLine(Pens.Magenta, e.P1, e.P2);
            //    }
            //    foreach (Point p in r.Corners())
            //    {
            //        RectangleUnion.DrawPoint(p, g);
            //    }
            //}
        }

        internal static Tuple<bool, Shape> TryMerge(Shape a, Shape b)
        {
            if (a.MinX > b.MaxX || b.MinX > a.MaxX)
                return Tuple.Create(false, (Shape)null);
            else if (!a.AllRectangles.Any(r1 => b.AllRectangles.Any(r2 => r1.IntersectsWith(r2))))
            {
                return Tuple.Create(false, (Shape)null);
            }
            else
            {
                a.MergeRectangles(b.AllRectangles);
                return Tuple.Create(true, a);
            }
        }

        public IEnumerable<PointF> Vertices
        {
            get
            {
                return m_rectangles.SelectMany(r => { r.Inflate(2, 2); return r.Corners(); });
            }
        }

        public IEnumerable<Edge> Edges
        {
            get
            {
                return m_rectangles.SelectMany(r => r.Edges());
            }
        }
    }

    public class RectangleUnion
    {
        private IList<Shape> m_shapes;

        public RectangleUnion(IList<Shape> shapes)
        {
            m_shapes = shapes;
        }

        public static RectangleUnion Unionize(IEnumerable<RectangleF> input)
        {
            IEnumerable<Shape> shapes = new List<Shape>();
            shapes = input.Select(r => new Shape(r));
            shapes = shapes.Merge((a, b) => Shape.TryMerge(a, b));
            shapes = shapes.Merge((a, b) => Shape.TryMerge(a, b));//Shouldn't have to do this twice
            return new RectangleUnion(shapes.ToList());
        }

        public void Draw(Graphics g)
        {
            foreach (var shape in m_shapes)
            {
                shape.Draw(g);
            }

            var edges = m_shapes.SelectMany(s => s.Edges).ToList();
            var vertices = m_shapes.SelectMany(s => s.Vertices).ToList();

            foreach (var edge in edges)
            {
                g.DrawLine(Pens.Magenta, edge.P1, edge.P2);
            }
            foreach (var vertex in vertices.Where(v => !m_shapes.Any(s => s.AllRectangles.Any(r => r.Contains(v)))))
            {
                DrawPoint(vertex, g);
            }

            var pairs = vertices.Pairs();
            foreach (var pair in pairs)
            {
                if (!edges.Any(e => e.Crosses(pair.Item1, pair.Item2)))
                    g.DrawLine(Pens.Red, pair.Item1, pair.Item2);
            }
        }

        public static void DrawPoint(PointF point, Graphics g)
        {
            g.DrawLine(Pens.Green, point.Take(1, 0), point.Plus(1, 0));
            g.DrawLine(Pens.Green, point.Take(0, 1), point.Plus(0, 1));
        }

        public IEnumerable<RectangleF> Obstacles
        {
            get
            {
                return m_shapes.SelectMany(s => s.AllRectangles);
            }
        }
    }

    public class PointComparer : IComparer<Point>
    {
        public int Compare(Point x, Point y)
        {
            if (x.X < y.X)
                return -1;
            else if (x.X > y.X)
                return 1;
            else if (x.Y < y.Y)
                return -1;
            else if (x.Y > y.Y)
                return 1;
            else
                return 0;
        }
    }

    //public class RectangleUnion
    //{


    //    SortedList<Point, Rectangle> m_rectangles = new SortedList<Point, Rectangle>(new PointComparer());

    //    public void AddRectangle(Rectangle r)
    //    {
    //        m_rectangles.Add(r.Location, r);
    //    }

    //    //public List<Point> Vertices
    //    //{
    //    //    get
    //    //    {
    //    //        var points = m_rectangles.SelectMany(RectangleUtil.Corners);
    //    //        var intersections = m_rectangles.Pairs().SelectMany(a => { var b = a.Item1; b.Intersect(a.Item2); return b.Corners(); });
    //    //        return points.Concat(intersections).ToList();
    //    //    }
    //    //}

    //    public void Draw(Graphics g)
    //    {
    //    }

    //    public void DrawPoint(Point point, Graphics g)
    //    {
    //        g.DrawLine(Pens.Green, point.Take(1, 0), point.Plus(1, 0));
    //        g.DrawLine(Pens.Green, point.Take(0, 1), point.Plus(0, 1));
    //    }
    //}
}

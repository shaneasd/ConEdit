using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Utilities
{
    public class Bezier
    {
        public static readonly Mat BezierDefinition = new Mat(4, 4, new double[] { -1, 3, -3, 1, 3, -6, 3, 0, -3, 3, 0, 0, 1, 0, 0, 0 });
        public readonly PointF A, B, C, D;
        public readonly PointF[] Coefficients;
        public readonly PointF Average;
        public readonly float BoundingRadius;
        public Bezier(PointF a, PointF b, PointF c, PointF d)
        {
            A = a; B = b; C = c; D = d;
            Coefficients = new PointF[4];
            Coefficients[0] = a;
            Coefficients[1] = b;
            Coefficients[2] = c;
            Coefficients[3] = d;

            Average = A.Plus(B).Plus(C).Plus(D).ScaleBy(0.25f);
            BoundingRadius = Coefficients.Select(p => p.DistanceTo(Average)).Max();
        }


        public PointF At(float t)
        {
            var bezierComponents = BezierDefinition.PreMultiply(Coefficients, (a, b) => b.ScaleBy((float)a), (a, b) => a.Plus(b), PointF.Empty);
            return bezierComponents.Zip(new double[] { t * t * t, t * t, t, 1 }, (p, a) => p.ScaleBy((float)a)).Aggregate((a, b) => a.Plus(b));
        }

        public bool WithinDistance(PointF p, float r)
        {
            //Early out if it's outside the bounding circle
            if (p.DistanceTo(Average) > BoundingRadius + r)
                return false;

            if (A.Take(p).LengthSquared() < r * r)
                return true; //It's at one of the end points
            if (D.Take(p).LengthSquared() < r * r)
                return true; //It's at the other end point

            //Vector containing <R,S,T,U> for the point: P = Rt^3 + St^2 + Tt^1 + Ut^0
            PointF[] bezierComponents = BezierDefinition.PreMultiply(Coefficients, (a, b) => b.ScaleBy((float)a), (a, b) => a.Plus(b), PointF.Empty);
            PointF R = bezierComponents[0];
            PointF S = bezierComponents[1];
            PointF T = bezierComponents[2];
            PointF U = bezierComponents[3];
            U = U.Take(p); //Translate into circle space

            //P.P = (Rt^3 + St^2 + Tt^1 + Ut^0).(Rt^3 + St^2 + Tt^1 + Ut^0)
            //    = R.Rt^6 + 2R.St^5 + (2R.T + S.S)t^4 + (2R.U + 2S.T)t^3 + (2S.U + T.T)t^2 + 2T.Ut^1 + U.Ut^0
            //P.P - r*r = 0 means intersection

            Polynomial poly = new Polynomial(new double[] { U.Dot(U) - r * r, 2 * U.Dot(T), 2 * S.Dot(U) + T.Dot(T), 2 * R.Dot(U) + 2 * S.Dot(T), 2 * R.Dot(T) + S.Dot(S), 2 * R.Dot(S), R.Dot(R) });

            return poly.HasRealRoot(0, 1);
        }

        public void DebugDraw(Graphics g)
        {
            var bezierComponents = BezierDefinition.PreMultiply(Coefficients, (a, b) => b.ScaleBy((float)a), (a, b) => a.Plus(b), PointF.Empty);

            for (float t = 0; t < 1; t += 0.05f)
            {
                g.DrawLine(Pens.Green, At(t), At(t + 0.05f));
            }
        }

        public void Draw(Graphics g, Pen pen)
        {
            g.DrawBezier(pen, A, B, C, D);
        }
    }
}

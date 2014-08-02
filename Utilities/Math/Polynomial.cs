using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class Polynomial
    {
        public readonly double[] Coefficients;
        public Polynomial(IEnumerable<double> coefficients)
        {
            Coefficients = coefficients.ToArray();
            for (int i = Coefficients.Length - 1; i >= 1; i--)
                if (Math.Abs(Coefficients[i]) < 1e-12)
                    Array.Resize(ref Coefficients, i);
            m_deriv = new Lazy<Polynomial>(() => new Polynomial(Coefficients.Select((t, i) => i * t).Skip(1)));
        }

        public uint Order { get { return (uint)Coefficients.Length - 1; } }
        public uint Indices { get { return (uint)Coefficients.Length; } }

        Lazy<Polynomial> m_deriv;
        public Polynomial Derivative
        {
            get
            {
                return m_deriv.Value;
            }
        }

        public override string ToString()
        {
            return string.Join(" + ", Coefficients.Select((t, i) => t + "t^" + i));
        }

        public double At(double t)
        {
            return Coefficients.Select((c, i) => c * Math.Pow(t, i)).Sum();
        }

        public double[] RealRoots()
        {
            if (Order == 0)
                return new double[0];
            if (Order == 1)
                return new double[] { -Coefficients[1] / Coefficients[0] };
            if (Order == 2)
            {
                double c = Coefficients[0];
                double b = Coefficients[1];
                double a = Coefficients[2];

                double discriminant = b * b - 4 * a * c;
                if (discriminant > 0)
                {
                    return new double[]
                    {
                        (-b + Math.Sqrt(discriminant))/(2 * a),
                        (-b - Math.Sqrt(discriminant))/(2 * a)
                    };
                }
                else
                    return new double[0];
            }
            throw new Exception();
        }

        public bool HasRealRoot(double lower, double upper)
        {
            if (Order == 0)
                return false;
            if (Order == 1)
                return true;

            var sturm = SturmSequence();

            int signChangesAtLower = 0;
            int signChangesAtUpper = 0;

            int lastSignLower = Math.Sign(sturm[0].At(lower));
            int lastSignUpper = Math.Sign(sturm[0].At(upper));
            for (int i = 0; i < sturm.Length; i++)
            {
                var signLower = Math.Sign(sturm[i].At(lower));
                var signUpper = Math.Sign(sturm[i].At(upper));
                if (signLower != 0)
                {
                    if (signLower * lastSignLower < 0)
                    {
                        signChangesAtLower++;
                    }
                    lastSignLower = signLower;
                }
                if (signUpper != 0)
                {
                    if (signUpper * lastSignUpper < 0)
                    {
                        signChangesAtUpper++;
                    }
                    lastSignUpper = signUpper;
                }
            }

            return signChangesAtLower != signChangesAtUpper;
        }

        public bool HasRealRoot()
        {
            if (Order == 0)
                return false;
            else if (Order == 1)
                return true;
            else if (Order == 2)
            {
                double c = Coefficients[0];
                double b = Coefficients[1];
                double a = Coefficients[2];

                double discriminant = b * b - 4 * a * c;
                return discriminant >= 0;
            }
            else
            {
                var sturm = SturmSequence();

                int signChangesAtNegativeInfinity = 0;
                int signChangesAtPositiveInfinity = 0;

                int lastSignN = sturm[0].SignAtNegativeInfinity;
                int lastSignP = sturm[0].SignAtNegativeInfinity;
                for (int i = 0; i < sturm.Length; i++)
                {
                    var signN = sturm[i].SignAtNegativeInfinity;
                    var signP = sturm[i].SignAtPositiveInfinity;
                    if (signN != 0)
                    {
                        if (signN * lastSignN < 0)
                        {
                            signChangesAtNegativeInfinity++;
                        }
                        lastSignN = signN;
                    }
                    if (signP != 0)
                    {
                        if (signP * lastSignP < 0)
                        {
                            signChangesAtPositiveInfinity++;
                        }
                        lastSignP = signP;
                    }
                }

                return signChangesAtNegativeInfinity != signChangesAtPositiveInfinity;
            }
        }

        public static Polynomial PolynomialRemainder(Polynomial a, Polynomial b)
        {
            while (a.Order >= b.Order)
            {
                var power = a.Order - b.Order;
                var scale = a.Coefficients.Last() / b.Coefficients.Last();

                double[] newCoefficients = a.Coefficients.Take((int)a.Order).ToArray();
                for (int i = 0; i < b.Order; i++)
                {
                    newCoefficients[i + power] -= scale * b.Coefficients[i];
                }
                a = new Polynomial(newCoefficients);
            }
            return a;
        }

        public int SignAtNegativeInfinity
        {
            get
            {
                return Math.Sign(Coefficients.Last()) * (((int)Order % 2) * -2 + 1);
            }
        }

        public int SignAtPositiveInfinity
        {
            get
            {
                return Math.Sign(Coefficients.Last());
            }
        }

        public Polynomial[] SturmSequence()
        {
            Polynomial[] result = new Polynomial[Order + 1];
            result[0] = this;
            result[1] = Derivative;
            int i;
            for (i = 2; i <= Order; i++)
            {
                var next = -(Polynomial.PolynomialRemainder(result[i - 2], result[i - 1]));
                if (next.Indices > 0)
                    result[i] = next;
                else
                {
                    Array.Resize(ref result, i);
                    break;
                }
            }
            return result;
        }

        public static Polynomial operator +(Polynomial a, Polynomial b)
        {
            Polynomial longer = a.Order > b.Order ? a : b;
            int lower = Math.Min((int)a.Order, (int)b.Order);
            List<double> coefficients = new List<double>((int)longer.Order + 1);
            for (int i = 0; i <= lower; i++)
                coefficients.Add(a.Coefficients[i] + b.Coefficients[i]);
            for (int i = lower + 1; i < coefficients.Capacity; i++)
                coefficients.Add(longer.Coefficients[i]);
            while (Math.Abs(coefficients.Last()) < 1e-12)
                coefficients.RemoveAt(coefficients.Count - 1);
            return new Polynomial(coefficients);
        }

        public static Polynomial operator -(Polynomial a)
        {
            return new Polynomial(a.Coefficients.Select(x => -x));
        }

        public static Polynomial operator *(Polynomial a, Polynomial b)
        {
            Polynomial longer = a.Order > b.Order ? a : b;
            double[] coefficients = new double[a.Order + b.Order + 1];
            for (uint i = 0; i <= a.Order; i++)
            {
                if (Math.Abs(a.Coefficients[i]) > 1e-12)
                {
                    for (uint j = 0; j <= b.Order; j++)
                    {
                        if (Math.Abs(b.Coefficients[j]) > 1e-12)
                            coefficients[i + j] += a.Coefficients[i] * b.Coefficients[j];
                    }
                }
            }
            return new Polynomial(coefficients);
        }
    }
}

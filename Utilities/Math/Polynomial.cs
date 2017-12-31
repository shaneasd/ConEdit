using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class Polynomial
    {
        private readonly double[] m_coefficients;
        public IReadOnlyList<double> Coefficients => m_coefficients;
        public Polynomial(IEnumerable<double> coefficients)
        {
            m_coefficients = coefficients.ToArray();
            if (!m_coefficients.Any())
                m_coefficients = new double[] { 0 };
            for (int i = m_coefficients.Length - 1; i >= 1; i--)
                if (Math.Abs(m_coefficients[i]) < 1e-12)
                    Array.Resize(ref m_coefficients, i);
                else
                    break;
            m_deriv = new Lazy<Polynomial>(() => new Polynomial(m_coefficients.Select((t, i) => i * t).Skip(1)));
        }

        public int Order => m_coefficients.Length - 1;
        public int Indexes => m_coefficients.Length;

        Lazy<Polynomial> m_deriv;
        public Polynomial Derivative => m_deriv.Value;

        public override string ToString()
        {
            return string.Join(" + ", m_coefficients.Select((t, i) => t + "t^" + i));
        }

        public double At(double t)
        {
            return m_coefficients.Select((c, i) => c * Math.Pow(t, i)).Sum();
        }

        public double[] RealRoots()
        {
            if (Order == 0)
                return new double[0];
            if (Order == 1)
                return new double[] { -m_coefficients[1] / m_coefficients[0] };
            if (Order == 2)
            {
                double c = m_coefficients[0];
                double b = m_coefficients[1];
                double a = m_coefficients[2];

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
            throw new NotSupportedException("Calculation of roots for polynomial of order more than 2 not supported");
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
                double c = m_coefficients[0];
                double b = m_coefficients[1];
                double a = m_coefficients[2];

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

        public bool IsZero()
        {
            if (Order == 0 && Math.Abs(m_coefficients[0]) < 1e-12)
                return true;
            else
                return false;
        }

        public static Polynomial PolynomialRemainder(Polynomial a, Polynomial b)
        {
            while (a.Order >= b.Order)
            {
                var power = a.Order - b.Order;
                var scale = a.m_coefficients.Last() / b.m_coefficients.Last();

                double[] newCoefficients = a.m_coefficients.Take((int)a.Order).ToArray();
                for (int i = 0; i < b.Order; i++)
                {
                    newCoefficients[i + power] -= scale * b.m_coefficients[i];
                }
                a = new Polynomial(newCoefficients);
            }
            return a;
        }

        public int SignAtNegativeInfinity => Math.Sign(m_coefficients.Last()) * (((int)Order % 2) * -2 + 1);

        public int SignAtPositiveInfinity => Math.Sign(m_coefficients.Last());

        public Polynomial[] SturmSequence()
        {
            Polynomial[] result = new Polynomial[Order + 1];
            result[0] = this;
            result[1] = Derivative;
            int i;
            for (i = 2; i <= Order; i++)
            {
                var next = -(Polynomial.PolynomialRemainder(result[i - 2], result[i - 1]));
                if (!next.IsZero())
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
                coefficients.Add(a.m_coefficients[i] + b.m_coefficients[i]);
            for (int i = lower + 1; i < coefficients.Capacity; i++)
                coefficients.Add(longer.m_coefficients[i]);
            return new Polynomial(coefficients);
        }

        public static Polynomial operator -(Polynomial a)
        {
            return new Polynomial(a.m_coefficients.Select(x => -x));
        }

        public static Polynomial operator *(Polynomial a, Polynomial b)
        {
            double[] coefficients = new double[a.Order + b.Order + 1];
            for (uint i = 0; i <= a.Order; i++)
            {
                if (Math.Abs(a.m_coefficients[i]) > 1e-12)
                {
                    for (uint j = 0; j <= b.Order; j++)
                    {
                        if (Math.Abs(b.m_coefficients[j]) > 1e-12)
                            coefficients[i + j] += a.m_coefficients[i] * b.m_coefficients[j];
                    }
                }
            }
            return new Polynomial(coefficients);
        }
    }
}

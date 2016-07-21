using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using ConversationEditor;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace Tests
{
    public static partial class Program
    {
        static void Main()
        {
            TestRawLocalization.TestDataIntegrity();
            //TestQuadTree.AllTests();
            //TestFileSystem.TestPathToFromDirDir();
            //TestWeakEvent();
            //TestPolynomial();
            //TestUndoQueue.TestEverything();
            //ManualResetEvent();
        }


        [NUnit.Framework.Test]
        public static void TestPolynomial()
        {
            {
                //Console.Out.WriteLine("Definition");
                double[] coefficients = { 5, 2, 8, 7 };
                Polynomial p = new Polynomial(coefficients);
                //Console.Out.WriteLine(p);
                //Console.Out.WriteLine(p.Derivative);
                //Console.Out.WriteLine();
                Assert.AreEqual(2*1, p.Derivative.Coefficients[0]);
                Assert.AreEqual(8*2, p.Derivative.Coefficients[1]);
                Assert.AreEqual(7*3, p.Derivative.Coefficients[2]);
            }

            {
                Console.Out.WriteLine("Zero");
                double[] coefficients = { 0, 0, 0, 0 };
                Polynomial p = new Polynomial(coefficients);
                Assert.True(p.Coefficients.Any());
            }

            {
                Console.Out.WriteLine("Polynomial Remainder");
                Polynomial a = new Polynomial(new double[] { 1, 1, 1, 1, 1 });
                Polynomial b = new Polynomial(new double[] { -1, 0, 1 });
                Console.Out.WriteLine(a);
                Console.Out.WriteLine("-----------------------");
                Console.Out.WriteLine(b);
                Console.Out.WriteLine();
                var c = Polynomial.PolynomialRemainder(a, b);
                Console.Out.WriteLine(c);
            }

            {
                Console.Out.WriteLine("Polynomial Remainder");
                Polynomial a = new Polynomial(new double[] { 1, 1, 1 });
                Polynomial b = new Polynomial(new double[] { 1, 1, 1 });
                var c = Polynomial.PolynomialRemainder(a, b);
                Console.Out.WriteLine(a);
                Console.Out.WriteLine("-----------------------");
                Console.Out.WriteLine(b);
                Console.Out.WriteLine();
                Console.Out.WriteLine(c);
            }

            {
                Console.Out.WriteLine("Multiplication");
                Polynomial a = new Polynomial(new double[] { 1, 2 });
                Polynomial b = new Polynomial(new double[] { -4, 1, 0, 0, 3 });
                var c = a * b;
                Console.Out.WriteLine(a + "  *  " + b);
                Console.Out.WriteLine();
                Console.Out.WriteLine(c);
            }

            {
                Console.Out.WriteLine("Addition");
                Polynomial a = new Polynomial(new double[] { 1, 2 });
                Polynomial b = new Polynomial(new double[] { -4, 1, 0, 0, 3 });
                var c = a + b;
                //Console.Out.WriteLine(a + "  +  " + b);
                //Console.Out.WriteLine();
                //Console.Out.WriteLine(c);
                Assert.AreEqual(a.Coefficients[0] + b.Coefficients[0], c.Coefficients[0]);
                Assert.AreEqual(a.Coefficients[1] + b.Coefficients[1], c.Coefficients[1]);
                Assert.AreEqual(b.Coefficients[2], c.Coefficients[2]);
                Assert.AreEqual(b.Coefficients[3], c.Coefficients[3]);
                Assert.AreEqual(b.Coefficients[4], c.Coefficients[4]);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using ConversationEditor;
using System.IO;
using System.Threading;

namespace ConsoleApplication1
{
    class Program
    {
        public static void Assert(bool condition)
        {
            if (!condition)
                throw new Exception();
        }
        static void Main(string[] args)
        {
            TestPolynomial();
            TestUndoQueue();
            ManualResetEvent();
        }

        public static void TestUndoQueue()
        {
            SaveableFileUndoable file = new SaveableFileUndoable(new MemoryStream(), new FileInfo("ignore.txt"), a => { });
            Assert(!file.Changed);
            file.Change(new GenericUndoAction(() => { }, () => { }, ""));
            Assert(file.Changed);
            file.UndoQueue.Undo();
            Assert(!file.Changed);
            file.UndoQueue.Redo();
            Assert(file.Changed);
            file.Save();
            Assert(!file.Changed);
            file.Change(new GenericUndoAction(() => { }, () => { }, ""));
            Assert(file.Changed);
            file.UndoQueue.Undo();
            Assert(!file.Changed);
            file.UndoQueue.Undo();
            Assert(file.Changed);
            file.UndoQueue.Redo();
            Assert(!file.Changed);
            file.Dispose();
        }

        public static void TestPolynomial()
        {
            {
                Console.Out.WriteLine("Definition");
                double[] coefficients = { 5, 2, 8, 7 };
                Polynomial p = new Polynomial(coefficients);
                Console.Out.WriteLine(p);
                Console.Out.WriteLine(p.Derivative);
                Console.Out.WriteLine();
            }

            {
                Console.Out.WriteLine("Zero");
                double[] coefficients = { 0, 0, 0, 0 };
                Polynomial p = new Polynomial(coefficients);
                Assert(p.Coefficients.Any());
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
                Console.Out.WriteLine(a + "  +  " + b);
                Console.Out.WriteLine();
                Console.Out.WriteLine(c);
            }
        }

        public static void ManualResetEvent()
        {
            ManualResetEventSlim e = new ManualResetEventSlim(false);
            ManualResetEventSlim w = new ManualResetEventSlim();
            ManualResetEventSlim ww = new ManualResetEventSlim();
            Thread t = new Thread(() => { Thread.Sleep(1000); e.Set(); });
            t.Start();
            WaitHandle.WaitAny(new[] { e.WaitHandle, w.WaitHandle, ww.WaitHandle });
        }
    }
}

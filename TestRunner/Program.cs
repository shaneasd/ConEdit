using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace TestRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            //Stack<int> s = new Stack<int>();
            //s.Push(0);
            //s.Push(1);
            //s.Push(2);
            //foreach (var a in s)
            //{
            //}

            //Utilities.DirectoryEqualityComparer d = new Utilities.DirectoryEqualityComparer();
            //DirectoryInfo d1 = new DirectoryInfo(@"C:\1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");
            //DirectoryInfo d2 = new DirectoryInfo(@"C:\1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678902134567890");
            //Stopwatch w = Stopwatch.StartNew();
            //for (int i = 0; i < 2600; i++)
            //{
            //    d.Equals(d1, d2);
            //}
            //w.Stop();

            DirectoryInfo a = new DirectoryInfo(@"c:\a\");
            DirectoryInfo b = new DirectoryInfo(@"c:\a\\");
            DirectoryInfo c = new DirectoryInfo(@"c:\a\\\");
            DirectoryInfo d = new DirectoryInfo(@"c:\A\");

            NUnit.ConsoleRunner.Runner.Main(args);
        }
    }
}

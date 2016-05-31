using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Utilities;
using System.Drawing;

namespace Tests
{
    public static class TestQuadTree
    {
        public static void AllTests()
        {
            var allMethods = typeof(TestQuadTree).GetMethods();
            var filteredMethods = allMethods.Where(m => m.CustomAttributes.Any(a => a.AttributeType == typeof(NUnit.Framework.TestAttribute)));
            foreach (var method in filteredMethods)
            {
                method.Invoke(null, new object[0]);
            }
        }

        [NUnit.Framework.Test]
        public static void TestAddDepth0()
        {
            var test = new QuadTreeElement<object>(RectangleF.FromLTRB(0, 0, 1, 1));
            var element1 = new object();
            var element2 = new object();
            Assert.That(test.Add(element1, RectangleF.FromLTRB(0.1f, 0.1f, 0.9f, 0.9f)), Is.EqualTo(test));
            Assert.That(test, Contains.Item(element1));
            Assert.That(test.Count(), Is.EqualTo(1));

            Assert.That(test.Add(element2, RectangleF.FromLTRB(0.1f, 0.1f, 0.9f, 0.9f)), Is.EqualTo(test));
            Assert.That(test, Contains.Item(element1));
            Assert.That(test, Contains.Item(element2));
            Assert.That(test.Count(), Is.EqualTo(2));
        }

        [NUnit.Framework.Test]
        public static void TestAddDepth1()
        {
            var test = new QuadTreeElement<object>(RectangleF.FromLTRB(0, 0, 1, 1));

            var element00 = new object();
            var element01 = new object();
            var element10 = new object();
            var element11 = new object();

            var result00 = test.Add(element00, RectangleF.FromLTRB(0.1f, 0.1f, 0.4f, 0.4f));
            var result01 = test.Add(element01, RectangleF.FromLTRB(0.1f, 0.6f, 0.4f, 0.9f));
            var result10 = test.Add(element10, RectangleF.FromLTRB(0.6f, 0.1f, 0.9f, 0.4f));
            var result11 = test.Add(element11, RectangleF.FromLTRB(0.6f, 0.6f, 0.9f, 0.9f));

            var results = new[] { result00, result01, result10, result11 };

            Assert.That(results.Distinct().Count(), Is.EqualTo(4));

            Assert.That(result00, Contains.Item(element00));
            Assert.That(result01, Contains.Item(element01));
            Assert.That(result10, Contains.Item(element10));
            Assert.That(result11, Contains.Item(element11));
            Assert.That(result00.Count(), Is.EqualTo(1));
            Assert.That(result01.Count(), Is.EqualTo(1));
            Assert.That(result10.Count(), Is.EqualTo(1));
            Assert.That(result11.Count(), Is.EqualTo(1));

            Assert.That(test, Contains.Item(element00));
            Assert.That(test, Contains.Item(element01));
            Assert.That(test, Contains.Item(element10));
            Assert.That(test, Contains.Item(element11));
            Assert.That(test.Count(), Is.EqualTo(4));
        }
    }
}

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Tests
{
    public static class TestGraph
    {
        /// <summary>
        /// A few elements with no ordering.
        /// </summary>
        [Test]
        public static void TestOrderNoRelation()
        {
            IEnumerable<T> noRelation<T>(T x) => Enumerable.Empty<T>();
            var test = new[] { 1, 2, 3 };
            var result = Graph.Order(test, noRelation, noRelation);
            void checkLists(List<List<int>> lists)
            {
                Assert.That(lists.Count, Is.EqualTo(3));
                Assert.That(lists.Any(l => l.SequenceEqual(test[0].Only())));
                Assert.That(lists.Any(l => l.SequenceEqual(test[1].Only())));
                Assert.That(lists.Any(l => l.SequenceEqual(test[2].Only())));
            }
            result.Do(checkLists, a => Assert.Fail("Failed to order unrelated vertices"));
        }

        /// <summary>
        /// Test a sequence of nodes that can only be ordered one way
        /// </summary>
        [Test]
        public static void TestStrictOrdering()
        {
            var test = new[] { 1, 2, 3, 4, 5 };
            IEnumerable<int> parents(int x) => x > test.Min() ? (x - 1).Only() : Enumerable.Empty<int>();
            IEnumerable<int> children(int x) => x < test.Max() ? (x + 1).Only() : Enumerable.Empty<int>();
            var result = Graph.Order(test, parents, children);
            void checkLists(List<List<int>> lists)
            {
                Assert.That(lists.Count, Is.EqualTo(1));
                CollectionAssert.AreEqual(test, lists[0]);
            }
            result.Do(checkLists, a => Assert.Fail("Failed to order unrelated vertices"));
        }

        private static void CheckOrder<T>(List<T> vertices, Func<T, IEnumerable<T>> getParents, Func<T, IEnumerable<T>> getChildren)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                var vertex = vertices[i];
                foreach (var parent in getParents(vertex))
                    Assert.That(vertices.IndexOf(parent) < i);
                foreach (var child in getChildren(vertex))
                    Assert.That(vertices.IndexOf(child) > i); //Strictly speaking I think if children and parents are set up correctly this is redundant but it can't hurt and may pick up issues with the test
            }
        }

        /// <summary>
        /// 1
        /// |\
        /// 2 3
        /// |/
        /// 4
        /// </summary>
        [Test]
        public static void TestDiamond()
        {
            var test = new[] { 1, 2, 3, 4 };
            IEnumerable<int> parents(int x)
            {
                switch (x)
                {
                    case 1: return Enumerable.Empty<int>();
                    case 2: return 1.Only();
                    case 3: return 1.Only();
                    case 4: return new[] { 2, 3 };
                    default:
                        throw new AssertionException("Something went wrong to get to here");
                }
            }
            IEnumerable<int> children(int x)
            {
                switch (x)
                {
                    case 1: return new[] { 2, 3 };
                    case 2: return 4.Only();
                    case 3: return 4.Only();
                    case 4: return Enumerable.Empty<int>();
                    default:
                        throw new AssertionException("Something went wrong to get to here");
                }
            }
            var result = Graph.Order(test, parents, children);

            void checkList(List<List<int>> lists)
            {
                Assert.That(lists.Count, Is.EqualTo(1));
                var list = lists[0];
                CollectionAssert.AreEquivalent(test, list);
                CheckOrder(list, parents, children);
            }

            result.Do(checkList, a => Assert.Fail("Failed to order unrelated vertices"));
        }

        /// <summary>
        /// 1
        /// |
        /// 2
        /// |\
        /// 3 4
        /// | |
        /// 5 6
        /// |/
        /// 7
        /// |
        /// 8
        /// </summary>
        [Test]
        public static void TestLongDiamond()
        {
            var test = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            IEnumerable<int> parents(int x)
            {
                switch (x)
                {
                    case 1: return Enumerable.Empty<int>();
                    case 2: return 1.Only();
                    case 3: return 2.Only();
                    case 4: return 2.Only();
                    case 5: return 3.Only();
                    case 6: return 4.Only();
                    case 7: return new[] { 5, 6 };
                    case 8: return 7.Only();
                    default:
                        throw new AssertionException("Something went wrong to get to here");
                }
            }
            IEnumerable<int> children(int x)
            {
                switch (x)
                {
                    case 1: return 2.Only();
                    case 2: return new[] { 3, 4 };
                    case 3: return 5.Only();
                    case 4: return 6.Only();
                    case 5: return 7.Only();
                    case 6: return 7.Only();
                    case 7: return 8.Only();
                    case 8: return Enumerable.Empty<int>();
                    default:
                        throw new AssertionException("Something went wrong to get to here");
                }
            }
            var result = Graph.Order(test, parents, children);

            void checkList(List<List<int>> lists)
            {
                Assert.That(lists.Count, Is.EqualTo(1));
                var list = lists[0];
                CollectionAssert.AreEquivalent(test, list);
                CheckOrder(list, parents, children);
            }

            result.Do(checkList, a => Assert.Fail("Failed to order unrelated vertices"));
        }

        /// <summary>
        ///   1
        ///   |
        ///   2
        ///  /|\
        /// 3 ||
        /// |/ |
        /// 4 /
        /// |/
        /// 5
        /// </summary>
        [Test]
        public static void TestCrossGeneration()
        {
            var test = new[] { 1, 2, 3, 4, 5 };
            IEnumerable<int> parents(int x)
            {
                switch (x)
                {
                    case 1: return Enumerable.Empty<int>();
                    case 2: return 1.Only();
                    case 3: return 2.Only();
                    case 4: return new[] { 3, 2 };
                    case 5: return new[] { 4, 2 };
                    default:
                        throw new AssertionException("Something went wrong to get to here");
                }
            }
            IEnumerable<int> children(int x)
            {
                switch (x)
                {
                    case 1: return 2.Only();
                    case 2: return new[] { 3, 4, 5 };
                    case 3: return 4.Only();
                    case 4: return 5.Only();
                    case 5: return Enumerable.Empty<int>();
                    default:
                        throw new AssertionException("Something went wrong to get to here");
                }
            }
            var result = Graph.Order(test, parents, children);

            void checkList(List<List<int>> lists)
            {
                Assert.That(lists.Count, Is.EqualTo(1));
                var list = lists[0];
                CollectionAssert.AreEquivalent(test, list);
                CheckOrder(list, parents, children);
            }

            result.Do(checkList, a => Assert.Fail("Failed to order unrelated vertices"));
        }

        /// <summary>
        /// 1    5   7
        /// |\   |
        /// 2 3  6
        /// |/
        /// 4
        /// </summary>
        [Test]
        public static void TestUnconnected()
        {
            var test = new[] { 1, 2, 3, 4, 5, 6, 7 };
            IEnumerable<int> parents(int x)
            {
                switch (x)
                {
                    case 1: return Enumerable.Empty<int>();
                    case 2: return 1.Only();
                    case 3: return 1.Only();
                    case 4: return new[] { 2, 3 };
                    case 5: return Enumerable.Empty<int>();
                    case 6: return 5.Only();
                    case 7: return Enumerable.Empty<int>();
                    default:
                        throw new AssertionException("Something went wrong to get to here");
                }
            }
            IEnumerable<int> children(int x)
            {
                switch (x)
                {
                    case 1: return new[] { 2, 3 };
                    case 2: return 4.Only();
                    case 3: return 4.Only();
                    case 4: return Enumerable.Empty<int>();
                    case 5: return 6.Only();
                    case 6: return Enumerable.Empty<int>();
                    case 7: return Enumerable.Empty<int>();
                    default:
                        throw new AssertionException("Something went wrong to get to here");
                }
            }
            var result = Graph.Order(test, parents, children);

            void checkList(List<int> list, params int[] against)
            {
                CollectionAssert.AreEquivalent(against, list);
                CheckOrder(list, parents, children);
            }

            void checkLists(List<List<int>> lists)
            {
                Assert.That(lists.Count, Is.EqualTo(3)); //Make sure it was successfully split into 3 lists
                Assert.That(lists.Select(x => x.Count), Is.EquivalentTo(new[] { 1, 2, 4 })); //Make sure the lists have 1, 2, 4 elements (not necessarily in that order)
                Assert.That(lists.SelectMany(x => x), Is.EquivalentTo(test)); //Make sure all the origin elements are represented in the union of the lists

                checkList(lists.First(l => l.Contains(1)), 1, 2, 3, 4);
                checkList(lists.First(l => l.Contains(5)), 5, 6);
                checkList(lists.First(l => l.Contains(7)), 7);
            }

            result.Do(checkLists, a => Assert.Fail("Failed to order unrelated vertices"));
        }

        /// <summary>
        ///  __
        /// /  \
        /// 1  |
        /// |  |
        /// 2  |
        /// |  |
        /// 3  |
        /// \__/
        /// </summary>
        [Test]
        public static void TestCycle()
        {
            var test = new[] { 1, 2, 3 };
            IEnumerable<int> parents(int x)
            {
                switch (x)
                {
                    case 1: return 3.Only();
                    case 2: return 1.Only();
                    case 3: return 2.Only();
                    default:
                        throw new AssertionException("Something went wrong to get to here");
                }
            }
            IEnumerable<int> children(int x)
            {
                switch (x)
                {
                    case 1: return 2.Only();
                    case 2: return 3.Only();
                    case 3: return 1.Only();
                    default:
                        throw new AssertionException("Something went wrong to get to here");
                }
            }
            var result = Graph.Order(test, parents, children);
            result.Do(a => Assert.Fail("failed to detect cycle"), a => { });
        }

        /// <summary>
        /// 1 5
        /// |\|
        /// 2 3
        /// |/
        /// 4
        /// </summary>
        [Test]
        public static void TestRealWorldFailure1()
        {
            var test = new[] { 1, 2, 3, 4, 5 };
            IEnumerable<int> parents(int x)
            {
                switch (x)
                {
                    case 1: return Enumerable.Empty<int>();
                    case 2: return 1.Only();
                    case 3: return new[] { 1, 5 };
                    case 4: return new[] { 2, 3 };
                    case 5: return Enumerable.Empty<int>();
                    default:
                        throw new AssertionException("Something went wrong to get to here");
                }
            }
            IEnumerable<int> children(int x)
            {
                switch (x)
                {
                    case 1: return new[] { 2, 3 };
                    case 2: return 4.Only();
                    case 3: return 4.Only();
                    case 4: return Enumerable.Empty<int>();
                    case 5: return 3.Only();
                    default:
                        throw new AssertionException("Something went wrong to get to here");
                }
            }
            var result = Graph.Order(test, parents, children);

            void checkList(List<List<int>> lists)
            {
                Assert.That(lists.Count, Is.EqualTo(1));
                var list = lists[0];
                CollectionAssert.AreEquivalent(test, list);
                CheckOrder(list, parents, children);
            }

            result.Do(checkList, a => Assert.Fail("Failed to order unrelated vertices"));
        }
    }
}

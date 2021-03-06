﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    //TODO: Introduce a tolerance to account for floating point precision

    public interface IReadOnlyQuadTree<T> : IEnumerable<T>
    {
        IEnumerable<T> FindTouchingRegion(RectangleF bounds);
    }

    public class ZOrderedQuadTree<T> : IReadOnlyQuadTree<T>
    {
        private Comparison<T> m_relativePosition;
        private QuadTree<T> m_tree;

        public ZOrderedQuadTree(QuadTree<T> tree, Comparison<T> relativePosition)
        {
            m_tree = tree;
            m_relativePosition = relativePosition;
        }

        public IEnumerable<T> FindTouchingRegion(RectangleF bounds)
        {
            var list = m_tree.FindTouchingRegion(bounds).ToList();
            list.Sort(m_relativePosition);
            return list;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var list = m_tree.ToList();
            list.Sort(m_relativePosition);
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            var list = m_tree.ToList();
            list.Sort(m_relativePosition);
            return list.GetEnumerator();
        }
    }

    //TODO: Kinda defeats the purpose of maintaining the spatial information if we're just going to ignore it.
    public class Fake<T> : IReadOnlyQuadTree<T>
    {
        IEnumerable<T> wrapped;
        public Fake(IEnumerable<T> w)
        {
            wrapped = w;
        }

        public IEnumerable<T> FindTouchingRegion(RectangleF bounds)
        {
            return wrapped;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return wrapped.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return wrapped.GetEnumerator();
        }
    }

    public class QuadTree<T> : IReadOnlyQuadTree<T>
    {
        Dictionary<T, RectangleF> m_boundsMap = new Dictionary<T, RectangleF>();

        Element m_root;
        public QuadTree(RectangleF initialBounds)
        {
            m_root = new Element(initialBounds);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return m_root.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_root.GetEnumerator();
        }

        public RectangleF GetBounds(T element)
        {
            return m_boundsMap[element];
        }

        public void Add(T element, RectangleF bounds)
        {
            bool added = false;
            while (!added)
            {
                if (m_root.Bounds.Left > bounds.Left) //Need to expand to the left
                {
                    if (m_root.Bounds.Top > bounds.Top) //Need to expand to the top
                    {
                        m_root = m_root.ExpandLeftTop();
                    }
                    else
                    {
                        m_root = m_root.ExpandLeftBottom();
                    }
                }
                else if (m_root.Bounds.Top > bounds.Top) //Need to expand to the top
                {
                    m_root = m_root.ExpandRightTop();
                }
                else if (m_root.Bounds.Bottom < bounds.Bottom || //Need to expand to the bottom
                         m_root.Bounds.Right < bounds.Right)     //Need to expand to the right
                {
                    m_root = m_root.ExpandRightBottom();
                }
                else
                {
                    m_root.Add(element, bounds);
                    added = true;
                }
            }
            m_boundsMap[element] = bounds;
        }

        public void Remove(T node)
        {
            RectangleF area = m_boundsMap[node];
            if (!m_root.Bounds.Contains(area))
                throw new InvalidOperationException("Tried to remove element whose bounds are outside the root nodes bounds");
            m_root.Remove(node, area);
        }

        public IEnumerable<T> FindTouchingRegion(RectangleF bounds)
        {
            return m_root.FindTouchingRegion(bounds);
        }

        public class Element : IEnumerable<T>
        {
            List<T> Data = new List<T>();
            List<Tuple<T, RectangleF>> ExtraData = new List<Tuple<T, RectangleF>>();
            Element Lower00 = null;
            Element Lower01 = null;
            Element Lower10 = null;
            Element Lower11 = null;
            List<Element> LowerNonNull = new List<Element>(4);
            internal readonly RectangleF Bounds;
            internal readonly RectangleF Bounds00;
            internal readonly RectangleF Bounds01;
            internal readonly RectangleF Bounds10;
            internal readonly RectangleF Bounds11;
            int m_splitSize = 2;

            public Element(RectangleF bounds)
            {
                Bounds = bounds;
                var center = bounds.Center();
                Bounds00 = RectangleF.FromLTRB(bounds.Left, bounds.Top, center.X, center.Y);
                Bounds01 = RectangleF.FromLTRB(bounds.Left, center.Y, center.X, bounds.Bottom);
                Bounds10 = RectangleF.FromLTRB(center.X, bounds.Top, Bounds.Right, center.Y);
                Bounds11 = RectangleF.FromLTRB(center.X, center.Y, Bounds.Right, bounds.Bottom);
            }

            public Element Add(T element, RectangleF area)
            {
                if (ExtraData.Count >= m_splitSize)
                {
                    m_splitSize = 0;
                    foreach (var x in ExtraData)
                        ReallyAdd(x.Item1, x.Item2);
                    ExtraData.Clear();
                    return ReallyAdd(element, area);
                }
                else
                {
                    ExtraData.Add(Tuple.Create(element, area));
                    return this;
                }
            }

            private bool TooSmall(RectangleF area)
            {
                return area.Width * area.Height < 1;//Avoid subdividing forever
            }

            private Element ReallyAdd(T element, RectangleF area)
            {
                var center = Bounds.Center();
                if (!TooSmall(area))
                {
                    if (area.Top > center.Y) //It's in the bottom half only
                    {
                        if (area.Left > center.X) //It's in the right half only
                        {
                            if (Lower11 == null)
                            {
                                Lower11 = new Element(Bounds11);
                                LowerNonNull.Add(Lower11);
                            }
                            return Lower11.Add(element, area);
                        }
                        else if (area.Right <= center.X) //It's in the left half only
                        {
                            if (Lower01 == null)
                            {
                                Lower01 = new Element(Bounds01);
                                LowerNonNull.Add(Lower01);
                            }
                            return Lower01.Add(element, area);
                        }
                    }
                    else if (area.Bottom <= center.Y) //It's in the top half only
                    {
                        if (area.Left > center.X) //It's in the right half only
                        {
                            if (Lower10 == null)
                            {
                                Lower10 = new Element(Bounds10);
                                LowerNonNull.Add(Lower10);
                            }
                            return Lower10.Add(element, area);
                        }
                        else if (area.Right <= center.X) //It's in the left half only
                        {
                            if (Lower00 == null)
                            {
                                Lower00 = new Element(Bounds00);
                                LowerNonNull.Add(Lower00);
                            }
                            return Lower00.Add(element, area);
                        }
                    }
                }

                Data.Add(element);
                return this;
            }

            public bool Remove(T node, RectangleF area)
            {
                if (ExtraData.Remove(Tuple.Create(node, area)))
                    return true;
                var center = Bounds.Center();
                if (!TooSmall(area))
                {
                    if (area.Top > center.Y) //It's in the bottom half only
                    {
                        if (area.Left > center.X) //It's in the right half only
                        {
                            if (Lower11 != null)
                                return Lower11.Remove(node, area);
                        }
                        else if (area.Right <= center.X) //It's in the left half only
                        {
                            if (Lower01 != null)
                                return Lower01.Remove(node, area);
                        }
                    }
                    else if (area.Bottom <= center.Y) //It's in the top half only
                    {
                        if (area.Left > center.X) //It's in the right half only
                        {
                            if (Lower10 != null)
                                return Lower10.Remove(node, area);
                        }
                        else if (area.Right <= center.X) //It's in the left half only
                        {
                            if (Lower00 != null)
                                return Lower00.Remove(node, area);
                        }
                    }
                }

                //Once we've descended as far as we can. Use this levels data.
                return Data.Remove(node);
            }

            /// <summary>
            /// For debug purposes only
            /// </summary>
            /// <returns></returns>
            public RectangleF? GetFirstNodeAreaDebug()
            {
                if (Data.Any())
                    return Bounds;
                else
                    return LowerNonNull.Select(a => a.GetFirstNodeAreaDebug()).FirstOrDefault(a => a != null);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return ExtraData.Select(a => a.Item1).Concat(Data.Concat(LowerNonNull.SelectMany(x => x))).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            internal Element ExpandLeftTop()
            {
                var result = new Element(RectangleF.FromLTRB(Bounds.Left - Bounds.Width, Bounds.Top - Bounds.Height, Bounds.Right, Bounds.Bottom));
                result.Lower11 = this;
                result.LowerNonNull.Add(this);
                return result;
            }

            internal Element ExpandLeftBottom()
            {
                var result = new Element(RectangleF.FromLTRB(Bounds.Left - Bounds.Width, Bounds.Top, Bounds.Right, Bounds.Bottom + Bounds.Height));
                result.Lower10 = this;
                result.LowerNonNull.Add(this);
                return result;
            }

            internal Element ExpandRightBottom()
            {
                var result = new Element(RectangleF.FromLTRB(Bounds.Left, Bounds.Top, Bounds.Right + Bounds.Width, Bounds.Bottom + Bounds.Height));
                result.Lower00 = this;
                result.LowerNonNull.Add(this);
                return result;
            }

            internal Element ExpandRightTop()
            {
                var result = new Element(RectangleF.FromLTRB(Bounds.Left, Bounds.Top - Bounds.Height, Bounds.Right + Bounds.Width, Bounds.Bottom));
                result.Lower01 = this;
                result.LowerNonNull.Add(this);
                return result;
            }

            public IEnumerable<T> FindTouchingRegion(RectangleF bounds)
            {
                if (Bounds.IntersectsWith(bounds))
                {
                    foreach (var d in ExtraData)
                        yield return d.Item1;
                    foreach (var d in Data)
                        yield return d;
                }
                foreach (var lower in LowerNonNull)
                {
                    foreach (var d in lower.FindTouchingRegion(bounds))
                        yield return d;
                }
            }

            internal RectangleF? FindAndRemnove(T n)
            {
                int i = ExtraData.FindIndex(a => a.Item1.Equals(n));
                if (i >= 0)
                {
                    var result = ExtraData[i];
                    ExtraData.RemoveAt(i);
                    return result.Item2;
                }

                if (Data.Remove(n))
                    return Bounds;
                else
                {
                    foreach (var x in LowerNonNull)
                    {
                        var found = x.FindAndRemnove(n);
                        if (found.HasValue)
                            return found;
                    }
                }
                return null;
            }
        }
    }
}

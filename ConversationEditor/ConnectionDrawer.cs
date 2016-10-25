using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Utilities;

namespace ConversationEditor
{
    internal class ConnectionDrawer : Disposable
    {
        private List<Tuple<PointF, PointF, bool>> m_connections = new List<Tuple<PointF, PointF, bool>>();
        private LineDrawer m_drawer;

        public ConnectionDrawer(IColorScheme scheme)
        {
            m_drawer = new LineDrawer(scheme);
        }

        //public void Draw(Graphics g, RectangleUnion obstacles)
        //{
        //    foreach (var t in m_connections)
        //    {
        //        LineDrawer.ConnectPoints(g, t.Item1, t.Item2, obstacles.Obstacles);
        //    }
        //}

        public void Draw(Graphics g)
        {
            foreach (var t in m_connections)
            {
                m_drawer.ConnectPoints(g, t.Item1, t.Item2, t.Item3);
            }
        }

        internal void Add(PointF from, PointF to, bool selected)
        {
            m_connections.Add(Tuple.Create(from, to, selected));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_drawer.Dispose();
            }
        }
    }
}

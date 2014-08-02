using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Utilities
{
    public class Polygon
    {
        private PointF[] m_vertices;

        /// <param name="vertices">Clockwise winding</param>
        public Polygon(params PointF[] vertices)
        {
            m_vertices = vertices.ToArray();
        }

    //    public void Grow(float amount)
    //    {
    //        for (int i = 0; i < m_vertices.Length; i++)
    //        {
    //            int j = (i + 1) % m_vertices.Length;
    //            m_vertices[j].Take(m_vertices[i]);
    //        }
    //    }
    }
}

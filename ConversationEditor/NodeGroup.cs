using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Drawing;
using System.Drawing.Drawing2D;
using Utilities;

namespace ConversationEditor
{
    using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI>;
    using System.Diagnostics;

    public class NodeGroupRenderer : IGUI
    {
        ColorScheme m_scheme;

        public NodeGroupRenderer(RectangleF area, ColorScheme scheme)
        {
            m_area = area;
            m_scheme = scheme;
        }

        RectangleF m_area;
        public RectangleF Area
        {
            get { return m_area; }
            set { m_area = value; }
        }

        public void UpdateArea() { } //The area isn't calculated automatically

        public void Draw(Graphics g, bool selected)
        {
            if (selected)
                g.FillRectangle(new SolidBrush(m_scheme.GroupBackgroundSelected), m_area);
            else
                g.FillRectangle(new SolidBrush(m_scheme.GroupBackgroundUnselected), m_area);
            g.DrawRectangle(m_scheme.ForegroundPen, m_area);
        }

        public void MoveTo(PointF point)
        {
            m_area.X = point.X - m_area.Width / 2.0f;
            m_area.Y = point.Y - m_area.Height / 2.0f;
        }

        public void MoveTop(float y)
        {
            m_area.Height = m_area.Bottom - y;
            m_area.Location = new PointF(m_area.Left, y);
        }

        public void MoveBottom(float y)
        {
            m_area.Height = y - m_area.Top;
        }

        public void MoveLeft(float x)
        {
            m_area.Width = m_area.Right - x;
            m_area.Location = new PointF(x, m_area.Top);
        }

        public void MoveRight(float x)
        {
            m_area.Width = x - m_area.Left;
        }
    }

    public class NodeGroup : IRenderable<NodeGroupRenderer>
    {
        NodeGroupRenderer m_renderer;

        public NodeGroup(RectangleF area, IEnumerable<ID<NodeTemp>> contents, ColorScheme scheme) : this(area, scheme)
        {
            Contents.UnionWith(contents);
        }

        public static NodeGroup Make<TNode2>(IEnumerable<TNode2> contents, ColorScheme scheme) where TNode2 : class, IGraphNode, IRenderable<IGUI>
        {
            var l = contents.Min(n => n.Renderer.Area.Left) - 20;
            var r = contents.Max(n => n.Renderer.Area.Right) + 20;
            var t = contents.Min(n => n.Renderer.Area.Top) - 20;
            var b = contents.Max(n => n.Renderer.Area.Bottom) + 20;
            return new NodeGroup(RectangleF.FromLTRB(l, t, r, b), contents.Select(n => n.Id), scheme);
        }

        public NodeGroup(RectangleF area, ColorScheme scheme)
        {
            m_renderer = new NodeGroupRenderer(area, scheme);
        }

        public NodeGroupRenderer Renderer
        {
            get { return m_renderer; }
        }

        public HashSet<ID<NodeTemp>> Contents = new HashSet<ID<NodeTemp>>();
    }
}
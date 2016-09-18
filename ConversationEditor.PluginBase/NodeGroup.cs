using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Conversation;
using Utilities;
using Utilities.UI;

namespace ConversationEditor
{
    using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;

    public class NodeGroupRenderer : IGui
    {
        public NodeGroupRenderer(RectangleF area)
        {
            m_area = area;
        }

        RectangleF m_area;

        public event Action<Changed<RectangleF>> AreaChanged;

        public RectangleF Area
        {
            get { return m_area; }
            set
            {
                var old = m_area;
                m_area = value;
                AreaChanged.Execute(Changed.Create(old, m_area));
            }
        }

        public void UpdateArea() { } //The area isn't calculated automatically

        public void Draw(Graphics g, bool selected, ColorScheme scheme)
        {
            if (selected)
            {
                using (var brush = new SolidBrush(scheme.GroupBackgroundSelected))
                    g.FillRectangle(brush, m_area);
            }
            else
            {
                using (var brush = new SolidBrush(scheme.GroupBackgroundUnselected))
                    g.FillRectangle(brush, m_area);
            }
            g.DrawRectangle(scheme.ForegroundPen, m_area);
        }

        public void MoveTo(PointF location)
        {
            var old = m_area;
            m_area.X = location.X - m_area.Width / 2.0f;
            m_area.Y = location.Y - m_area.Height / 2.0f;
            AreaChanged.Execute(Changed.Create(old, m_area));
        }

        public void MoveTop(float y)
        {
            var old = m_area;
            m_area.Height = m_area.Bottom - y;
            m_area.Location = new PointF(m_area.Left, y);
            AreaChanged.Execute(Changed.Create(old, m_area));
        }

        public void MoveBottom(float y)
        {
            var old = m_area;
            m_area.Height = y - m_area.Top;
            AreaChanged.Execute(Changed.Create(old, m_area));
        }

        public void MoveLeft(float x)
        {
            var old = m_area;
            m_area.Width = m_area.Right - x;
            m_area.Location = new PointF(x, m_area.Top);
            AreaChanged.Execute(Changed.Create(old, m_area));
        }

        public void MoveRight(float x)
        {
            var old = m_area;
            m_area.Width = x - m_area.Left;
            AreaChanged.Execute(Changed.Create(old, m_area));
        }
    }

    public class NodeGroup : IRenderable<NodeGroupRenderer>
    {
        private NodeGroupRenderer m_renderer;
        private HashSet<Id<NodeTemp>> m_contents = new HashSet<Id<NodeTemp>>();

        //Renderer can never change so these callbacks will never be triggered
        public event Action RendererChanging { add { } remove { } }
        public event Action RendererChanged { add { } remove { } }

        public NodeGroup(RectangleF area, IEnumerable<Id<NodeTemp>> contents)
                : this(area)
        {
            m_contents.UnionWith(contents);
        }

        public static NodeGroup Make<TNode>(IEnumerable<TNode> contents) where TNode : class, IConversationNode, IRenderable<IGui>
        {
            var l = contents.Min(n => n.Renderer.Area.Left) - 20;
            var r = contents.Max(n => n.Renderer.Area.Right) + 20;
            var t = contents.Min(n => n.Renderer.Area.Top) - 20;
            var b = contents.Max(n => n.Renderer.Area.Bottom) + 20;
            return new NodeGroup(RectangleF.FromLTRB(l, t, r, b), contents.Select(n => n.Id));
        }

        public NodeGroup(RectangleF area)
        {
            m_renderer = new NodeGroupRenderer(area);
        }

        public NodeGroupRenderer Renderer
        {
            get { return m_renderer; }
        }

        public HashSet<Id<NodeTemp>> Contents
        {
            get { return m_contents; }
        }
    }
}

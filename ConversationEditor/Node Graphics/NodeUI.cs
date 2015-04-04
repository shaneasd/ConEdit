﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Utilities;
using Conversation;

namespace ConversationEditor
{
    public abstract class NodeUI : INodeGUI
    {
        public interface IFactory //TODO: Distinguish between domain node renderers and conversation node renderers
        {
            bool WillRender(ID<NodeTypeTemp> nodeType);

            string DisplayName { get; }

            Guid Guid { get; }

            INodeGUI GetRenderer(ConversationNode<INodeGUI> n, PointF p, Func<ID<LocalizedText>, string> localizer, Func<IDataSource> datasource);
        }

        public NodeUI(ConversationNode<INodeGUI> node, PointF p)
        {
            m_node = node;
            m_pos = p;
        }

        public abstract string DisplayName { get; }
        protected abstract SizeF CalculateArea(Graphics g);
        protected abstract void InnerDraw(Graphics g, bool selected);

        private readonly ConversationNode<INodeGUI> m_node;
        public ConversationNode<INodeGUI> Node { get { return m_node; } }

        protected SizeF m_size = SizeF.Empty;
        protected PointF m_pos;
        public RectangleF Area { get { return new RectangleF(m_pos.Take(m_size.Width / 2.0f, m_size.Height / 2.0f), m_size); } }

        public bool Contains(Point p)
        {
            return Area.Contains(p);
        }

        public void MoveTo(PointF location)
        {
            m_pos = location;
        }

        public void UpdateArea()
        {
            using (Bitmap b = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(b))
                m_size = CalculateArea(g);
        }


        public void Draw(Graphics g, bool selected, ColorScheme scheme)
        {
            UpdateArea();

            InnerDraw(g, selected);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Utilities;
using Conversation;

namespace ConversationEditor
{
    public abstract class NodeUI : Disposable, INodeGui
    {
        public interface IFactory //TODO: Distinguish between domain node renderers and conversation node renderers
        {
            bool WillRender(Id<NodeTypeTemp> nodeType);

            string DisplayName { get; }

            Guid Guid { get; }

            INodeGui GetRenderer(ConversationNode<INodeGui> n, PointF p, Func<Id<LocalizedText>, string> localizer, Func<IDataSource> datasource);
        }

        protected NodeUI(ConversationNode<INodeGui> node, PointF p)
        {
            m_node = node;
            m_pos = p;
        }

        protected abstract SizeF CalculateArea(Graphics g);
        protected abstract void InnerDraw(Graphics g, bool selected);

        private readonly ConversationNode<INodeGui> m_node;
        public ConversationNode<INodeGui> Node { get { return m_node; } }

        private SizeF m_size = SizeF.Empty;
        private PointF m_pos;

        public event Action<Changed<RectangleF>> AreaChanged;

        public RectangleF Area { get { return new RectangleF(m_pos.Take(m_size.Width / 2.0f, m_size.Height / 2.0f), m_size); } }

        public bool Contains(Point p)
        {
            return Area.Contains(p);
        }

        public void MoveTo(PointF location)
        {
            var old = Area;
            m_pos = location;
            AreaChanged.Execute(Changed.Create(old, Area));
        }

        public void UpdateArea()
        {
            //TODO: Create the graphics once and cache it
            using (Bitmap b = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(b))
            {
                var newsize = CalculateArea(g);
                if (newsize != m_size)
                {
                    var old = Area;
                    m_size = newsize;
                    AreaChanged.Execute(Changed.Create(old, Area));
                }
            }
        }


        public void Draw(Graphics g, bool selected, ColorScheme scheme)
        {
            UpdateArea();

            InnerDraw(g, selected);
        }
    }
}

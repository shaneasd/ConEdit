using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Utilities;
using Conversation;
using System.Diagnostics;

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
            Area = new RectangleF(p, SizeF.Empty);
        }

        protected abstract SizeF CalculateArea(Graphics g);
        protected abstract void InnerDraw(Graphics g, bool selected);

        private readonly ConversationNode<INodeGui> m_node;
        public ConversationNode<INodeGui> Node { get { return m_node; } }

        public event Action<Changed<RectangleF>> AreaChanged;

        private RectangleF m_area;
        public RectangleF Area
        {
            get { return m_area; }
            private set
            {
                var old = m_area;
                m_area = value;
                //Debug.WriteLine("Moving {0} -> \n {1}", old, value);
                AreaChanged.Execute(Changed.Create(old, value));
            }
        }

        public bool Contains(Point p)
        {
            return Area.Contains(p);
        }

        public void MoveTo(PointF location)
        {
            Area = new RectangleF(location.Take(new PointF(Area.Size.Width * 0.5f, Area.Size.Height * 0.5f)), Area.Size);
        }

        public void UpdateArea()
        {
            //TODO: Create the graphics once and cache it
            using (Bitmap b = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(b))
            {
                var newsize = CalculateArea(g);
                if (newsize != Area.Size)
                {
                    Area = new RectangleF(Area.Center().Take(new PointF(newsize.Width * 0.5f, newsize.Height * 0.5f)), newsize);
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

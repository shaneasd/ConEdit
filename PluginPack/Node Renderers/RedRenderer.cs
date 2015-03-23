using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using Conversation;
using System.Drawing;

namespace PluginPack
{
    public class RedRenderer : NodeUI
    {
        public class Factory : NodeUI.IFactory
        {
            public static Factory Instance = new Factory();

            public bool WillRender(ID<NodeTypeTemp> nodeType)
            {
                return nodeType != SpecialNodes.START_GUID;
            }

            public string DisplayName
            {
                get { return "Red Renderer"; }
            }

            public INodeGUI GetRenderer(ConversationNode<INodeGUI> n, PointF p, Func<ID<LocalizedText>, string> localizer, Func<IDataSource> datasource)
            {
                return new RedRenderer(n, p, localizer);
            }

            static Guid m_guid = Guid.Parse("7166fb2a-c457-4d0a-8c93-906e925a50ae");
            public Guid Guid
            {
                get { return m_guid; }
            }
        }

        public RedRenderer(ConversationNode<Conversation.INodeGUI> node, PointF p, Func<ID<LocalizedText>, string> localizer) :
            base(node, p)
        {
        }

        public override string DisplayName
        {
            get { return Factory.Instance.DisplayName; }
        }

        protected override void InnerDraw(System.Drawing.Graphics g, bool selected)
        {
            g.FillRectangle(Brushes.Red, this.Area);
        }

        protected override SizeF CalculateArea(System.Drawing.Graphics g)
        {
            return new SizeF(100, 100);
        }
    }
}

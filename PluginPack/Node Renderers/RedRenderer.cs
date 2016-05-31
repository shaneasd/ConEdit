using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using Conversation;
using System.Drawing;

namespace PluginPack
{
    public class RedRendererFactory : NodeUI.IFactory
    {
        private static RedRendererFactory m_instance = new RedRendererFactory();
        public static RedRendererFactory Instance { get { return m_instance; } }

        public bool WillRender(Id<NodeTypeTemp> nodeType)
        {
            return nodeType != SpecialNodes.Start;
        }

        public string DisplayName
        {
            get { return "Red Renderer"; }
        }

        public INodeGui GetRenderer(ConversationNode<INodeGui> n, PointF p, Func<Id<LocalizedText>, string> localizer, Func<IDataSource> datasource)
        {
            return new RedRenderer(n, p);
        }

        static Guid s_guid = Guid.Parse("7166fb2a-c457-4d0a-8c93-906e925a50ae");
        public Guid Guid
        {
            get { return s_guid; }
        }
    }

    public class RedRenderer : NodeUI
    {
        public RedRenderer(ConversationNode<ConversationEditor.INodeGui> node, PointF p) :
            base(node, p)
        {
        }

        protected override void InnerDraw(System.Drawing.Graphics g, bool selected)
        {
            g.FillRectangle(Brushes.Red, this.Area);
        }

        protected override SizeF CalculateArea(System.Drawing.Graphics g)
        {
            return new SizeF(100, 100);
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}

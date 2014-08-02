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
        public new class Factory : NodeUI.Factory
        {
            public static Factory Instance = new Factory();

            public override bool WillRender(ID<NodeTypeTemp> nodeType)
            {
                return nodeType != SpecialNodes.START_GUID;
            }

            public override string DisplayName
            {
                get { return "Red Renderer"; }
            }

            public override INodeGUI GetRenderer(ConversationNode<INodeGUI> n, PointF p, Func<ID<LocalizedText>, string> localizer)
            {
                return new RedRenderer(n, p, localizer);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Drawing;
using Utilities;
using System.Reflection;
using System.IO;

namespace ConversationEditor
{
    public class StartGUI : NodeUI
    {
        public new class Factory : NodeUI.Factory
        {
            public static Factory Instance = new Factory();

            public override bool WillRender(ID<NodeTypeTemp> nodeType)
            {
                return nodeType == SpecialNodes.START_GUID;
            }

            public override string DisplayName
            {
                get { return "Start Node Renderer"; }
            }

            public override INodeGUI GetRenderer(ConversationNode<INodeGUI> n, PointF p, Func<ID<LocalizedText>, string> localizer)
            {
                return new StartGUI(n, p, localizer);
            }
        }

        Image image;
        public StartGUI(ConversationNode<INodeGUI> node, PointF p, Func<ID<LocalizedText>, string> localizer)
            : base(node, p)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("ConversationEditor.Resources.Start Icon.png"))
                image = new Bitmap(stream);
        }

        protected override void InnerDraw(Graphics g, bool selected)
        {
            g.DrawImage(image, Area);
            if (selected)
            {
                Pen outline = new Pen(Brushes.Black, 2);
                g.DrawRectangle(outline, Area);
            }
        }

        protected override SizeF CalculateArea(Graphics g)
        {
            return image.Size;
        }

        public override string DisplayName
        {
            get { return Factory.Instance.DisplayName; }
        }
            }
}

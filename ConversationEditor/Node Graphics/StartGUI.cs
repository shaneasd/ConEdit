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
        public class Factory : NodeUI.IFactory
        {
            public static Factory Instance = new Factory();

            public bool WillRender(ID<NodeTypeTemp> nodeType)
            {
                return nodeType == SpecialNodes.START_GUID;
            }

            public string DisplayName
            {
                get { return "Start Node Renderer"; }
            }

            public INodeGUI GetRenderer(ConversationNode<INodeGUI> n, PointF p, Func<ID<LocalizedText>, string> localizer)
            {
                return new StartGUI(n, p, localizer);
            }

            static Guid m_guid = Guid.Parse("346ac22d-6393-4958-8d36-fedff89b40c0");
            public Guid Guid
            {
                get { return m_guid; }
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

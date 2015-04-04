using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using Conversation;
using System.Drawing;
using System.IO;
using Utilities;

namespace Clandestine
{
    public class PartialRenderer : EditableUI
    {
        private Func<IDataSource> m_datasource;
        new public class Factory : NodeUI.IFactory
        {
            public static Factory Instance = new Factory();

            public bool WillRender(ID<NodeTypeTemp> nodeType)
            {
                return true;
            }

            public string DisplayName
            {
                get { return "Partial Renderer"; }
            }

            public INodeGUI GetRenderer(ConversationNode<INodeGUI> n, PointF p, Func<ID<LocalizedText>, string> localizer, Func<IDataSource> datasource)
            {
                return new PartialRenderer(n, p, localizer, datasource);
            }

            static Guid m_guid = Guid.Parse("a24b2f32-571d-4e3b-b091-2712d412ac5e");
            public Guid Guid
            {
                get { return m_guid; }
            }
        }

        public PartialRenderer(ConversationNode<ConversationEditor.INodeGUI> node, PointF p, Func<ID<LocalizedText>, string> localizer, Func<IDataSource> datasource) :
            base(node, p, localizer)
        {
            m_datasource = datasource;
        }

        protected override bool ShouldRender(Parameter p)
        {
            var config = m_datasource().GetNode(Node.Type).GetParameterConfig(p.Id);
            return !DontRenderConfig.TryGet(config);
        }
        
        public override string DisplayName
        {
            get { return Factory.Instance.DisplayName; }
        }
    }
}

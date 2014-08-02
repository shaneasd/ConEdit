using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Conversation;
using System.Drawing;

namespace ConversationEditor
{
    public class NodeRendererChoice : TypeChoice
    {
        NodeUI.Factory m_factory;

        public NodeRendererChoice(NodeUI.Factory factory)
            : base(factory.GetType())
        {
            m_factory = factory;
        }

        public NodeRendererChoice(string assembly, string type)
            : base(assembly, type)
        {
            m_factory = (NodeUI.Factory)Assembly.LoadFrom(assembly).GetType(type).GetConstructor(new Type[0]).Invoke(new object[0]);
        }

        public bool WillRender(ID<NodeTypeTemp> guid)
        {
            return m_factory.WillRender(guid);
        }

        public override string DisplayName
        {
            get
            {
                return m_factory.DisplayName;
            }
        }

        public INodeGUI GetRenderer(ConversationNode<INodeGUI> n, PointF p, Func<ID<LocalizedText>, string> localizer)
        {
            return m_factory.GetRenderer(n, p, localizer);
        }

        public static NodeRendererChoice DefaultConversation(ID<NodeTypeTemp> guid)
        {
            if (guid == SpecialNodes.START_GUID)
                return new NodeRendererChoice(StartGUI.Factory.Instance);
            else if (guid == SpecialNodes.TERMINATOR_GUID)
                return new NodeRendererChoice(TerminatorGUI.Factory.Instance);
            else
                return new NodeRendererChoice(EditableUI.Factory.Instance);
        }

        public static NodeRendererChoice DefaultDomain(ID<NodeTypeTemp> guid)
        {
                return new NodeRendererChoice(DomainNodeRenderer.Factory.Instance);
        }
    }
}

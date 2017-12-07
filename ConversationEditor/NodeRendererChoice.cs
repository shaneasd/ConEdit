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
        NodeUI.IFactory m_factory;

        public NodeRendererChoice(NodeUI.IFactory factory)
            : base(factory.GetType())
        {
            m_factory = factory;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile", Justification = "Can't see an alternative method...")]
        public NodeRendererChoice(string assembly, string type)
            : base(assembly, type)
        {
            m_factory = (NodeUI.IFactory)Assembly.LoadFile(assembly).GetType(type).GetConstructor(new Type[0]).Invoke(new object[0]);
        }

        public bool WillRender(Id<NodeTypeTemp> guid)
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

        public INodeGui GetRenderer(ConversationNode<INodeGui> n, PointF p, Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localizer, Func<IDataSource> datasource)
        {
            return m_factory.GetRenderer(n, p, localizer, datasource);
        }

        public static NodeRendererChoice DefaultConversation(Id<NodeTypeTemp> guid)
        {
            if (guid == SpecialNodes.Start)
                return new NodeRendererChoice(StartGuiFactory.Instance);
            else if (guid == SpecialNodes.Terminator)
                return new NodeRendererChoice(TerminatorGuiFactory.Instance);
            else
                return new NodeRendererChoice(EditableUIFactory.Instance);
        }

        public static NodeRendererChoice DefaultDomain(Id<NodeTypeTemp> guid)
        {
                return new NodeRendererChoice(DomainNodeRendererFactory.Instance);
        }
    }
}

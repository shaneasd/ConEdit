using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Conversation;
using Utilities;

using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI>;

namespace ConversationEditor
{
    public interface INodeFactory<TNode> : INodeFactory<TNode, NodeUIData> where TNode : IGraphNode, IConfigurable
    {
    }


    public interface INodeFactory : INodeFactory<ConversationNode>
    {
    }

    public class NodeFactory : INodeFactory
    {
        Func<ID<LocalizedText>, string> m_localizer;

        private TypeMapConfig<ID<NodeTypeTemp>, NodeRendererChoice> m_config;
        public NodeFactory(TypeMapConfig<ID<NodeTypeTemp>, NodeRendererChoice> config, Func<ID<LocalizedText>, string> localizer)
        {
            m_localizer = localizer;
            m_config = config;
            m_config.ValueChanged += () => UpdateRenderers();
        }

        private List<ConversationNode> m_toUpdate = new List<ConversationNode>();
        public void UpdateRenderers()
        {
            foreach (var n in m_toUpdate.ToList())
            {
                n.SetRenderer(nn => MakeRenderer(nn, nn.Renderer.Area.Center()));
                m_toUpdate.Remove(n);
            }
        }

        public INodeGUI MakeRenderer(ConversationNode n, PointF p)
        {
            if (n.m_data is UnknownEditable)
                return new UnknownNodeRenderer(n, p);

            var choice = m_config[n.Type];
            m_toUpdate.Add(n);
            return choice.GetRenderer(n, p, m_localizer);
        }

        public INodeGUI MakeCorruptedRenderer(ConversationNode n, PointF p)
        {
            if (n.m_data is UnknownEditable)
                return new UnknownNodeRenderer(n, p);
            return new CorruptedNodeRenderer(n, n.Renderer == null ? p : n.Renderer.Area.Center());
        }

        public ConversationNode MakeNode(IEditable e, NodeUIData uiData)
        {
            PointF p = uiData.Area.Center();
            var result = new ConversationNode<INodeGUI>(e, n => MakeRenderer(n, n.Renderer == null ? p : n.Renderer.Area.Center()), n => MakeCorruptedRenderer(n, p));
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Conversation;
using Utilities;

using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGUI>;

namespace ConversationEditor
{
    internal interface INodeFactory<TNode> : INodeFactory<TNode, NodeUIData> where TNode : IGraphNode, IConfigurable
    {
    }


    internal interface INodeFactory : INodeFactory<ConversationNode>
    {
    }

    internal class NodeFactory : INodeFactory
    {
        private Func<ID<NodeTypeTemp>, ConversationNode, PointF, INodeGUI> GetNodeRendererChoice;

        public NodeFactory(TypeMapConfig<ID<NodeTypeTemp>, NodeRendererChoice> config, Func<ID<LocalizedText>, string> localizer, Func<IDataSource> datasource)
        {
            GetNodeRendererChoice = (id, n, p) => config[id].GetRenderer(n, p, localizer, datasource);
            config.ValueChanged += () => UpdateRenderers();
        }

        public NodeFactory(MapConfig<ID<NodeTypeTemp>, Guid> config, IEnumerable<NodeUI.IFactory> factories, Action<Action> changedCallback, Func<ID<LocalizedText>, string> localizer, Func<IDataSource> datasource)
        {
            var nodeRenderers = factories.ToDictionary(n => n.Guid, n => n);
            GetNodeRendererChoice = (id, n, p) => nodeRenderers[config[id]].GetRenderer(n, p, localizer, datasource);
            changedCallback(UpdateRenderers);
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

            m_toUpdate.Add(n);

            return GetNodeRendererChoice(n.Type, n, p);
        }

        private INodeGUI MakeCorruptedRenderer(ConversationNode n, PointF p)
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

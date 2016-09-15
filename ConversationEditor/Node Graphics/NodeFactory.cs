using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Conversation;
using Utilities;

using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;

namespace ConversationEditor
{
    public interface INodeFactory<TNode, in TNodeUI> where TNode : IGraphNode, IConfigurable
    {
        TNode MakeNode(IEditable e, TNodeUI uiData);
    }

    internal interface INodeFactory<TNode> : INodeFactory<TNode, NodeUIData> where TNode : IGraphNode, IConfigurable
    {
    }


    internal interface INodeFactory : INodeFactory<ConversationNode>
    {
    }

    internal class NodeFactory : INodeFactory
    {
        private Func<Id<NodeTypeTemp>, ConversationNode, PointF, INodeGui> GetNodeRendererChoice;

        public NodeFactory(TypeMapConfig<Id<NodeTypeTemp>, NodeRendererChoice> config, Func<Id<LocalizedText>, string> localizer, Func<IDataSource> datasource)
        {
            GetNodeRendererChoice = (id, n, p) => config[id].GetRenderer(n, p, localizer, datasource);
            config.ValueChanged += () => UpdateRenderers();
        }

        public NodeFactory(MapConfig<Id<NodeTypeTemp>, Guid> config, IEnumerable<NodeUI.IFactory> factories, Action<Action> changedCallback, Func<Id<LocalizedText>, string> localizer, Func<IDataSource> datasource)
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

        public INodeGui MakeRenderer(ConversationNode n, PointF p)
        {
            if (n.m_data is UnknownEditable)
                return new UnknownNodeRenderer(n, p);

            m_toUpdate.Add(n);

            return GetNodeRendererChoice(n.Type, n, p);
        }

        private static INodeGui MakeCorruptedRenderer(ConversationNode n, PointF p)
        {
            if (n.m_data is UnknownEditable)
                return new UnknownNodeRenderer(n, p);
            return new CorruptedNodeRenderer(n, n.Renderer == null ? p : n.Renderer.Area.Center());
        }

        public ConversationNode MakeNode(IEditable e, NodeUIData uiData)
        {
            PointF p = uiData.Area.Center();
            var result = new ConversationNode<INodeGui>(e, n => MakeRenderer(n, n.Renderer == null ? p : n.Renderer.Area.Center()), n => MakeCorruptedRenderer(n, p));
            return result;
        }
    }
}

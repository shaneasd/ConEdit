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
    /// <summary>
    /// Implementers of this interface must be threadsafe.
    /// i.e. calls to MakeNode must be able to be safely made from worker threads potentially in parallel
    /// </summary>
    public interface INodeFactory<TNode, in TNodeUI> where TNode : IConversationNode, IConfigurable
    {
        TNode MakeNode(IConversationNodeData e, TNodeUI uiData);
    }

    /// <summary>
    /// See threadsafety note on INodeFactory<TNode, in TNodeUI>
    /// </summary>
    public interface INodeFactory : INodeFactory<ConversationNode, NodeUIData>
    {
    }

    /// <summary>
    /// This class is threadsafe after construction. i.e. its public members can be called from multiple threads in parallel
    /// </summary>
    public class NodeFactory : INodeFactory
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

        object m_lock = new object();

        private List<ConversationNode> m_toUpdate = new List<ConversationNode>();
        public void UpdateRenderers()
        {
            lock (m_lock)
            {
                foreach (var n in m_toUpdate.ToList())
                {
                    n.SetRenderer(nn => MakeRenderer(nn, nn.Renderer.Area.Center()));
                    m_toUpdate.Remove(n);
                }
            }
        }

        public INodeGui MakeRenderer(ConversationNode n, PointF p)
        {
            if (n.Data is UnknownEditable)
                return new UnknownNodeRenderer(n, p);

            lock (m_lock)
            {
                m_toUpdate.Add(n);
                return GetNodeRendererChoice(n.Data.NodeTypeId, n, p);
            }
        }

        private static INodeGui MakeCorruptedRenderer(ConversationNode n, PointF p)
        {
            if (n.Data is UnknownEditable)
                return new UnknownNodeRenderer(n, p);
            return new CorruptedNodeRenderer(n, n.Renderer == null ? p : n.Renderer.Area.Center());
        }

        public ConversationNode MakeNode(IConversationNodeData e, NodeUIData uiData)
        {
            PointF p = uiData.Area.Center();
            var result = new ConversationNode<INodeGui>(e, n => MakeRenderer(n, n.Renderer == null ? p : n.Renderer.Area.Center()), n => MakeCorruptedRenderer(n, p));
            return result;
        }
    }
}

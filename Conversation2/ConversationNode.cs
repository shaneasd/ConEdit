using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Drawing;

namespace Conversation
{
    public interface INodeUI<TNodeUI>
        where TNodeUI : INodeUI<TNodeUI>
    {
        ConversationNode<TNodeUI> Node { get; }
    }

    public interface IRenderable<out TRenderer>
    {
        TRenderer Renderer { get; }
    }

    public class ConversationNode<TNodeUI> : IConversationNode, IRenderable<TNodeUI>, IConfigurable
        where TNodeUI : INodeUI<TNodeUI>
    {
        private Func<ConversationNode<TNodeUI>, TNodeUI> m_nodeUI;
        private Func<ConversationNode<TNodeUI>, TNodeUI> m_corruptedUI;
        private Func<ConversationNode<TNodeUI>, TNodeUI> m_currentRenderer;

        private TNodeUI m_renderer;
        public TNodeUI Renderer { get { return m_renderer; } }

        /// <summary>
        /// Attempt to decorrupt the node if it is corrupt. Does nothing if not corrupt.
        /// This is only called on load so doesn't need to be undoable
        /// </summary>
        public void TryDecorrupt()
        {
            if (m_data.Parameters.Any(p => p.Corrupted))
            {
                m_data.TryDecorrupt();
                UpdateRendererCorruption();
            }
        }

        public void UpdateRendererCorruption()
        {
            if (m_data.Parameters.All(p => !p.Corrupted))
            {
                if (m_currentRenderer != m_nodeUI)
                {
                    m_currentRenderer = m_nodeUI;
                    m_renderer = m_currentRenderer(this);
                }
            }
            else
            {
                if (m_currentRenderer != m_corruptedUI)
                {
                    m_currentRenderer = m_corruptedUI;
                    m_renderer = m_currentRenderer(this);
                }
            }
        }

        public void SetRenderer(Func<ConversationNode<TNodeUI>, TNodeUI> newRenderer)
        {
            if (m_currentRenderer == m_nodeUI)
            {
                m_currentRenderer = newRenderer;
                m_renderer = newRenderer(this);
            }
            m_nodeUI = newRenderer;
        }

        public readonly IEditable m_data;
        #region Thin wrapper around m_data
        public ID<NodeTemp> Id { get { return m_data.NodeID; } }
        public ID<NodeTypeTemp> Type { get { return m_data.NodeTypeID; } }
        public event Action Linked { add { m_data.Linked += value; } remove { m_data.Linked -= value; } }
        public IEnumerable<Parameter> Parameters { get { return m_data.Parameters; } }
        public List<NodeData.ConfigData> Config { get { return m_data.Config; } }
        public IEnumerable<Output> Connectors { get { return m_data.Connectors; } }
        public string NodeName { get { return m_data.Name; } }
        public void ChangeId(ID<NodeTemp> id) { m_data.ChangeId(id); }
        #endregion

        public event Action Modified;

        public ConfigureResult Configure(Func<IEditable, ConfigureResult> configure)
        {
            ConfigureResult result = configure(m_data);
            return result.Transformed<ConfigureResult>(sup => new SimpleUndoPair
            {
                Redo = () =>
                {
                    sup.Redo();
                    UpdateRendererCorruption();
                    Modified.Execute();
                },
                Undo = () =>
                {
                    sup.Undo();
                    UpdateRendererCorruption();
                    Modified.Execute();
                }
            }, crno => crno);
        }

        public ConversationNode(IEditable data, Func<ConversationNode<TNodeUI>, TNodeUI> nodeUI, Func<ConversationNode<TNodeUI>, TNodeUI> corruptedUI)
        {
            m_data = data;
            m_nodeUI = nodeUI;
            m_corruptedUI = corruptedUI;

            if (data.Parameters.Any(p => p.Corrupted))
                m_currentRenderer = corruptedUI;
            else
                m_currentRenderer = nodeUI;
            m_renderer = m_currentRenderer(this);
        }

        public Tuple<Action, Action> GetNodeRemoveActions()
        {
            List<Action> redoActions = new List<Action>();
            List<Action> undoActions = new List<Action>();
            foreach (var t in Connectors)
            {
                var disconnectAll = t.DisconnectAll();
                redoActions.Add(disconnectAll.Redo);
                undoActions.Add(disconnectAll.Undo);
            }

            Action undo = () => { foreach (var action in undoActions) action(); };
            Action redo = () => { foreach (var action in redoActions) action(); };
            return Tuple.Create(undo, redo);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Drawing;
using System.Collections.ObjectModel;

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
        event Action RendererChanging;
        event Action RendererChanged;
    }

    public class ConversationNode<TNodeUI> : IConversationNode, IRenderable<TNodeUI>, IConfigurable
        where TNodeUI : INodeUI<TNodeUI>
    {
        private Func<ConversationNode<TNodeUI>, TNodeUI> m_nodeUI;
        private Func<ConversationNode<TNodeUI>, TNodeUI> m_corruptedUI;
        private Func<ConversationNode<TNodeUI>, TNodeUI> m_currentRenderer;

        private TNodeUI m_renderer;
        public TNodeUI Renderer
        {
            get { return m_renderer; }
            private set
            {
                RendererChanging.Execute();
                m_renderer = value;
                RendererChanged.Execute();
            }
        }

        public void UpdateRendererCorruption()
        {
            if (Data.Parameters.All(p => !p.Corrupted))
            {
                if (m_currentRenderer != m_nodeUI)
                {
                    m_currentRenderer = m_nodeUI;
                    Renderer = m_currentRenderer(this);
                }
            }
            else
            {
                if (m_currentRenderer != m_corruptedUI)
                {
                    m_currentRenderer = m_corruptedUI;
                    Renderer = m_currentRenderer(this);
                }
            }
        }

        public void SetRenderer(Func<ConversationNode<TNodeUI>, TNodeUI> newRenderer)
        {
            if (m_currentRenderer == m_nodeUI)
            {
                m_currentRenderer = newRenderer;
                Renderer = newRenderer(this);
            }
            m_nodeUI = newRenderer;
        }

        public IConversationNodeData Data { get; }
        #region Thin wrapper around Data
        public Id<NodeTemp> Id { get { return Data.NodeId; } }
        public Id<NodeTypeTemp> Type { get { return Data.NodeTypeId; } }
        public IEnumerable<IParameter> Parameters { get { return Data.Parameters; } }
        public IReadOnlyList<NodeData.ConfigData> Config { get { return Data.Config; } }
        public IEnumerable<Output> Connectors { get { return Data.Connectors; } }
        public string NodeName { get { return Data.Name; } }
        #endregion

        public event Action Modified;
        public event Action RendererChanging;
        public event Action RendererChanged;

        public ConfigureResult Configure(Func<IConversationNodeData, ConfigureResult> configureData)
        {
            ConfigureResult result = configureData(Data);
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

        public ConversationNode(IConversationNodeData data, Func<ConversationNode<TNodeUI>, TNodeUI> nodeUI, Func<ConversationNode<TNodeUI>, TNodeUI> corruptedUI)
        {
            Data = data;
            m_nodeUI = nodeUI;
            m_corruptedUI = corruptedUI;

            if (data.Parameters.Any(p => p.Corrupted))
                m_currentRenderer = corruptedUI;
            else
                m_currentRenderer = nodeUI;
            Renderer = m_currentRenderer(this);
        }

        public SimpleUndoPair GetNodeRemoveActions()
        {
            List<Action> redoActions = new List<Action>();
            List<Action> undoActions = new List<Action>();
            foreach (var t in Connectors)
            {
                var disconnectAll = t.DisconnectAllActions();
                redoActions.Add(disconnectAll.Redo);
                undoActions.Add(disconnectAll.Undo);
            }

            Action undo = () => { foreach (var action in undoActions) action(); };
            Action redo = () => { foreach (var action in redoActions) action(); };
            return new SimpleUndoPair { Redo = redo, Undo = undo };
        }
    }
}

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
        /// <summary>
        /// Generator for the renderer to use when the node is not corrupted
        /// </summary>
        private Func<ConversationNode<TNodeUI>, TNodeUI> m_uncorruptedUI;

        /// <summary>
        /// Generator for the renderer to use when the node is corrupted
        /// </summary>
        private Func<ConversationNode<TNodeUI>, TNodeUI> m_corruptedUI;

        /// <summary>
        /// Either m_uncorruptedUI or m_corruptedUI based on whether the node is currently corrupted
        /// </summary>
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
                if (m_currentRenderer != m_uncorruptedUI)
                {
                    m_currentRenderer = m_uncorruptedUI;
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
            if (m_currentRenderer == m_uncorruptedUI)
            {
                m_currentRenderer = newRenderer;
                Renderer = newRenderer(this);
            }
            m_uncorruptedUI = newRenderer;
        }

        public IConversationNodeData Data { get; }

        /// <summary>
        /// Triggered after the node has been modified as a result of an action returned from Configure
        /// </summary>
        public event Action Modified;
        public event Action RendererChanging;
        public event Action RendererChanged;

        public ConfigureResult Configure(NodeEditOperation configureData)
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
            m_uncorruptedUI = nodeUI;
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
            foreach (var t in Data.Connectors)
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

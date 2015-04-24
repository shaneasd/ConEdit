using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Drawing;

namespace ConversationEditor
{
    public class MenuAction<TNode> where TNode : IRenderable<IGUI>, IConversationNode, IConfigurable
    {
        private readonly string m_name;
        public string Name { get { return m_name; } }

        private readonly Func<TNode, Point, Action> m_nodeAction;
        public Func<TNode, Point, Action> NodeAction { get { return m_nodeAction; } }

        private readonly Action<Output, Point> m_transitionAction;
        public Action<Output, Point> TransitionAction { get { return m_transitionAction; } }

        private readonly Action<NodeGroup, Point> m_groupAction;
        public Action<NodeGroup, Point> GroupAction { get { return m_groupAction; } }

        private readonly Action<Point> m_emptySpaceAction;
        public Action<Point> EmptySpaceAction { get { return m_emptySpaceAction; } }

        public IEnumerable<MenuAction<TNode>> Children { get { return m_children; } }
        private List<MenuAction<TNode>> m_children = new List<MenuAction<TNode>>();

        public MenuAction<TNode> Add(MenuAction<TNode> child)
        {
            m_children.Add(child);
            return child;
        }

        public MenuAction(string name, Func<TNode, Point, Action> nodeAction, Action<Output, Point> transitionAction,
            Action<NodeGroup, Point> groupAction, Action<Point> emptySpaceAction)
        {
            m_name = name;
            m_nodeAction = nodeAction;
            m_transitionAction = transitionAction;
            m_groupAction = groupAction;
            m_emptySpaceAction = emptySpaceAction;
        }
    }

}

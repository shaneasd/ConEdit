using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Drawing;

namespace ConversationEditor
{
    public class MenuAction<TNode> where TNode : IRenderable<IGui>, IConversationNode, IConfigurable
    {
        public string Name { get; }

        public Func<TNode, Point, Action> NodeAction { get; }

        public Action<Output, Point> TransitionAction { get; }

        public Action<NodeGroup, Point> GroupAction { get; }

        public Action<Point> EmptySpaceAction { get; }

        public IEnumerable<MenuAction<TNode>> Children => m_children;
        private List<MenuAction<TNode>> m_children = new List<MenuAction<TNode>>();

        public MenuAction<TNode> Add(MenuAction<TNode> child)
        {
            m_children.Add(child);
            return child;
        }

        public MenuAction(string name, Func<TNode, Point, Action> nodeAction, Action<Output, Point> transitionAction,
            Action<NodeGroup, Point> groupAction, Action<Point> emptySpaceAction)
        {
            Name = name;
            NodeAction = nodeAction;
            TransitionAction = transitionAction;
            GroupAction = groupAction;
            EmptySpaceAction = emptySpaceAction;
        }
    }

}

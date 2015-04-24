using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Utilities;
using Conversation;

namespace ConversationEditor
{
    internal class ContextMenu<TNode> where TNode : IRenderable<IGUI>, IConversationNode, IConfigurable
    {
        Dictionary<MenuAction<TNode>, ToolStripMenuItem> m_menuActions = new Dictionary<MenuAction<TNode>, ToolStripMenuItem>();
        private readonly ContextMenuStrip m_menu;
        private Control m_control;

        private MouseController<TNode> m_mouseController;
        private readonly Func<Point, Point> ToGraphSpace;
        private readonly Func<bool> ConversationReal;
        public event Action Opening;

        public ContextMenu(ColorScheme scheme, MouseController<TNode> mouseController, Func<Point, Point> toGraphSpace, Func<bool> conversationReal)
        {
            m_mouseController = mouseController;
            ToGraphSpace = toGraphSpace;
            ConversationReal = conversationReal;
            m_menu = new ContextMenuStrip() { RenderMode = ToolStripRenderMode.Professional, Renderer = scheme.ContextMenu };
            m_menu.Opening += (a, b) => b.Cancel = UpdateContextMenu();
        }

        private Point MenuPosition
        {
            get
            {
                return m_control.PointToClient(new Point(m_menu.Left, m_menu.Top));
            }
        }

        internal bool UpdateContextMenu()
        {
            Opening.Execute();

            var point = ToGraphSpace(MenuPosition);

            bool showone = false;
            foreach (MenuAction<TNode> mm in m_menuActions.Keys)
            {
                var m = mm;
                bool show = false;
                m_mouseController.ForClickedOn(point, a => { show = m.NodeAction(a, point) != null; }, a => { show = m.TransitionAction != null; }, (a, b) => { }, a => { show = m.GroupAction != null; }, () => { show = m.EmptySpaceAction != null; });
                bool visible = show & ConversationReal();
                m_menuActions[m].Visible = visible;
                showone |= visible;
            }

            return !showone;
        }

        internal void ResetCustomNodes(params MenuAction<TNode>[] newActions2)
        {
            m_menuActions.Clear();
            m_menu.Items.Clear();
            foreach (var item in newActions2)
                AddCustomNode(item, m_menu.Items);
        }

        private void AddCustomNode(MenuAction<TNode> node, ToolStripItemCollection tsic)
        {
            EventHandler OnClick = (s, e) =>
            {
                Point p = ToGraphSpace(MenuPosition);
                m_mouseController.ForClickedOn(p, n => node.NodeAction(n, p)(), n => node.TransitionAction(n, p), (c, b) => { }, n => node.GroupAction(n, p), () => node.EmptySpaceAction(p));
            };
            ToolStripMenuItem a = new ToolStripMenuItem(node.Name, null, OnClick);

            tsic.Add(a);
            foreach (var n in node.Children)
            {
                AddCustomNode(n, a.DropDownItems);
            }
            m_menuActions[node] = a;
        }

        internal void AttachTo(Control control)
        {
            control.ContextMenuStrip = m_menu;
            m_control = control;
        }
    }
}

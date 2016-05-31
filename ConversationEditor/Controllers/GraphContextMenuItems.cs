using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor.Controllers;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
using Utilities;
using Conversation;
using System.Windows.Forms;
using System.Drawing;

namespace ConversationEditor
{
    internal class DomainContextMenuItems : GraphContextMenuItems
    {
        public DomainContextMenuItems(Action<ConversationNode> findReferences)
            : base(findReferences)
        {
        }

        public override IEnumerable<MenuAction<ConversationNode>> GetMenuActions(IGraphEditorControl<ConversationNode> control)
        {
            foreach (var action in base.GetMenuActions(control))
                yield return action;
            yield return new MenuAction<ConversationNode>("Find References", (n, p) => () => FileReferences(n), null, null, null);
        }
    }

    internal class GraphContextMenuItems : IMenuActionFactory<ConversationNode>
    {
        protected Action<ConversationNode> FileReferences;
        public GraphContextMenuItems(Action<ConversationNode> findReferences)
        {
            FileReferences = findReferences;
        }

        public virtual IEnumerable<MenuAction<ConversationNode>> GetMenuActions(IGraphEditorControl<ConversationNode> control)
        {
            MenuAction<ConversationNode> addNodes = new MenuAction<ConversationNode>("Add Node", (n, p) => null, null, null, p => { });
            AddNodeMenuItem(addNodes, control.DataSource.Nodes, control);
            yield return addNodes;

            yield return new MenuAction<ConversationNode>("Reset Zoom", (n, p) => null, null, null, (p) => { control.GraphScale = 1; });
            yield return new MenuAction<ConversationNode>("Paste", (n, p) => null, null, null, (p) => { control.Paste(p); });
            yield return new MenuAction<ConversationNode>("Delete", (n, p) => () => { control.CurrentFile.Remove(n.Only(), Enumerable.Empty<NodeGroup>()); }, null, null, null);
            yield return new MenuAction<ConversationNode>("Remove Links", (n, p) => () => { foreach (var c in n.Connectors) control.CurrentFile.RemoveLinks(c); }, (i, p) => { control.CurrentFile.RemoveLinks(i); }, null, null);
            yield return new MenuAction<ConversationNode>("Copy ID", (n, p) => control.ShowIds ? () => Clipboard.SetText(n.Id.Serialized()) : (Action)null, null, null, null);
        }

        private void AddNodeMenuItem(MenuAction<ConversationNode> menu, INodeType node, IGraphEditorControl<ConversationNode> control)
        {
            foreach (var n in node.Nodes.OrderBy(n => n.Name))
            {
                var nn = n;
                var name = nn.Name;
                menu.Add(new MenuAction<ConversationNode>(name, (nnn, p) => null, null, null, p => { control.AddNode(nn, p); }));
            }
            foreach (var n in node.ChildTypes)
            {
                var nn = n;
                var a = menu.Add(new MenuAction<ConversationNode>(nn.Name, (nnn, p) => null, null, null, p => { }));
                AddNodeMenuItem(a, nn, control);
            }
        }
    }
}

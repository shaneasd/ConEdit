using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor.Controllers;
using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI>;
using Utilities;
using Conversation;
using System.Windows.Forms;
using System.Drawing;

namespace ConversationEditor
{
    public class DomainContextMenuItems : GraphContextMenuItems
    {
        public DomainContextMenuItems(Action<ConversationNode> findReferences )
            : base(findReferences)
        {
        }

        public override IEnumerable<MenuAction2<ConversationNode>> GetMenuActions(GraphEditorControl<ConversationNode> control)
        {
            foreach (var action in base.GetMenuActions(control))
                yield return action;
            yield return new MenuAction2<ConversationNode>("Find References", (n, p) => () => FileReferences(n), null, null, null);
        }
    }

    public class GraphContextMenuItems : IMenuActionFactory<ConversationNode>
    {
        protected Action<ConversationNode> FileReferences;
        public GraphContextMenuItems(Action<ConversationNode> findReferences)
        {
            FileReferences = findReferences;
        }

        public virtual IEnumerable<MenuAction2<ConversationNode>> GetMenuActions(GraphEditorControl<ConversationNode> control)
        {
            MenuAction2<ConversationNode> addNodes = new MenuAction2<ConversationNode>("Add Node", (n, p) => null, null, null, p => { });
            AddNodeMenuItem(addNodes, control.DataSource.Nodes, control);
            yield return addNodes;

            yield return new MenuAction2<ConversationNode>("Reset Zoom", (n, p) => null, null, null, (p) => { control.GraphScale = 1; });
            yield return new MenuAction2<ConversationNode>("Paste", (n, p) => null, null, null, (p) => { control.Paste(p); });
            yield return new MenuAction2<ConversationNode>("Delete", (n, p) => () => { control.CurrentFile.Remove(n.Only(), Enumerable.Empty<NodeGroup>()); }, null, null, null);
            yield return new MenuAction2<ConversationNode>("Remove Links", (n, p) => null, (i, p) => { control.CurrentFile.RemoveLinks(i); }, null, null);
            yield return new MenuAction2<ConversationNode>("Copy ID", (n, p) => control.ShowIDs ? () => Clipboard.SetText(n.Id.Serialized()) : (Action)null, null, null, null);
        }

        private void AddNodeMenuItem(MenuAction2<ConversationNode> menu, INodeType node, GraphEditorControl<ConversationNode> control)
        {
            foreach (var n in node.Nodes)
            {
                var nn = n;
                var name = nn.Name;
                menu.Add(new MenuAction2<ConversationNode>(name, (nnn, p) => null, null, null, p => { control.AddNode(nn, p); }));
            }
            foreach (var n in node.ChildTypes)
            {
                var nn = n;
                var a = menu.Add(new MenuAction2<ConversationNode>(nn.Name, (nnn, p) => null, null, null, p => { }));
                AddNodeMenuItem(a, nn, control);
            }
        }
    }
}

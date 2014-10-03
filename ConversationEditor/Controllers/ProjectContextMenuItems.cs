using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI>;

namespace ConversationEditor.Controllers
{
    //This is a half complete class for a graph editor for projects
    public class ProjectContextMenuItems : IMenuActionFactory<ConversationNode>
    {
        private IProject m_project;
        public ProjectContextMenuItems(IProject project)
        {
            m_project = project;
        }

        public IEnumerable<MenuAction2<ConversationNode>> GetMenuActions(GraphEditorControl<ConversationNode> control)
        {
            yield return new MenuAction2<ConversationNode>("Reset Zoom", (n, p) => null, null, null, (p) => { control.GraphScale = 1; });
            yield return new MenuAction2<ConversationNode>("Delete", (n, p) => () => { Delete(n); }, null, null, null);
            yield return new MenuAction2<ConversationNode>("Remove Links", (n, p) => null, (i, p) => { control.CurrentFile.RemoveLinks(i); }, null, null);
        }

        private void Delete(ConversationNode node)
        {
        }
    }
}

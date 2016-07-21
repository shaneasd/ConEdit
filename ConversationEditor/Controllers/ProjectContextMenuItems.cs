using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;

namespace ConversationEditor
{
    //This is a half complete class for a graph editor for projects
    internal class ProjectContextMenuItems : IMenuActionFactory<ConversationNode>
    {
        public ProjectContextMenuItems()
        {
        }

        public IEnumerable<MenuAction<ConversationNode>> GetMenuActions(IGraphEditorControl<ConversationNode> control, IProject2 project, Action<IEnumerable<IErrorListElement>> log)
        {
            yield return new MenuAction<ConversationNode>("Reset Zoom", (n, p) => null, null, null, (p) => { control.GraphScale = 1; });
            //yield return new MenuAction2<ConversationNode>("Delete", (n, p) => () => { Delete(n); }, null, null, null);
            //yield return new MenuAction2<ConversationNode>("Remove Links", (n, p) => null, (i, p) => { control.CurrentFile.RemoveLinks(i); }, null, null);
        }

        //private void Delete(ConversationNode node)
        //{
        //}
    }
}

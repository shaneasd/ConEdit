using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    public interface IMenuActionFactory<TNode> where TNode : class, IRenderable<IGUI>, IConversationNode, IConfigurable
    {
        IEnumerable<MenuAction2<TNode>> GetMenuActions(ColorScheme scheme, GraphEditorControl<TNode> control);
    }
}

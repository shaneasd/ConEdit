using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.IO;
using ConversationEditor;

namespace ConversationEditor
{
    /// <summary>
    /// Defines context menu items for the graph editor
    /// Implementers of this task will be loaded from plugins by reflection provided they have a parameterless constructor
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public interface IMenuActionFactory<TNode> where TNode : class, IRenderable<IGui>, IConversationNode, IConfigurable
    {
        IEnumerable<MenuAction<TNode>> GetMenuActions(IGraphEditorControl<TNode> control);
    }
}

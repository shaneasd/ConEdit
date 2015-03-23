using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using ConversationEditor.Controllers;
using ConversationNode = Conversation.ConversationNode<Conversation.INodeGUI>;
using System.Drawing;
using Conversation;

namespace PluginPack
{
    public class MenuActionFactory : IMenuActionFactory<ConversationNode>
    {
        public IEnumerable<MenuAction2<ConversationNode>> GetMenuActions(ColorScheme scheme, GraphEditorControl<ConversationNode> control)
        {
            Action<ConversationNode, Point> jump = (n, p) =>
                {
                    if (n.Type == SpecialNodes.JUMP_TO_GUID)
                    {
                        var targetnodes = control.CurrentFile.Nodes.Where(a => a.Type == SpecialNodes.JUMP_TARGET_GUID);
                        var target = targetnodes.FirstOrDefault(t => JumpsErrorChecker<ConversationNode>.GetID(t) == JumpsErrorChecker<ConversationNode>.GetTarget(n));
                        if (target != null)
                            control.SelectNode(target);
                    }
                    else if (n.Type == SpecialNodes.JUMP_TARGET_GUID)
                    {
                        var sourceNodes = control.CurrentFile.Nodes.Where(a => a.Type == SpecialNodes.JUMP_TO_GUID);
                        var source = sourceNodes.FirstOrDefault(t => JumpsErrorChecker<ConversationNode>.GetTarget(t) == JumpsErrorChecker<ConversationNode>.GetID(n));
                        if (source != null)
                            control.SelectNode(source);
                    }
                };
            yield return new MenuAction2<ConversationNode>("Jump", (n, p) => (n.Type == SpecialNodes.JUMP_TO_GUID || n.Type == SpecialNodes.JUMP_TARGET_GUID ? () => jump(n, p) : (Action)null), null, null, null);
        }
    }
}

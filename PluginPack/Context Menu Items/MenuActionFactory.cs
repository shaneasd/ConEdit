using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
using System.Drawing;
using Conversation;

namespace PluginPack
{
    public class MenuActionFactory : IMenuActionFactory<ConversationNode>
    {
        public IEnumerable<MenuAction<ConversationNode>> GetMenuActions(IGraphEditorControl<ConversationNode> control, IProject2 project, Action<IEnumerable<IErrorListElement>> log, ILocalizationEngine localizer)
        {
            Action<ConversationNode, Point> jump = (n, p) =>
                {
                    if (n.Data.NodeTypeId == SpecialNodes.JumpTo)
                    {
                        var targetnodes = control.CurrentFile.Nodes.Where(a => a.Data.NodeTypeId == SpecialNodes.JumpTarget);
                        var target = targetnodes.FirstOrDefault(t => JumpsErrorChecker<ConversationNode>.GetId(t) == JumpsErrorChecker<ConversationNode>.GetTarget(n));
                        if (target != null)
                            control.SelectNode(target);
                    }
                    else if (n.Data.NodeTypeId == SpecialNodes.JumpTarget)
                    {
                        var sourceNodes = control.CurrentFile.Nodes.Where(a => a.Data.NodeTypeId == SpecialNodes.JumpTo);
                        var source = sourceNodes.FirstOrDefault(t => JumpsErrorChecker<ConversationNode>.GetTarget(t) == JumpsErrorChecker<ConversationNode>.GetId(n));
                        if (source != null)
                            control.SelectNode(source);
                    }
                };
            yield return new MenuAction<ConversationNode>("Jump", (n, p) => (n.Data.NodeTypeId == SpecialNodes.JumpTo || n.Data.NodeTypeId == SpecialNodes.JumpTarget ? () => jump(n, p) : (Action)null), null, null, null);
        }
    }
}

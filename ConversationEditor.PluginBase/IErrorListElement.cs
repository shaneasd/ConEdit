using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;
using Utilities;

namespace ConversationEditor
{
    public interface IErrorListElement
    {
        IEnumerable<ConversationNode> Nodes { get; }
        IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo> File { get; }
        string Message { get; }

        IEnumerator<Tuple<ConversationNode, IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>>> MakeEnumerator();
    }
}

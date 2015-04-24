using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGUI>;
using Conversation;

namespace ConversationEditor
{
    public interface IDomainFile : IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>, IInProject
    {
        DomainData Data { get; }
        event Action ConversationDomainModified;
    }
}

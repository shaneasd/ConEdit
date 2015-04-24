using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using System.IO;
using Conversation.Serialization;

namespace ConversationEditor
{
    using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGUI>;

    public interface IConversationFile : IConversationEditorControlData<ConversationNode, TransitionNoduleUIInfo>, IInProject
    {
    }
}

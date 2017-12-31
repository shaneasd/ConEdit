using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;

namespace ConversationEditor
{
    internal class DanglingAudioError : ConversationError<ConversationNode>
    {
        private readonly string m_file;
        public DanglingAudioError(string file, ConversationNode node)
            : base(node.Only())
        {
            m_file = file;
        }

        public override string Message => $"Audio parameter value '{m_file}' does not have a corresponding audio file loaded in the project";
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using ConversationNode = Conversation.ConversationNode<ConversationEditor.INodeGui>;

namespace ConversationEditor
{
    internal class PointlessAudioError : ConversationError<ConversationNode>
    {
        private string m_file;
        public PointlessAudioError(string file)
            : base(Enumerable.Empty<ConversationNode>())
        {
            m_file = file;
        }

        public override string Message => "Audio file '" + m_file + "' is not referenced by any conversation in this project";
    }
}

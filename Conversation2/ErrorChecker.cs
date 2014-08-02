﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public abstract class ErrorChecker<T>
        where T : IConversationNode
    {
        public abstract IEnumerable<ConversationError<T>> Check(IEnumerable<T> conversationFile);

        public abstract string GetName();
    }
}

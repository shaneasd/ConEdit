﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public interface IErrorCheckerUtilities<out T> where T: class, IConversationNode
    {
        Guid GetCategory(ID<NodeTypeTemp> type);
        T ReverseLookup(IEditable data);
    }

    public abstract class ErrorChecker<T>
        where T : class, IConversationNode
    {
        public abstract IEnumerable<ConversationError<T>> Check(IEnumerable<T> conversationFile, IErrorCheckerUtilities<T> utils);

        public abstract string GetName();
    }
}

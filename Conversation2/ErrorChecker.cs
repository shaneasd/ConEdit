using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public interface IErrorCheckerUtilities
    {
        Guid GetCategory(ID<NodeTypeTemp> type);
    }

    public abstract class ErrorChecker<T>
        where T : IConversationNode
    {
        public abstract IEnumerable<ConversationError<T>> Check(IEnumerable<T> conversationFile, IErrorCheckerUtilities utils);

        public abstract string GetName();
    }
}

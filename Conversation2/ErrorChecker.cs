using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public interface IErrorCheckerUtilities<out T> where T: class, IConversationNode
    {
        Guid GetCategory(Id<NodeTypeTemp> type);
        T ReverseLookup(IEditable data);
    }

    //TODO: This could probably be an interface but look out for reflection
    public abstract class ErrorChecker<T>
        where T : class, IConversationNode
    {
        public abstract IEnumerable<ConversationError<T>> Check(IEnumerable<T> nodes, IErrorCheckerUtilities<T> utils);

        public abstract string Name { get; }
    }
}

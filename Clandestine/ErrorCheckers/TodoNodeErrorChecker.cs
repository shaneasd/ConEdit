using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Conversation;

namespace Clandestine
{
    public class BadNodeErrorChecker<T> : ErrorChecker<T> where T : class,  IConversationNode
    {
        class TodoError : ConversationError<T>
        {
            public TodoError(T node)
                : base(node.Only())
            {
            }

            public override string Message
            {
                get { return "Conversation contains TODO node"; }
            }
        }

        public override IEnumerable<ConversationError<T>> Check(IEnumerable<T> nodes, IErrorCheckerUtilities<T> utils)
        {
            return nodes.Where(e => e.Type == SpecialNodes.ToDo).Select(node => new TodoError(node));
        }

        public override string Name
        {
            get { return "TODO nodes"; }
        }
    }
}

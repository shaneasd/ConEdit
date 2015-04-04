using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

namespace ConversationEditor.Error_Checkers
{
    public class InvalidConnectionErrorChecker<T> : ErrorChecker<T> where T : class, IConversationNode
    {
        class InvalidConnectionError : ConversationError<T>
        {
            public InvalidConnectionError(IEnumerable<T> nodes)
                : base(nodes)
            {
            }

            public override string Message
            {
                get { return "Rule breaking connection"; }
            }
        }

        public override IEnumerable<ConversationError<T>> Check(IEnumerable<T> conversationFile, IErrorCheckerUtilities<T> utils)
        {
            HashSet<UnorderedTuple2<T>> results = new HashSet<UnorderedTuple2<T>>();
            foreach (var node in conversationFile)
            {
                foreach (var c in node.Connectors)
                {
                    foreach (var connection in c.Connections)
                    {
                        if (!c.Rules.CanConnect(c.m_definition.Id, connection.m_definition.Id))
                            results.Add(UnorderedTuple.Make(node, utils.ReverseLookup(connection.Parent)));
                    }
                }
            }

            return results.Select(r => new InvalidConnectionError(r));
        }

        public override string GetName()
        {
            return "Invalid Connections";
        }
    }
}

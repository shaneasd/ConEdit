﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

namespace ConversationEditor.ErrorCheckers
{
    internal class InvalidConnectionErrorChecker<T> : ErrorChecker<T> where T : class, IConversationNode
    {
        class InvalidConnectionError : ConversationError<T>
        {
            public InvalidConnectionError(IEnumerable<T> nodes)
                : base(nodes)
            {
            }

            public override string Message => "Rule breaking connection";
        }

        public override IEnumerable<ConversationError<T>> Check(IEnumerable<T> nodes, IErrorCheckerUtilities<T> utils)
        {
            HashSet<UnorderedTuple2<T>> results = new HashSet<UnorderedTuple2<T>>();
            foreach (var node in nodes)
            {
                foreach (var c in node.Data.Connectors)
                {
                    foreach (var connection in c.Connections)
                    {
                        if (!c.Rules.CanConnect(c.Definition.Id, connection.Definition.Id))
                            results.Add(UnorderedTuple.Make(node, utils.ReverseLookup(connection.Parent)));
                    }
                }
            }

            return results.Select(r => new InvalidConnectionError(r));
        }

        public override string Name => "Invalid Connections";
    }
}

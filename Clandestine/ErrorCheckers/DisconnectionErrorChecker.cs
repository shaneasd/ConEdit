using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Conversation;
using ConversationEditor;

namespace Clandestine
{
    public class DisconnectionErrorChecker<T> : ErrorChecker<T> where T : class, IConversationNode
    {
        public class DisconnectedInputError : ConversationError<T>
        {
            public DisconnectedInputError(T node) : base(node.Only()) { }
            public override string Message
            {
                get { return "Disconnected input"; }
            }
        }

        public class DisconnectedOutputError : ConversationError<T>
        {
            public DisconnectedOutputError(T node) : base(node.Only()) { }
            public override string Message
            {
                get { return "Disconnected output"; }
            }
        }

        public override IEnumerable<ConversationError<T>> Check(IEnumerable<T> nodes, IErrorCheckerUtilities<T> utils)
        {
            foreach (var node in nodes)
            {
                var inputs = node.Connectors.Where(c => c.m_definition.Id == SpecialConnectors.Input.Id);
                var outputs = node.Connectors.Where(c => c.m_definition.Id == SpecialConnectors.Output.Id);

                foreach (var input in inputs)
                    if (!input.Connections.Any())
                        yield return new DisconnectedInputError(node);
                if (node.Type == SpecialNodes.BRANCH)
                {
                    if (!outputs.Any(c => c.Connections.Any()))
                        yield return new DisconnectedOutputError(node);
                }
                else
                {
                    foreach (var output in outputs)
                        if (!output.Connections.Any())
                            yield return new DisconnectedOutputError(node);
                }
            }
        }

        public override string GetName()
        {
            return "Disconnected link";
        }
    }
}

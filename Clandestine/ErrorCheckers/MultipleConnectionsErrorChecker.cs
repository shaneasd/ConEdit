using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Conversation;
using ConversationEditor;

namespace Clandestine
{
    /// <summary>
    /// This error checker asserts that two nodes cannot be connected to the same 'Output' unless
    /// - They are both 'Option' nodes OR
    /// - The parent 'Output' is on a 'Random' node or a node in the 'AI Barks' category
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MultipleConnectionsErrorChecker<T> : ErrorChecker<T>
        where T : IConversationNode
    {
        public class NonOptionSiblingsError : ConversationError<T>
        {
            public NonOptionSiblingsError(T node)
                : base(node.Only())
            {
            }
            public override string Message
            {
                get { return "Node has an output with multiple children including a non-option node"; }
            }
        }

        public override IEnumerable<ConversationError<T>> Check(IEnumerable<T> nodes, IErrorCheckerUtilities utils)
        {
            var filteredNodes = nodes.Where(n => !Clandestine.Util.IsAIBark(n.Type, utils) && n.Type != SpecialNodes.RANDOM_GUID);
            foreach (var n in filteredNodes)
            {
                var outputs = n.Connectors.Where(c => c.m_definition.Id == SpecialConnectors.Output.Id);
                foreach (var transitionOut in outputs)
                {
                    var connectedNodes = transitionOut.Connections.Select(c => c.Parent).Evaluate();
                    bool hasOption = connectedNodes.Any(a => a.NodeTypeID == SpecialNodes.OPTION_GUID);
                    int countNonOption = connectedNodes.Count(a => a.NodeTypeID != SpecialNodes.OPTION_GUID);
                    if (countNonOption + (hasOption ? 1 : 0) > 1)
                    {
                        yield return new NonOptionSiblingsError(n);
                    }
                }
            }
        }

        public override string GetName()
        {
            return "Multiple connection";
        }
    }
}

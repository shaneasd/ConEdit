using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Conversation;

namespace PluginPack
{
    public class OptionErrorChecker<T> : ErrorChecker<T>
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

        public override IEnumerable<ConversationError<T>> Check(IEnumerable<T> nodes)
        {
            foreach (var n in nodes)
            {
                foreach (var transitionOut in n.Connectors)
                {
                    var connectedNodes = transitionOut.Connections.Select(c => c.Parent).Evaluate();
                    bool hasOption = connectedNodes.Any(a => a.NodeTypeID == SpecialNodes.OPTION_GUID);
                    int countNonOption = connectedNodes.Count(a => a.NodeTypeID != SpecialNodes.OPTION_GUID);
                    if (countNonOption + (hasOption ? 1 : 0) > 1)
                        yield return new NonOptionSiblingsError(n);
                }
            }
        }

        public override string GetName()
        {
            return "Non-option siblings";
        }
    }
}

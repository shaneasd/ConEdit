using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

namespace Clandestine
{
    public class StartNodeErrorChecker<T> : ErrorChecker<T>
            where T : class, IConversationNode
    {
        class StartNodeCountError : ConversationError<T>
        {
            public StartNodeCountError(IEnumerable<T> nodes)
                : base(nodes)
            {
            }

            public override string Message => Nodes.Any() ? "More than one start node" : "No start node";
        }

        public override IEnumerable<ConversationError<T>> Check(IEnumerable<T> nodes, IErrorCheckerUtilities<T> utils)
        {
            var startNodes = nodes.Where(n => Clandestine.Util.IsStartNode(n.Data.NodeTypeId, utils)).ToList();
            if (startNodes.Count != 1)
                yield return new StartNodeCountError(startNodes);
        }

        public override string Name => "Incorrect number of start nodes";
    }
}

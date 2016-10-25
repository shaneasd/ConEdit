using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Conversation;

namespace Clandestine
{
    public class TerminatorErrorChecker<T> : ErrorChecker<T>
        where T : class, IConversationNode
    {
        class TerminatorError : ConversationError<T>
        {
            public TerminatorError(T node)
                : base(node.Only())
            {
            }

            public override string Message
            {
                get { return "Conversation node not connected via any path to a terminator"; }
            }
        }

        class StartError : ConversationError<T>
        {
            public StartError(T node)
                : base(node.Only())
            {
            }

            public override string Message
            {
                get { return "Conversation node not connected via any path to a valid start"; }
            }
        }

        private IEnumerable<ConversationError<T>> CheckStart(IEnumerable<T> nodes, IErrorCheckerUtilities<T> utils)
        {
            HashSet<T> connected = new HashSet<T>(nodes.Where(n => Clandestine.Util.IsStartNode(n.Data.NodeTypeId, utils)));

            var editableMapping = nodes.ToDictionary(n => n.Data.NodeId, n => n);

            List<T> toProcess = connected.ToList();
            for (int i = 0; i < toProcess.Count; i++)
            {
                var node = toProcess[i];
                var connections = node.Data.Connectors.SelectMany(c => c.Connections).Select(c => c.Parent);
                foreach (var connection in connections)
                {
                    var x = editableMapping[connection.NodeId];
                    if (!connected.Contains(x))
                    {
                        toProcess.Add(x);
                        connected.Add(x);
                    }
                }
            }

            foreach (var node in nodes)
            {
                if (!connected.Contains(node))
                    if (node.Data.Connectors.Any())
                        yield return new StartError(node);
            }
        }

        private IEnumerable<ConversationError<T>> CheckEnd(IEnumerable<T> nodes, IErrorCheckerUtilities<T> utils)
        {
            HashSet<T> connected = new HashSet<T>(nodes.Where(n => n.Data.NodeTypeId == SpecialNodes.Terminator));

            var editableMapping = nodes.ToDictionary(n => n.Data.NodeId, n => n);

            List<T> toProcess = connected.ToList();
            for (int i = 0; i < toProcess.Count; i++)
            {
                var node = toProcess[i];
                var connections = node.Data.Connectors.SelectMany(c => c.Connections).Select(c => c.Parent);
                foreach (var connection in connections)
                {
                    var x = editableMapping[connection.NodeId];
                    if (!connected.Contains(x))
                    {
                        toProcess.Add(x);
                        connected.Add(x);
                    }
                }
            }

            foreach (var node in nodes)
            {
                if (!connected.Contains(node))
                    if (node.Data.Connectors.Any())
                        yield return new TerminatorError(node);
            }
        }

        public override IEnumerable<ConversationError<T>> Check(IEnumerable<T> nodes, IErrorCheckerUtilities<T> utils)
        {
            var start = CheckStart(nodes, utils);
            var end = CheckEnd(nodes, utils);
            return start.Concat(end);
        }

        public override string Name
        {
            get { return "Node not in valid path"; }
        }
    }
}

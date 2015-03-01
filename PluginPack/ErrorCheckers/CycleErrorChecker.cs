using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Conversation;

namespace PluginPack
{
    public class CycleErrorChecker<T> : ErrorChecker<T>
        where T : IConversationNode
    {
        class CycleError : ConversationError<T>
        {
            public CycleError(T node)
                : base(node.Only())
            {
            }

            public override string Message
            {
                get { return "Conversation node not connected via any path to a terminator"; }
            }
        }

        public override IEnumerable<ConversationError<T>> Check(IEnumerable<T> nodes, IErrorCheckerUtilities utils)
        {
            return Enumerable.Empty<ConversationError<T>>(); //TODO: Reimplement this
            //var connectedNodes = new LinkedList<T>();
            //var disconnectedNodes = new LinkedList<T>(nodes);

            //for (var node = disconnectedNodes.First; node != null; )
            //{
            //    if (node.Value.Type == SpecialNodes.TERMINATOR_GUID)
            //    {
            //        connectedNodes.AddFirst(node.Value);
            //        var nextNode = node.Next;
            //        disconnectedNodes.Remove(node);
            //        node = nextNode;
            //    }
            //    else if (!node.Value.TransitionsIn.Any() && !node.Value.TransitionsOut.Any()) //This node can't be connected
            //    {
            //        var nextNode = node.Next;
            //        disconnectedNodes.Remove(node);
            //        node = nextNode;
            //    }
            //    else
            //    {
            //        node = node.Next;
            //    }
            //}

            //bool removedOne = true;
            //while (removedOne)
            //{
            //    removedOne = false;
            //    for (var node = disconnectedNodes.First; node != null; node = node.Next)
            //    {
            //        if (node.Value.Outputs.SelectMany(t => t.Connections).Any(n => connectedNodes.Contains(n.Parent)))
            //        {
            //            connectedNodes.AddFirst(node.Value);
            //            disconnectedNodes.Remove(node);
            //            removedOne = true;
            //        }
            //        if (node.Value.Type == SpecialNodes.JUMP_TO_GUID)
            //        {
            //            if (connectedNodes.Where(n => n.Type == SpecialNodes.JUMP_TARGET_GUID).Any(n => JumpsErrorChecker<T, U>.GetID(n) == JumpsErrorChecker<T, U>.GetTarget(node.Value)))
            //            {
            //                connectedNodes.AddFirst(node.Value);
            //                disconnectedNodes.Remove(node);
            //                removedOne = true;
            //            }
            //        }
            //    }
            //}

            //return disconnectedNodes.Select(n => new CycleError(n));
        }

        public override string GetName()
        {
            return "Cycle error checker";
        }
    }
}

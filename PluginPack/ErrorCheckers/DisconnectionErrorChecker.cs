using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Conversation;

namespace PluginPack
{
    //public class DisconnectionErrorChecker<T> : ErrorChecker<T> where T : IConversationNode
    //{
    //    public class DisconnectedLinkError : ConversationError<T>
    //    {
    //        public DisconnectedLinkError(T node) : base(node.Only()) { }
    //        public override string Message
    //        {
    //            get { return "Disconnected link"; }
    //        }
    //    }
    //    public override IEnumerable<ConversationError<T>> Check(IEnumerable<T> nodes, IErrorCheckerUtilities utils)
    //    {
    //        foreach (var node in nodes)
    //        {
    //            if (node.Connectors.Any(t => !t.Connections.Any()))
    //                yield return new DisconnectedLinkError(node);
    //        }
    //    }

    //    public override string GetName()
    //    {
    //        return "Disconnected link";
    //    }
    //}
}

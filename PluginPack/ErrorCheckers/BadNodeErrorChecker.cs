﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Conversation;

namespace PluginPack
{
    public class BadNodeErrorChecker<T> : ErrorChecker<T> where T : IConversationNode
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

        public override IEnumerable<ConversationError<T>> Check(IEnumerable<T> nodes)
        {
            return nodes.Where(e => e.Type == SpecialNodes.TODO_GUID).Select(node => new TodoError(node));
        }

        public override string GetName()
        {
            return "TODO nodes";
        }
    }
}

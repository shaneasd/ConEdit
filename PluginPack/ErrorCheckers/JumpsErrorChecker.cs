using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Conversation;

namespace PluginPack
{
    public class JumpsErrorChecker<T> : ErrorChecker<T> where T : class, IConversationNode
    {
        public static string GetID(T node)
        {
            return (node.Parameters.Single(n => n.Name == "ID") as IDynamicEnumParameter).Value;
        }

        public static string GetTarget(T node)
        {
            return (node.Parameters.Single(n => n.Name == "Target") as IDynamicEnumParameter).Value;
        }

        class DuplicatedTargetError : ConversationError<T>
        {
            private string m_target;
            public DuplicatedTargetError(IEnumerable<T> nodes, string target)
                : base(nodes)
            {
                m_target = target;
            }

            public override string Message
            {
                get { return "Multiple jump targets named \"" + m_target + "\""; }
            }
        }

        class PointlessTargetError : ConversationError<T>
        {
            private string m_id;
            public PointlessTargetError(T node) : base(node.Only()) { m_id = GetID(node); }
            public override string Message
            {
                get { return "Jump target \"" + m_id + "\" has nothing jumping to it"; }
            }
        }

        class DeadEndJumpError : ConversationError<T>
        {
            private string m_target;
            public DeadEndJumpError(T node) : base(node.Only()) { m_target = GetTarget(node); }
            public override string Message
            {
                get { return "Jump to unknown target: \"" + m_target + "\""; }
            }
        }

        public override IEnumerable<ConversationError<T>> Check(IEnumerable<T> nodes, IErrorCheckerUtilities<T> utils)
        {
            var targetnodes = nodes.Where(a => a.Type == SpecialNodes.JUMP_TARGET_GUID);
            var jumpnodes = nodes.Where(a => a.Type == SpecialNodes.JUMP_TO_GUID);

            //Find all the jump targets with duplicate ids
            foreach (var d in targetnodes.GroupBy(GetID, a => a).Where(g => g.Count() > 1))
                yield return new DuplicatedTargetError(d, d.Key);

            //Find all the jump targets with nothing jumping to them
            foreach (var node in targetnodes.Where(n => !jumpnodes.Any(nn => GetTarget(nn) == GetID(n))))
                yield return new PointlessTargetError(node);

            //Find all the jumps to missing targets
            foreach (var node in jumpnodes.Where(n => !targetnodes.Any(nn => GetID(nn) == GetTarget(n))))
                yield return new DeadEndJumpError(node);
        }

        public override string GetName()
        {
            return "Invalid jumps";
        }
    }

}

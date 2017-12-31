using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using Conversation;

namespace Clandestine
{
    public class JumpsErrorChecker<T> : ErrorChecker<T> where T : class, IConversationNode
    {
        public static string GetId(T node)
        {
            return (node.Data.Parameters.Single(n => string.Equals(n.Name, "ID", StringComparison.OrdinalIgnoreCase)) as IDynamicEnumParameter).Value;
        }

        public static string GetTarget(T node)
        {
            return (node.Data.Parameters.Single(n => string.Equals(n.Name, "Target", StringComparison.OrdinalIgnoreCase)) as IDynamicEnumParameter).Value;
        }

        class DuplicatedTargetError : ConversationError<T>
        {
            private string m_target;
            public DuplicatedTargetError(IEnumerable<T> nodes, string target)
                : base(nodes)
            {
                m_target = target;
            }

            public override string Message => $"Multiple jump targets named \"{m_target}\"";
        }

        class PointlessTargetError : ConversationError<T>
        {
            private string m_id;
            public PointlessTargetError(T node) : base(node.Only()) { m_id = GetId(node); }
            public override string Message => $"Jump target \"{m_id}\" has nothing jumping to it";
        }

        class DeadEndJumpError : ConversationError<T>
        {
            private string m_target;
            public DeadEndJumpError(T node) : base(node.Only()) { m_target = GetTarget(node); }
            public override string Message => $"Jump to unknown target: \"{m_target}\"";
        }

        public override IEnumerable<ConversationError<T>> Check(IEnumerable<T> nodes, IErrorCheckerUtilities<T> utils)
        {
            var targetnodes = nodes.Where(a => a.Data.NodeTypeId == SpecialNodes.JumpTarget);
            var jumpnodes = nodes.Where(a => a.Data.NodeTypeId == SpecialNodes.JumpTo);

            //Find all the jump targets with duplicate ids
            foreach (var d in targetnodes.GroupBy(GetId, a => a).Where(g => g.Count() > 1))
                yield return new DuplicatedTargetError(d, d.Key);

            //Find all the jump targets with nothing jumping to them
            foreach (var node in targetnodes.Where(n => !jumpnodes.Any(nn => GetTarget(nn) == GetId(n))))
                yield return new PointlessTargetError(node);

            //Find all the jumps to missing targets
            foreach (var node in jumpnodes.Where(n => !targetnodes.Any(nn => GetId(nn) == GetTarget(n))))
                yield return new DeadEndJumpError(node);
        }

        public override string Name => "Invalid jumps";
    }

}

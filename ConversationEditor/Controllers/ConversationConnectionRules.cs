using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

namespace ConversationEditor
{
    public class ConversationConnectionRules : IConnectionRules
    {
        public static readonly ConversationConnectionRules Instance = new ConversationConnectionRules();

        HashSet<UnorderedTuple2<ID<TConnectorDefinition>>> m_allowed = new HashSet<UnorderedTuple2<ID<TConnectorDefinition>>>();

        public bool CanConnect(ID<TConnectorDefinition> a, ID<TConnectorDefinition> b)
        {
            var test = UnorderedTuple.Make(a, b);
            var alwaysAllowed = UnorderedTuple.Make(SpecialConnectors.Input.Id, SpecialConnectors.Output.Id);
            if (test.Equals(alwaysAllowed))
                return true;
            if (m_allowed.Contains(test))
                return true;
            return false;
        }

        internal void SetRules(IEnumerable<ConnectionDefinitionData> allowed)
        {
            m_allowed.Clear();
            m_allowed.UnionWith(allowed.Select(c => c.Connectors));
        }
    }
}

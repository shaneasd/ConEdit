﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

namespace ConversationEditor
{
    internal class DomainConnectionRules : IConnectionRules
    {
        static readonly HashSet<UnorderedTuple2<ID<TConnectorDefinition>>> acceptablePairings = new HashSet<UnorderedTuple2<ID<TConnectorDefinition>>>()
        {
            UnorderedTuple.Make(DomainIDs.EnumOutputDefinition.Id, DomainIDs.EnumValueOutputDefinition.Id), //Connect enumeration declarations with possible values
            
            UnorderedTuple.Make(DomainIDs.ConfigOutputDefinition.Id, DomainIDs.NodeOutputConfigDefinition.Id), //Connect node declaration with config
            UnorderedTuple.Make(DomainIDs.ConfigOutputDefinition.Id, DomainIDs.ParameterConfigConnectorDefinition.Id), //Connect parameter declarations with config

            UnorderedTuple.Make(DomainIDs.ParameterOutputDefinition.Id, DomainIDs.NodeOutputParametersDefinition.Id), //Connect node declaration with parameters for that node type
            UnorderedTuple.Make(DomainIDs.ConnectorOutputDefinition.Id, DomainIDs.NodeOutputConnectorsDefinition.Id), //Connect node declaration with connectors for that node type

            UnorderedTuple.Make(DomainIDs.ConnectorDefiinitionOutputDefinition.Id, DomainIDs.ParameterOutputDefinition.Id), //Connect connector declaration with parameters for that connector type
            //UnorderedTuple.Make(DomainIDs.CONNECTOR_DEFINITION_CONNECTION_DEFINITION.Id, DomainIDs.CONNECTION_DEFINITION_CONNECTOR.Id), //Connect connector declaration with connection declaration
        };

        public static readonly DomainConnectionRules Instance = new DomainConnectionRules();

        public bool CanConnect(ID<TConnectorDefinition> a, ID<TConnectorDefinition> b)
        {
            return acceptablePairings.Contains(UnorderedTuple.Make(a, b));
        }
    }
}

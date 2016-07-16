using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public class DomainData
    {
        public List<NodeTypeData> NodeTypes { get; } = new List<NodeTypeData>();
        public List<DynamicEnumerationData> DynamicEnumerations { get; } = new List<DynamicEnumerationData>();
        public List<LocalDynamicEnumerationData> LocalDynamicEnumerations { get; } = new List<LocalDynamicEnumerationData>();
        public List<EnumerationData> Enumerations { get; } = new List<EnumerationData>();
        public List<DecimalData> Decimals { get; } = new List<DecimalData>();
        public List<IntegerData> Integers { get; } = new List<IntegerData>();
        public List<NodeData> Nodes { get; } = new List<NodeData>();
        public List<ConnectorDefinitionData> Connectors { get; } = new List<ConnectorDefinitionData>();
        public List<ConnectionDefinitionData> Connections { get; } = new List<ConnectionDefinitionData>();
    }
}

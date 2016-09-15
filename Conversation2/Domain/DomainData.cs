using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public interface IDomainData
    {
        IReadOnlyList<NodeTypeData> NodeTypes { get; }
        IReadOnlyList<DynamicEnumerationData> DynamicEnumerations { get; }
        IReadOnlyList<LocalDynamicEnumerationData> LocalDynamicEnumerations { get; }
        IReadOnlyList<EnumerationData> Enumerations { get; }
        IReadOnlyList<DecimalData> Decimals { get; }
        IReadOnlyList<IntegerData> Integers { get; }
        IReadOnlyList<NodeData> Nodes { get; }
        IReadOnlyList<ConnectorDefinitionData> Connectors { get; }
        IReadOnlyList<ConnectionDefinitionData> Connections { get; }
    }

    public class DomainData : IDomainData
    {
        IReadOnlyList<NodeTypeData> IDomainData.NodeTypes { get { return m_nodeTypes; } }
        IReadOnlyList<DynamicEnumerationData> IDomainData.DynamicEnumerations { get { return m_dynamicEnumerations; } }
        IReadOnlyList<LocalDynamicEnumerationData> IDomainData.LocalDynamicEnumerations { get { return m_localDynamicEnumerations; } }
        IReadOnlyList<EnumerationData> IDomainData.Enumerations { get { return m_enumerations; } }
        IReadOnlyList<DecimalData> IDomainData.Decimals { get { return m_decimals; } }
        IReadOnlyList<IntegerData> IDomainData.Integers { get { return m_integers; } }
        IReadOnlyList<NodeData> IDomainData.Nodes { get { return m_nodes; } }
        IReadOnlyList<ConnectorDefinitionData> IDomainData.Connectors { get { return m_connectors; } }
        IReadOnlyList<ConnectionDefinitionData> IDomainData.Connections { get { return m_connections; } }

        private readonly List<NodeTypeData> m_nodeTypes = new List<NodeTypeData>();
        private readonly List<DynamicEnumerationData> m_dynamicEnumerations = new List<DynamicEnumerationData>();
        private readonly List<LocalDynamicEnumerationData> m_localDynamicEnumerations = new List<LocalDynamicEnumerationData>();
        private readonly List<EnumerationData> m_enumerations = new List<EnumerationData>();
        private readonly List<DecimalData> m_decimals = new List<DecimalData>();
        private readonly List<IntegerData> m_integers = new List<IntegerData>();
        private readonly List<NodeData> m_nodes = new List<NodeData>();
        private readonly List<ConnectorDefinitionData> m_connectors = new List<ConnectorDefinitionData>();
        private readonly List<ConnectionDefinitionData> m_connections = new List<ConnectionDefinitionData>();

        public ICollection<NodeTypeData> NodeTypes { get { return m_nodeTypes; } }
        public ICollection<DynamicEnumerationData> DynamicEnumerations { get { return m_dynamicEnumerations; } }
        public ICollection<LocalDynamicEnumerationData> LocalDynamicEnumerations { get { return m_localDynamicEnumerations; } }
        public ICollection<EnumerationData> Enumerations { get { return m_enumerations; } }
        public ICollection<DecimalData> Decimals { get { return m_decimals; } }
        public ICollection<IntegerData> Integers { get { return m_integers; } }
        public ICollection<NodeData> Nodes { get { return m_nodes; } }
        public ICollection<ConnectorDefinitionData> Connectors { get { return m_connectors; } }
        public ICollection<ConnectionDefinitionData> Connections { get { return m_connections; } }
    }
}

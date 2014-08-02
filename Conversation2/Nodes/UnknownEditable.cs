using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    public class UnknownEditable : IEditable
    {
        private ID<NodeTemp> m_nodeID;
        private ID<NodeTypeTemp> m_nodeTypeID;
        private List<UnknownParameter> m_parameters;
        public event Action Linked { add { } remove { } }

        public UnknownEditable(ID<NodeTemp> nodeID, ID<NodeTypeTemp> nodeTypeID, IEnumerable<UnknownParameter> parameters)
        {
            m_nodeID = nodeID;
            m_nodeTypeID = nodeTypeID;
            m_parameters = parameters.ToList();
        }

        public ID<NodeTemp> NodeID
        {
            get { return m_nodeID; }
        }

        public ID<NodeTypeTemp> NodeTypeID
        {
            get { return m_nodeTypeID; }
        }

        public string Name
        {
            get { return "Unknown Node"; }
        }

        public List<NodeData.ConfigData> Config
        {
            get { return new List<NodeData.ConfigData>(); }
        }

        public IEnumerable<Parameter> Parameters
        {
            get { return m_parameters; }
        }

        List<Output> m_connectors = new List<Output>();
        public IEnumerable<Output> Connectors
        {
            get { return m_connectors; }
        }

        public void ChangeId(ID<NodeTemp> id)
        {
            m_nodeID = id;
        }

        public void TryDecorrupt()
        {
        }

        class CustomConnectionRules : IConnectionRules
        {
            HashSet<UnordererTuple2<ID<TConnectorDefinition>>> m_rules = new HashSet<UnordererTuple2<ID<TConnectorDefinition>>>();

            public bool CanConnect(ID<TConnectorDefinition> a, ID<TConnectorDefinition> b)
            {
                return m_rules.Contains(UnordererTuple.Make(a, b));
            }

            public void Allow(ID<TConnectorDefinition> a, ID<TConnectorDefinition> b)
            {
                m_rules.Add(UnordererTuple.Make(a, b));
            }
        }

        CustomConnectionRules m_rules = new CustomConnectionRules();

        public void AddConnector(ID<TConnector> id)
        {
            if (!m_connectors.Any(c => c.ID == id))
            {
                ConnectorDefinitionData data = new ConnectorDefinitionData("", ID<TConnectorDefinition>.ConvertFrom(id), new List<NodeData.ParameterData>(), ConnectorPosition.Bottom);
                m_connectors.Add(new Output(id, data, this, new List<Parameter>(), m_rules));
            }
        }

        public void AllowConnection(Output connector1, Output connector2)
        {
            m_rules.Allow(connector1.m_definition.Id, connector2.m_definition.Id);
        }


        public SimpleUndoPair RemoveUnknownParameter(UnknownParameter p)
        {
            throw new NotImplementedException(); //TODO: Should we allow this?
        }
    }
}

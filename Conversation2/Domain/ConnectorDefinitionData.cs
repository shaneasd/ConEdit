using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public struct ConnectorDefinitionData
    {
        public ConnectorDefinitionData(string name, Id<TConnectorDefinition> id, List<NodeData.ParameterData> parameters, ConnectorPosition position, bool hidden)
        {
            m_name = name;
            m_id = id;
            m_parameters = parameters.ToList().AsReadOnly();
            m_position = position;
            m_hidden = hidden;
        }

        public ConnectorDefinitionData(string name, Id<TConnectorDefinition> id, List<NodeData.ParameterData> parameters, ConnectorPosition position)
            : this(name, id, parameters, position, false)
        {
        }

        private readonly string m_name; //Can be null
        private readonly Id<TConnectorDefinition> m_id;
        private readonly IEnumerable<NodeData.ParameterData> m_parameters;
        private readonly ConnectorPosition m_position;
        private readonly bool m_hidden;

        public string Name { get { return m_name; } } //Can be null
        public Id<TConnectorDefinition> Id { get { return m_id; } }
        public IEnumerable<NodeData.ParameterData> Parameters { get { return m_parameters; } }
        public ConnectorPosition Position { get { return m_position; } }
        public bool Hidden { get { return m_hidden; } }

        public static Id<TConnectorDefinition> InputDefinitionId { get; } = Id<TConnectorDefinition>.Parse("73e5cff2-7d6c-45e8-8f0e-08bcb780acc9");
        public static Id<TConnectorDefinition> OutputDefinitionId { get; } = Id<TConnectorDefinition>.Parse("a800357f-5013-44c1-8637-c8a60cff240b");
        public static Id<Parameter> OutputName { get; } = Id<Parameter>.Parse("ec0c0b5c-57d9-484b-8946-c8dcf3e09b38");

        //Domain currently doesn't have any connectors with parameters so ignore them for now
        public Output MakeWithoutParameters(Id<TConnector> id, IEditable parent, IConnectionRules rules)
        {
            if (this.Parameters.Any())
                throw new Exception("Something wrong with domain domain");
            return new Output(id, this, parent, new List<Parameter>(), rules);
        }

        public Func<IEditable, List<Parameter>, Output> Make(Id<TConnector> id, IConnectionRules rules)
        {
            var thisCopy = this; //Make a copy because the lamba can't capture this because this is a struct
            return (parent, parameters) => new Output(id, thisCopy, parent, parameters, rules);
        }
    }
}

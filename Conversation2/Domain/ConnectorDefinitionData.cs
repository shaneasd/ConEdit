using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public struct ConnectorDefinitionData
    {
        public ConnectorDefinitionData(string name, Id<TConnectorDefinition> id, IReadOnlyCollection<NodeData.ParameterData> parameters, ConnectorPosition position, bool hidden)
        {
            Name = name;
            Id = id;
            Parameters = parameters;
            Position = position;
            Hidden = hidden;
        }

        public ConnectorDefinitionData(string name, Id<TConnectorDefinition> id, IReadOnlyCollection<NodeData.ParameterData> parameters, ConnectorPosition position)
            : this(name, id, parameters, position, false)
        {
        }

        public string Name { get; } //Can be null
        public Id<TConnectorDefinition> Id { get; }
        public IEnumerable<NodeData.ParameterData> Parameters { get; }
        public ConnectorPosition Position { get; }
        public bool Hidden { get; }

        public static Id<TConnectorDefinition> InputDefinitionId { get; } = Id<TConnectorDefinition>.Parse("73e5cff2-7d6c-45e8-8f0e-08bcb780acc9");
        public static Id<TConnectorDefinition> OutputDefinitionId { get; } = Id<TConnectorDefinition>.Parse("a800357f-5013-44c1-8637-c8a60cff240b");
        public static Id<Parameter> OutputName { get; } = Id<Parameter>.Parse("ec0c0b5c-57d9-484b-8946-c8dcf3e09b38");

        public Func<IEditable, IReadOnlyList<IParameter>, Output> Make(Id<TConnector> id, IConnectionRules rules)
        {
            var thisCopy = this; //Make a copy because the lamba can't capture this because this is a struct
            return (parent, parameters) => new Output(id, thisCopy, parent, parameters, rules);
        }
    }
}

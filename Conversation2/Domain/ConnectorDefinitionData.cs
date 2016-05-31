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
            Name = name;
            Id = id;
            Parameters = parameters.ToList().AsReadOnly();
            Position = position;
            Hidden = hidden;
        }

        public ConnectorDefinitionData(string name, Id<TConnectorDefinition> id, List<NodeData.ParameterData> parameters, ConnectorPosition position)
            : this(name, id, parameters, position, false)
        {
        }

        public readonly string Name; //Can be null
        public readonly Id<TConnectorDefinition> Id;
        public readonly IEnumerable<NodeData.ParameterData> Parameters;
        public readonly ConnectorPosition Position;
        public readonly bool Hidden;

        public static readonly Id<TConnectorDefinition> InputDefinitionId = Id<TConnectorDefinition>.Parse("73e5cff2-7d6c-45e8-8f0e-08bcb780acc9");
        public static readonly Id<TConnectorDefinition> OutputDefinitionId = Id<TConnectorDefinition>.Parse("a800357f-5013-44c1-8637-c8a60cff240b");
        public static readonly Id<Parameter> OutputName = Id<Parameter>.Parse("ec0c0b5c-57d9-484b-8946-c8dcf3e09b38");

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

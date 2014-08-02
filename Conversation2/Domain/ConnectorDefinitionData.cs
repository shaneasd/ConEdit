using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public struct ConnectorDefinitionData
    {
        public ConnectorDefinitionData(string name, ID<TConnectorDefinition> id, List<NodeData.ParameterData> parameters, ConnectorPosition position, bool hidden = false)
        {
            Name = name;
            Id = id;
            Parameters = parameters.ToList().AsReadOnly();
            Position = position;
            Hidden = hidden;
        }
        public readonly string Name; //Can be null
        public readonly ID<TConnectorDefinition> Id;
        public readonly IEnumerable<NodeData.ParameterData> Parameters;
        public readonly ConnectorPosition Position;
        public readonly bool Hidden;

        public static readonly ID<TConnectorDefinition> INPUT_DEFINITION_ID = ID<TConnectorDefinition>.Parse("73e5cff2-7d6c-45e8-8f0e-08bcb780acc9");
        public static readonly ID<TConnectorDefinition> OUTPUT_DEFINITION_ID = ID<TConnectorDefinition>.Parse("a800357f-5013-44c1-8637-c8a60cff240b");
        public static readonly ID<Parameter> OUTPUT_NAME = ID<Parameter>.Parse("ec0c0b5c-57d9-484b-8946-c8dcf3e09b38");

        //Domain currently doesn't have any connectors with parameters so ignore them for now
        public Output MakeWithoutParameters(ID<TConnector> id, IEditable parent, IConnectionRules rules)
        {
            if (this.Parameters.Any())
                throw new Exception("Something wrong with domain domain");
            return new Output(id, this, parent, new List<Parameter>(), rules);
        }

        public Func<IEditable, List<Parameter>, Output> Make(ID<TConnector> id, IConnectionRules rules)
        {
            var thisCopy = this; //Make a copy because the lamba can't capture this because this is a struct
            return (parent, parameters) => new Output(id, thisCopy, parent, parameters, rules);
        }
    }
}

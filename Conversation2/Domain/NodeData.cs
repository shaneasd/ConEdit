using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    /// <summary>
    /// Data structure representing all data required to define a specific node type
    /// Essentially all data in a node definition node, and all attached nodes, in a domain graph.
    /// </summary>
    public struct NodeData
    {
        /// <summary>
        /// Defines a connector which forms part of a node definition
        /// The node can have multiple connectors and multiple connectors of the same type
        /// </summary>
        public struct ConnectorData
        {
            public ConnectorData(ID<TConnector> id, ID<TConnectorDefinition> typeID, List<Parameter> parameters)
            {
                Id = id;
                TypeID = typeID;
                Parameters = parameters;
            }

            /// <summary>
            /// Identifies this connector for the node type uniquely. A combination of this ID and a node ID will uniquely identify a connector in a graph.
            /// </summary>
            public readonly ID<TConnector> Id;

            /// <summary>
            /// Identifies the connector definition which classifies this connector
            /// </summary>
            public readonly ID<TConnectorDefinition> TypeID;

            public readonly List<Parameter> Parameters;

            //public Func<IEditable, Output> Make(Func<ID<OutputTemp>, OutputDefinition> definitionFactory)
            //{
            //    ConnectorData thisCopy = this;
            //    return e => definitionFactory(thisCopy.TypeID).Generate(e, thisCopy.Id);
            //}
        }

        public struct ParameterData
        {
            public ParameterData(string name, ID<Parameter> id, ID<ParameterType> type)
            {
                Type = type;
                Name = name;
                Id = id;
                Default = null;
            }

            public ParameterData(string name, ID<Parameter> id, ID<ParameterType> type, string def)
            {
                Type = type;
                Name = name;
                Id = id;
                Default = def;
            }

            public ID<ParameterType> Type;
            public string Name;
            public ID<Parameter> Id;
            public string Default; //Can be null, string form of the default value

            public Parameter Make(Func<ID<ParameterType>, string, ID<Parameter>, string, Parameter> parameterFactory)
            {
                var result = parameterFactory(Type, Name, Id, Default);
                //var @default = ;
                //if (@default != null)
                //{
                //    bool success = result.TryDeserialiseValue(@default);
                //    if (!success)
                //        throw new Exception("Failed to deserialise a local value: " + @default.ToString() + " by " + result.GetType().ToString());
                //}
                return result;
            }
        }

        public struct ConfigData
        {
            public readonly ID<NodeTypeTemp> Type;
            public IEnumerable<Parameter> Parameters;

            public ConfigData(ID<NodeTypeTemp> type, IEnumerable<Parameter> parameters)
            {
                Type = type;
                Parameters = parameters.ToList();
            }

            //public ConfigData(string name, string value)
            //{
            //    Name = name;
            //    Value = value;
            //}
            //public string Name;
            //public string Value;
        }

        public NodeData(string name, Guid? type, ID<NodeTypeTemp> guid, List<ConnectorData> connectors, List<ParameterData> parameters, List<ConfigData> config)
        {
            Name = name;
            Type = type;
            Guid = guid;
            Connectors = connectors;
            Parameters = parameters;
            Config = config;
        }

        public string Name;
        public Guid? Type; //Category
        public ID<NodeTypeTemp> Guid;
        public List<ConnectorData> Connectors;
        public List<ParameterData> Parameters;
        public List<ConfigData> Config;
    }
}

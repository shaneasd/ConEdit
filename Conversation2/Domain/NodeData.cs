using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.Collections.ObjectModel;

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
            public ConnectorData(Id<TConnector> id, Id<TConnectorDefinition> typeId, List<Parameter> parameters)
            {
                Id = id;
                TypeId = typeId;
                Parameters = parameters;
            }

            /// <summary>
            /// Identifies this connector for the node type uniquely. A combination of this ID and a node ID will uniquely identify a connector in a graph.
            /// </summary>
            public readonly Id<TConnector> Id;

            /// <summary>
            /// Identifies the connector definition which classifies this connector
            /// </summary>
            public readonly Id<TConnectorDefinition> TypeId;

            public readonly List<Parameter> Parameters;

            //public Func<IEditable, Output> Make(Func<ID<OutputTemp>, OutputDefinition> definitionFactory)
            //{
            //    ConnectorData thisCopy = this;
            //    return e => definitionFactory(thisCopy.TypeID).Generate(e, thisCopy.Id);
            //}
        }

        public struct ParameterData
        {
            public ParameterData(string name, Id<Parameter> id, ParameterType type, ReadOnlyCollection<ConfigData> config)
            {
                if (config == null)
                    throw new InternalLogicException("Parameter config cannot be null (A)");
                Type = type;
                Name = name;
                Id = id;
                Default = null;
                Config = config;
            }

            public ParameterData(string name, Id<Parameter> id, ParameterType type, ReadOnlyCollection<ConfigData> config, string def)
            {
                if (config == null)
                    throw new InternalLogicException("Parameter config cannot be null (B)");
                Type = type;
                Name = name;
                Id = id;
                Default = def;
                Config = config;
            }

            public ParameterType Type;
            public string Name;
            public Id<Parameter> Id;
            public string Default; //Can be null, string form of the default value
            public ReadOnlyCollection<ConfigData> Config;

            public Parameter Make(Func<ParameterType, string, Id<Parameter>, string, Parameter> parameterFactory)
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
            public readonly Id<NodeTypeTemp> Type;
            public IEnumerable<Parameter> Parameters;

            public ConfigData(Id<NodeTypeTemp> type, IEnumerable<Parameter> parameters)
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

        public NodeData(string name, Guid? type, Id<NodeTypeTemp> guid, List<ConnectorData> connectors, List<ParameterData> parameters, List<ConfigData> config)
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
        public Id<NodeTypeTemp> Guid;
        public List<ConnectorData> Connectors;
        public List<ParameterData> Parameters;
        public List<ConfigData> Config;
    }
}

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
            public ConnectorData(Id<TConnector> id, Id<TConnectorDefinition> typeId, IReadOnlyList<IParameter> parameters)
            {
                Id = id;
                TypeId = typeId;
                Parameters = parameters;
            }

            /// <summary>
            /// Identifies this connector for the node type uniquely. A combination of this ID and a node ID will uniquely identify a connector in a graph.
            /// </summary>
            public Id<TConnector> Id { get; }

            /// <summary>
            /// Identifies the connector definition which classifies this connector
            /// </summary>
            public Id<TConnectorDefinition> TypeId { get; }

            public IReadOnlyList<IParameter> Parameters { get; }

            //public Func<IEditable, Output> Make(Func<ID<OutputTemp>, OutputDefinition> definitionFactory)
            //{
            //    ConnectorData thisCopy = this;
            //    return e => definitionFactory(thisCopy.TypeID).Generate(e, thisCopy.Id);
            //}
        }

        public struct ParameterData
        {
            public ParameterData(string name, Id<Parameter> id, ParameterType type, ReadOnlyCollection<ConfigData> config) : this(name, id, type, config, null)
            {
            }

            public ParameterData(string name, Id<Parameter> id, ParameterType type, ReadOnlyCollection<ConfigData> config, string def)
            {
                if (config == null)
                    throw new ArgumentNullException(nameof(config));
                Type = type;
                Name = name;
                Id = id;
                Default = def;
                Config = config;
            }

            /// <summary>
            /// The type of the parameter: string, int, etc
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "There isn't a more appropriate name")]
            public ParameterType Type { get; }

            public string Name { get; }
            public Id<Parameter> Id { get; }
            public string Default { get; } //Can be null, string form of the default value
            public ReadOnlyCollection<ConfigData> Config { get; }

            public IParameter Make(Func<ParameterType, string, Id<Parameter>, string, IParameter> parameterFactory)
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
            /// <summary>
            /// Unique identifier of the node type corresponding to this configuration
            /// </summary>
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "There isn't a more appropriate name")]
            public Id<NodeTypeTemp> Type { get; }

            /// <summary>
            /// Parameters of the config. Typically it would be a single value (e.g. a path to an icon) but can be any number including zero.
            /// </summary>
            public IEnumerable<IParameter> Parameters { get; }

            /// <param name="type">Unique identifier of the node type corresponding to this configuration</param>
            /// <param name="parameters">Parameters of the config. Typically it would be a single value (e.g. a path to an icon) but can be any number including zero.</param>
            public ConfigData(Id<NodeTypeTemp> type, IEnumerable<IParameter> parameters)
            {
                Type = type;
                Parameters = parameters.ToList();
            }
        }

        public NodeData(string name, Guid? category, Id<NodeTypeTemp> guid, IReadOnlyList<ConnectorData> connectors, IReadOnlyList<ParameterData> parameters, IReadOnlyList<ConfigData> config)
        {
            Name = name;
            Category = category;
            Guid = guid;
            Connectors = connectors;
            Parameters = parameters;
            Config = config;
        }

        public string Name { get; }
        public Guid? Category { get; }
        public Id<NodeTypeTemp> Guid { get; }
        public IReadOnlyList<ConnectorData> Connectors { get; }
        public IReadOnlyList<ParameterData> Parameters { get; }
        public IReadOnlyList<ConfigData> Config { get; }
    }
}

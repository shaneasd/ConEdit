﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.IO;
using System.Collections.ObjectModel;
using TDocument = System.Object;

namespace Conversation
{
    public struct NodeDataGeneratorParameterData
    {
        public Id<Parameter> Guid { get; }
        public string Value { get; }

        public NodeDataGeneratorParameterData(Id<Parameter> guid, string value)
        {
            if (guid == null)
                throw new ArgumentNullException(nameof(guid));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            Guid = guid;
            Value = value;
        }
    }

    /// <summary>
    /// Information needed by a ConverstionNodeData about the generator that constructed it
    /// </summary>
    public interface INodeDataGenerator
    {
        string Name { get; }
        Id<NodeTypeTemp> Guid { get; }
        IReadOnlyList<NodeData.ConfigData> Config { get; }

        /// <summary>
        /// Get all config on the parameter definition of the input parameter
        /// </summary>
        ReadOnlyCollection<NodeData.ConfigData> GetParameterConfig(Id<Parameter> parameterId);
        IConversationNodeData Generate(Id<NodeTemp> id, IEnumerable<NodeDataGeneratorParameterData> parameters, TDocument document);
    }

    //public class CorruptEditableGenerator : INodeDataGenerator
    //{
    //    private ID<NodeTypeTemp> m_guid;
    //    private List<NodeData.ConfigData> m_config = new List<NodeData.ConfigData>();
    //    public event Action GeneratorChanged { add { } remove { } }
    //    public CorruptEditableGenerator(ID<NodeTypeTemp> guid)
    //    {
    //        m_guid = guid;
    //    }

    //    public string Name
    //    {
    //        get { return "Unkown node type"; }
    //    }

    //    public ID<NodeTypeTemp> Guid
    //    {
    //        get { return m_guid; }
    //    }

    //    public List<NodeData.ConfigData> Config
    //    {
    //        get { return m_config; }
    //    }

    //    public IEditable Generate(ID<NodeTemp> id, IEnumerable<Func<IEditable, Output>> connectors, IEnumerable<CorruptedEditable.ParameterData> parameters)
    //    {
    //        return new CorruptedEditable(id, Guid, connectors, parameters);
    //    }
    //}

    public class NodeDataGenerator : INodeDataGenerator
    {
        TypeSet m_types;
        IDictionary<Id<TConnectorDefinition>, ConnectorDefinitionData> m_connectorDefinitions;
        IConnectionRules m_rules;
        NodeData m_data;
        Func<IParameter[], List<IParameter>> m_extraParameters;
        //private List<ExternalFunction> m_generated = new List<ExternalFunction>();
        bool m_exists = true;

        public NodeDataGenerator(NodeData data, TypeSet types, IDictionary<Id<TConnectorDefinition>, ConnectorDefinitionData> connectorDefinitions, IConnectionRules rules, Func<IParameter[], List<IParameter>> extraParameters)
        {
            m_data = data;
            m_types = types;
            m_connectorDefinitions = connectorDefinitions;
            m_rules = rules;
            m_extraParameters = extraParameters ?? (x => new List<IParameter>());
        }

        private IEnumerable<Func<IConversationNodeData, Output>> MakeConnectors()
        {
            Func<NodeData.ConnectorData, Func<IConversationNodeData, Output>> processConnector = c =>
            {
                Func<IConversationNodeData, IReadOnlyList<IParameter>, Output> a = m_connectorDefinitions[c.TypeId].Make(c.Id, m_rules);
                return data => a(data, c.Parameters);
            };
            return m_data.Connectors.Select(processConnector).Evaluate();
        }

        private IEnumerable<IParameter> MakeParameters(IEnumerable<NodeDataGeneratorParameterData> parameterData, TDocument document)
        {
            var parameters = m_data.Parameters.Select(p => p.Make((a, b, c, d) => m_types.Make(a, b, c, d, document))).ToArray();

            var result = parameters.Concat(m_extraParameters(parameters)).ToList();
            foreach (var d in parameterData)
            {
                var parameter = result.SingleOrDefault(p => p.Id == d.Guid);
                if (parameter != null)
                {
                    parameter.TryDeserialiseValue(d.Value);
                }
                else
                {
                    result.Add(new UnknownParameter(d.Guid, d.Value));
                }
            }
            return result;
        }

        public ReadOnlyCollection<NodeData.ConfigData> GetParameterConfig(Id<Parameter> parameterId)
        {
            var parameterDefinition = m_data.Parameters.Single(p => p.Id == parameterId);
            return parameterDefinition.Config;
        }

        public IConversationNodeData Generate(Id<NodeTemp> id, IEnumerable<NodeDataGeneratorParameterData> parameters, TDocument document)
        {
            var result = new ConversationNodeData(this, id, MakeConnectors(), MakeParameters(parameters, document));
            //m_generated.Add(result);
            return result;
        }

        public string Name
        {
            get { return m_exists ? m_data.Name : "Definition Deleted"; }
        }

        public Id<NodeTypeTemp> Guid
        {
            get { return m_data.Guid; }
        }

        public IReadOnlyList<NodeData.ConfigData> Config
        {
            get { return m_data.Config; }
        }

        public void Removed()
        {
            m_exists = false;
            //foreach (var instance in m_generated)
            //    instance.GeneratorChanged();
        }
    }
}

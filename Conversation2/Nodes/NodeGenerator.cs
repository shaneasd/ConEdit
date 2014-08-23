using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    /// <summary>
    /// Information needed by an editable about the generator that constructed it
    /// </summary>
    public interface IEditableGenerator
    {
        string Name { get; }
        ID<NodeTypeTemp> Guid { get; }
        List<NodeData.ConfigData> Config { get; }
        IEnumerable<Func<IEditable, Output>> MakeConnectors();
        List<Parameter> MakeParameters(List<EditableGenerator.ParameterData> parameterData);
    }

    //public class CorruptEditableGenerator : IEditableGenerator
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

    public abstract class EditableGenerator : IEditableGenerator
    {
        public abstract IEditable Generate(ID<NodeTemp> id, List<ParameterData> parameters);
        public abstract string Name { get; }
        public abstract ID<NodeTypeTemp> Guid { get; }
        public abstract List<NodeData.ConfigData> Config { get; }
        public abstract IEnumerable<Func<IEditable, Output>> MakeConnectors();
        public abstract List<Parameter> MakeParameters(List<EditableGenerator.ParameterData> parameterData);
        public struct ParameterData
        {
            public readonly ID<Parameter> Guid;
            public readonly string Value;
            public ParameterData(ID<Parameter> guid, string value)
            {
                Guid = guid;
                Value = value;
            }
        }
    }

    public class GenericEditableGenerator2 : EditableGenerator
    {
        TypeSet m_types;
        IDictionary<ID<TConnectorDefinition>, ConnectorDefinitionData> m_connectorDefinitions;
        IConnectionRules m_rules;
        NodeData m_data;
        Func<Parameter[], List<Parameter>> m_extraParameters;
        private List<ExternalFunction> m_generated = new List<ExternalFunction>();
        bool m_exists = true;

        public GenericEditableGenerator2(NodeData data, TypeSet types, IDictionary<ID<TConnectorDefinition>, ConnectorDefinitionData> connectorDefinitions, IConnectionRules rules)
        {
            m_data = data;
            m_types = types;
            m_connectorDefinitions = connectorDefinitions;
            m_rules = rules;
            m_extraParameters = (x => new List<Parameter>());
        }

        public GenericEditableGenerator2(NodeData data, TypeSet types, IDictionary<ID<TConnectorDefinition>, ConnectorDefinitionData> connectorDefinitions, IConnectionRules rules, Func<Parameter[], List<Parameter>> extraParameters)
        {
            m_data = data;
            m_types = types;
            m_connectorDefinitions = connectorDefinitions;
            m_rules = rules;
            m_extraParameters = extraParameters ?? (x => new List<Parameter>());
        }

        public override IEnumerable<Func<IEditable, Output>> MakeConnectors()
        {
            Func<NodeData.ConnectorData, Func<IEditable, Output>> processConnector = c =>
            {
                Func<IEditable, List<Parameter>, Output> a = m_connectorDefinitions[c.TypeID].Make(c.Id, m_rules);
                return data => a(data, c.Parameters);
            };
            return m_data.Connectors.Select(processConnector).Evaluate();
        }

        public override List<Parameter> MakeParameters(List<EditableGenerator.ParameterData> parameterData)
        {
            var parameters = m_data.Parameters.Select(p => p.Make(m_types.Make)).ToArray();

            var result =  parameters.Concat(m_extraParameters(parameters)).ToList();
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

        public override IEditable Generate(ID<NodeTemp> id, List<EditableGenerator.ParameterData> parameterData)
        {
            var result = new ExternalFunction(this, id, MakeConnectors(), MakeParameters(parameterData));
            m_generated.Add(result);
            return result;
        }

        public override string Name
        {
            get { return m_exists ? m_data.Name : "Definition Deleted"; }
        }

        public override ID<NodeTypeTemp> Guid
        {
            get { return m_data.Guid; }
        }

        public override List<NodeData.ConfigData> Config
        {
            get { return m_data.Config; }
        }

        public void ChangeData(NodeData data)
        {
            m_data = data;
            m_exists = true;
            foreach (var instance in m_generated)
                instance.GeneratorChanged();
        }

        public void Removed()
        {
            m_exists = false;
            foreach (var instance in m_generated)
                instance.GeneratorChanged();
        }
    }
}

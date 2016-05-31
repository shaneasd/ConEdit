using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.IO;
using System.Collections.ObjectModel;
using TDocument = System.Object;

namespace Conversation
{
    public struct EditableGeneratorParameterData
    {
        private readonly Id<Parameter> m_guid;
        public Id<Parameter> Guid { get { return m_guid; } }

        private readonly string m_value;
        public string Value { get { return m_value; } }

        public EditableGeneratorParameterData(Id<Parameter> guid, string value)
        {
            m_guid = guid;
            m_value = value;
        }
    }

    /// <summary>
    /// Information needed by an editable about the generator that constructed it
    /// </summary>
    public interface IEditableGenerator
    {
        string Name { get; }
        Id<NodeTypeTemp> Guid { get; }
        ReadOnlyCollection<NodeData.ConfigData> Config { get; }
        IEnumerable<Func<IEditable, Output>> MakeConnectors();
        List<Parameter> MakeParameters(List<EditableGeneratorParameterData> parameterData, TDocument document);
        ReadOnlyCollection<NodeData.ConfigData> GetParameterConfig(Id<Parameter> parameterId);
        IEditable Generate(Id<NodeTemp> id, List<EditableGeneratorParameterData> parameters, TDocument document);
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
        public abstract IEditable Generate(Id<NodeTemp> id, List<EditableGeneratorParameterData> parameters, TDocument document);
        public abstract string Name { get; }
        public abstract Id<NodeTypeTemp> Guid { get; }
        public abstract ReadOnlyCollection<NodeData.ConfigData> Config { get; }
        public abstract IEnumerable<Func<IEditable, Output>> MakeConnectors();
        public abstract List<Parameter> MakeParameters(List<EditableGeneratorParameterData> parameterData, TDocument document);
        public abstract ReadOnlyCollection<NodeData.ConfigData> GetParameterConfig(Id<Parameter> parameterId);
    }

    public class GenericEditableGenerator2 : EditableGenerator
    {
        TypeSet m_types;
        IDictionary<Id<TConnectorDefinition>, ConnectorDefinitionData> m_connectorDefinitions;
        IConnectionRules m_rules;
        NodeData m_data;
        Func<Parameter[], List<Parameter>> m_extraParameters;
        //private List<ExternalFunction> m_generated = new List<ExternalFunction>();
        bool m_exists = true;

        public GenericEditableGenerator2(NodeData data, TypeSet types, IDictionary<Id<TConnectorDefinition>, ConnectorDefinitionData> connectorDefinitions, IConnectionRules rules)
        {
            m_data = data;
            m_types = types;
            m_connectorDefinitions = connectorDefinitions;
            m_rules = rules;
            m_extraParameters = (x => new List<Parameter>());
        }

        public GenericEditableGenerator2(NodeData data, TypeSet types, IDictionary<Id<TConnectorDefinition>, ConnectorDefinitionData> connectorDefinitions, IConnectionRules rules, Func<Parameter[], List<Parameter>> extraParameters)
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
                Func<IEditable, List<Parameter>, Output> a = m_connectorDefinitions[c.TypeId].Make(c.Id, m_rules);
                return data => a(data, c.Parameters);
            };
            return m_data.Connectors.Select(processConnector).Evaluate();
        }

        public override List<Parameter> MakeParameters(List<EditableGeneratorParameterData> parameterData, TDocument document)
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

        public override ReadOnlyCollection<NodeData.ConfigData> GetParameterConfig(Id<Parameter> parameterId)
        {
            var parameterDefinition = m_data.Parameters.Single(p => p.Id == parameterId);
            return parameterDefinition.Config;
        }

        public override IEditable Generate(Id<NodeTemp> id, List<EditableGeneratorParameterData> parameters, TDocument document)
        {
            var result = new ExternalFunction(this, id, MakeConnectors(), MakeParameters(parameters, document));
            //m_generated.Add(result);
            return result;
        }

        public override string Name
        {
            get { return m_exists ? m_data.Name : "Definition Deleted"; }
        }

        public override Id<NodeTypeTemp> Guid
        {
            get { return m_data.Guid; }
        }

        public override ReadOnlyCollection<NodeData.ConfigData> Config
        {
            get { return new ReadOnlyCollection<NodeData.ConfigData>(m_data.Config); }
        }

        public void ChangeData(NodeData data)
        {
            m_data = data;
            m_exists = true;
            //foreach (var instance in m_generated)
            //    instance.GeneratorChanged();
        }

        public void Removed()
        {
            m_exists = false;
            //foreach (var instance in m_generated)
            //    instance.GeneratorChanged();
        }
    }
}

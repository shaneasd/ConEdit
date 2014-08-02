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
    }

    public class CorruptEditableGenerator : IEditableGenerator
    {
        private ID<NodeTypeTemp> m_guid;
        private List<NodeData.ConfigData> m_config = new List<NodeData.ConfigData>();
        public CorruptEditableGenerator(ID<NodeTypeTemp> guid)
        {
            m_guid = guid;
        }

        public string Name
        {
            get { return "Unkown node type"; }
        }

        public ID<NodeTypeTemp> Guid
        {
            get { return m_guid; }
        }

        public List<NodeData.ConfigData> Config
        {
            get { return m_config; }
        }

        public IEditable Generate(ID<NodeTemp> id, IEnumerable<Func<IEditable, Output>> connectors, IEnumerable<CorruptedEditable.ParameterData> parameters)
        {
            return new CorruptedEditable(id, Guid, connectors, parameters);
        }
    }

    public abstract class EditableGenerator : IEditableGenerator
    {
        public abstract IEditable Generate(ID<NodeTemp> id);
        public abstract IEditable Generate(ID<NodeTemp> id, List<ParameterData> parameters);
        public abstract string Name { get; }
        public abstract ID<NodeTypeTemp> Guid { get; }
        public abstract List<NodeData.ConfigData> Config { get; }

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

    //public class GenericEditableGenerator : EditableGenerator
    //{
    //    private Func<ID<NodeTemp>, ExternalFunction> m_generator;
    //    private string m_name;
    //    private readonly ID<NodeTypeTemp> m_guid;
    //    private List<NodeData.ConfigData> m_config;
    //    public GenericEditableGenerator(string name, ID<NodeTypeTemp> guid, List<NodeData.ConfigData> config, Func<ID<NodeTemp>, EditableGenerator, ExternalFunction> generator)
    //    {
    //        m_generator = id => generator(id, this);
    //        m_name = name;
    //        m_guid = guid;
    //        m_config = config;
    //    }
    //    public override IEditable Generate(ID<NodeTemp> id)
    //    {
    //        return m_generator(id);
    //    }

    //    public void Rename(string name)
    //    {
    //        m_name = name;
    //    }

    //    public override string Name { get { return m_name; } }
    //    public override ID<NodeTypeTemp> Guid { get { return m_guid; } }
    //    public override List<NodeData.ConfigData> Config { get { return m_config; } }
    //}

    public class GenericEditableGenerator2 : EditableGenerator
    {
        TypeSet m_types;
        IDictionary<ID<TConnectorDefinition>, ConnectorDefinitionData> m_connectorDefinitions;
        IConnectionRules m_rules;
        NodeData m_data;
        Func<Parameter[], List<Parameter>> m_extraParameters;

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

        private IEnumerable<Func<IEditable, Output>> MakeConnectors()
        {
            Func<NodeData.ConnectorData, Func<IEditable, Output>> processConnector = c =>
            {
                Func<IEditable, List<Parameter>, Output> a = m_connectorDefinitions[c.TypeID].Make(c.Id, m_rules);
                return data => a(data, c.Parameters);
            };
            return m_data.Connectors.Select(processConnector).Evaluate();
        }

        private Parameter[] MakeParameters()
        {
            var parameters = m_data.Parameters.Select(p => p.Make(m_types.Make)).ToArray();

            return parameters.Concat(m_extraParameters(parameters)).ToArray();
        }

        public override IEditable Generate(ID<NodeTemp> id)
        {
            return new ExternalFunction(this, id, MakeConnectors(), MakeParameters());
        }

        public override IEditable Generate(ID<NodeTemp> id, List<EditableGenerator.ParameterData> parameterData)
        {
            List<Parameter> parameters = MakeParameters().ToList();
            foreach (var d in parameterData)
            {
                var parameter = parameters.SingleOrDefault(p => p.Id == d.Guid);
                if (parameter != null)
                {
                    parameter.TryDeserialiseValue(d.Value);
                }
                else
                {
                    parameters.Add(new UnknownParameter(d.Guid, d.Value));
                }
            }
            return new ExternalFunction(this, id, MakeConnectors(), parameters);
        }

        public override string Name
        {
            get { return m_data.Name; }
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
        }
    }
}

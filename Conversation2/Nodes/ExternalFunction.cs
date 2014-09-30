using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Utilities;

namespace Conversation
{
    public class ExternalFunction : IEditable
    {
        private ID<NodeTemp> m_id;
        private readonly IEditableGenerator m_generator;
        private List<Parameter> m_parameters;
        private readonly IEnumerable<Output> m_connectors;

        public ID<NodeTypeTemp> NodeTypeID
        {
            get { return m_generator.Guid; }
        }

        public string Name
        {
            get { return m_generator.Name; }
        }

        public List<NodeData.ConfigData> Config
        {
            get { return m_generator.Config; }
        }

        public IEnumerable<Output> Connectors
        {
            get
            {
                return m_connectors;
            }
        }

        public IEnumerable<Parameter> Parameters
        {
            get
            {
                return m_parameters;
            }
        }

        public void ChangeId(ID<NodeTemp> id)
        {
            m_id = id;
        }

        public void TryDecorrupt()
        {
            foreach (var corrupted in Parameters.Where(p => p.Corrupted))
            {
                corrupted.TryDecorrupt();
            }
        }

        public ID<NodeTemp> NodeID { get { return m_id; } }

        public event Action Linked;
        protected void OnOutputLinked()
        {
            Linked.Execute();
        }

        public ExternalFunction(IEditableGenerator generator, ID<NodeTemp> id, IEnumerable<Func<IEditable, Output>> connectors, IEnumerable<Parameter> parameters)
        {
            m_id = id;
            m_parameters = parameters.ToList();
            m_connectors = connectors.Select(i => i(this)).OrderBy(o=>o.GetName()).Evaluate(); //TODO: Ordering by name is a little awkward what with many connectors not having one
            foreach (var connector in Connectors)
            {
                connector.Connected += OnOutputLinked;
                connector.Disconnected += OnOutputLinked;
            }

            //TODO: Assert that connectors match node definition?

            m_generator = generator;
        }


        public SimpleUndoPair RemoveUnknownParameter(UnknownParameter p)
        {
            int index = m_parameters.IndexOf(p);
            return new SimpleUndoPair
            {
                Redo = () => { m_parameters.Remove(p); },
                Undo = () => { m_parameters.Insert(index, p); },
            };
        }

        private List<EditableGenerator.ParameterData> ParameterData()
        {
            return Parameters.Select(p => new EditableGenerator.ParameterData(p.Id, p.ValueAsString())).ToList();
        }

        public void GeneratorChanged()
        {
            m_parameters = m_generator.MakeParameters(ParameterData());
            //TODO: Update connectors
        }
    }
}

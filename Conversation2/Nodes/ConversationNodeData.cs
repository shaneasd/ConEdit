using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Utilities;
using System.Collections.ObjectModel;

namespace Conversation
{
    internal class ConversationNodeData : IConversationNodeData
    {
        private Id<NodeTemp> m_id;
        private readonly INodeDataGenerator m_generator;
        private List<IParameter> m_parameters;
        private readonly IEnumerable<Output> m_connectors;

        public Id<NodeTypeTemp> NodeTypeId => m_generator.Guid;

        public string Name => m_generator.Name;

        public string Description => m_generator.Description;

        public IReadOnlyList<NodeData.ConfigData> Config => m_generator.Config;

        public IEnumerable<Output> Connectors => m_connectors;

        public IEnumerable<IParameter> Parameters => m_parameters;

        public void ChangeId(Id<NodeTemp> id)
        {
            m_id = id;
        }

        public Id<NodeTemp> NodeId => m_id;

        public event Action Linked;
        protected void OnOutputLinked(Output o)
        {
            Linked.Execute();
        }

        public ConversationNodeData(INodeDataGenerator generator, Id<NodeTemp> id, IEnumerable<Func<IConversationNodeData, Output>> connectors, IEnumerable<IParameter> parameters)
        {
            m_id = id;
            m_parameters = parameters.ToList();
            m_connectors = connectors.Select(i => i(this)).OrderBy(o => o.GetName()).Evaluate(); //TODO: Ordering by name is a little awkward what with many connectors not having one
            foreach (var connector in Connectors)
            {
                connector.Connected += OnOutputLinked;
                connector.Disconnected += OnOutputLinked;
            }

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
    }
}

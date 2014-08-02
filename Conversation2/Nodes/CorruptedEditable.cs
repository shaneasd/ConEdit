using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;

namespace Conversation
{
    public class CorruptedEditable : IEditable
    {
        public struct ParameterData
        {
            public ID<Parameter> Id;
            public string Value;
        }

        public event Action Linked;
        protected void OnOutputLinked()
        {
            Linked.Execute();
        }

        private IEnumerable<Output> m_connectors;
        private ID<NodeTemp> m_nodeID;
        private ID<NodeTypeTemp> m_nodeTypeID;

        public CorruptedEditable(ID<NodeTemp> nodeID, ID<NodeTypeTemp> nodeTypeID, IEnumerable<Func<IEditable, Output>> connectors, IEnumerable<ParameterData> parameters)
        {
            m_nodeID = nodeID;
            m_nodeTypeID = nodeTypeID;
            m_connectors = connectors.Select(i => i(this)).Evaluate();
            foreach (var connector in Connectors)
            {
                connector.Connected += OnOutputLinked;
                connector.Disconnected += OnOutputLinked;
            }
        }

        public ID<NodeTemp> NodeID { get { return m_nodeID; } }

        public ID<NodeTypeTemp> NodeTypeID { get { return m_nodeTypeID; } }

        public string Name
        {
            get { return "Unkown node type"; }
        }

        List<NodeData.ConfigData> m_config = new List<NodeData.ConfigData>();
        public List<NodeData.ConfigData> Config
        {
            get { return m_config; }
        }

        public IEnumerable<Parameter> Parameters
        {
            get { return Enumerable.Empty<Parameter>(); }
        }

        public IEnumerable<Output> Connectors
        {
            get
            {
                return m_connectors;
            }
        }

        public void ChangeId(ID<NodeTemp> id)
        {
            throw new NotImplementedException();
        }

        public void TryDecorrupt()
        {
            //throw new NotImplementedException();
        }


        public SimpleUndoPair RemoveUnknownParameter(UnknownParameter p)
        {
            throw new NotImplementedException(); //TODO: Can this happen?
        }
    }
}

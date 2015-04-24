using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Collections.ObjectModel;

namespace ConversationEditor
{
    internal class NodeRendererCustomization : IEditable
    {
        //public static Guid DefaultRenderer(ID<NodeTypeTemp> nodeType)
        //{
        //    return DefaultEnumEditor.Factory.GUID;
        //}

        private List<Parameter> m_parameters;
        private MapConfig<ID<NodeTypeTemp>, Guid> m_typeMapConfig;
        public NodeRendererCustomization()
        {
            m_parameters = new List<Parameter>();
        }

        public NodeRendererCustomization(IDataSource datasource, MapConfig<ID<NodeTypeTemp>, Guid> typeMapConfig, IEnumerable<NodeUI.IFactory> allRenderers)
        {
            m_typeMapConfig = typeMapConfig;
            m_parameters = new List<Parameter>();
            foreach (var type in datasource.AllNodes())
            {
                var options = allRenderers.Where(e => e.WillRender(type.Guid)).Select(e => Tuple.Create(e.Guid, e.DisplayName)).ToList();
                Guid def = m_typeMapConfig[type.Guid];
                var enumeration = new Enumeration(options, ParameterType.Basic.ConvertFrom(type.Guid), def);
                var p = new EnumParameter(type.Name, ID<Parameter>.ConvertFrom(type.Guid), enumeration, def.ToString());
                m_parameters.Add(p);
            }
        }

        public IEnumerable<Parameter> Parameters
        {
            get { return m_parameters; }
        }

        public ID<NodeTemp> NodeId
        {
            get { throw new NotImplementedException(); }
        }

        public ID<NodeTypeTemp> NodeTypeId
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { return "Customize node renderers"; }
        }

        public ReadOnlyCollection<NodeData.ConfigData> Config
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Output> Connectors
        {
            get { return Enumerable.Empty<Output>(); }
        }

        public event Action Linked { add { } remove { } }

        public void ChangeId(ID<NodeTemp> id)
        {
            throw new NotImplementedException();
        }

        public void TryDecorrupt()
        {
            throw new NotImplementedException();
        }

        public Utilities.SimpleUndoPair RemoveUnknownParameter(UnknownParameter p)
        {
            throw new NotImplementedException();
        }
    }
}

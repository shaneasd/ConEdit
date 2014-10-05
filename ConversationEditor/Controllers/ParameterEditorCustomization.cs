using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    class ParameterEditorCustomization : IEditable
    {
        public static Guid DefaultEditor(ID<ParameterType> type, WillEdit willEdit)
        {
            if (willEdit.IsDecimal(type))
                return DefaultDecimalEditor.Factory.GUID;
            else if (willEdit.IsDynamicEnum(type))
                return DefaultDynamicEnumEditor.Factory.GUID;
            else if (willEdit.IsEnum(type))
                return DefaultEnumEditor.Factory.GUID;
            else if (willEdit.IsInteger(type))
                return DefaultIntegerEditor.Factory.GUID;
            else if (type == BaseTypeBoolean.PARAMETER_TYPE)
                return DefaultBooleanEditor.Factory.GUID;
            else if (type == BaseTypeString.PARAMETER_TYPE)
                return DefaultStringEditor.Factory.GUID;
            else if (type == BaseTypeLocalizedString.PARAMETER_TYPE)
                return DefaultLocalizedStringEditor.Factory.GUID;
            else if (type == BaseTypeAudio.PARAMETER_TYPE)
                return DefaultAudioEditor.Factory.GUID;
            else
                return Guid.Empty;
        }

        public static IEnumerable<ParameterEditorChoice> DefaultEditors
        {
            get
            {
                yield return new ParameterEditorChoice(typeof(DefaultBooleanEditor));
                yield return new ParameterEditorChoice(typeof(DefaultDecimalEditor));
                yield return new ParameterEditorChoice(typeof(DefaultDynamicEnumEditor));
                yield return new ParameterEditorChoice(typeof(DefaultEnumEditor));
                yield return new ParameterEditorChoice(typeof(DefaultIntegerEditor));
                //yield return new ParameterEditorChoice(typeof(DefaultFilePathEditor));
                yield return new ParameterEditorChoice(typeof(DefaultStringEditor));
                yield return new ParameterEditorChoice(typeof(DefaultLocalizedStringEditor));
                yield return new ParameterEditorChoice(typeof(DefaultAudioEditor));
            }
        }

        private List<Parameter> m_parameters;
        private MapConfig<ID<ParameterType>, Guid> m_typeMapConfig;
        public ParameterEditorCustomization()
        {
            m_parameters = new List<Parameter>();
            m_parameters.Add(new EnumParameter("Character", ID<Parameter>.New(), new Enumeration(new[] { Tuple.Create(Guid.NewGuid(), "value") }, ID<ParameterType>.New())));
        }

        public ParameterEditorCustomization(IDataSource datasource, MapConfig<ID<ParameterType>, Guid> typeMapConfig, List<IParameterEditorFactory> allEditors)
        {
            m_typeMapConfig = typeMapConfig;
            m_parameters = new List<Parameter>();
            foreach (var type in datasource.ParameterTypes)
            {
                var options = allEditors.Where(e => e.WillEdit(type, WillEdit.Create(datasource))).Select(e => Tuple.Create(e.Guid, e.Name)).ToList();
                Guid def = m_typeMapConfig[type];
                var enumeration = new Enumeration(options, type, def);
                var p = new EnumParameter(datasource.GetTypeName(type), ID<Parameter>.ConvertFrom(type), enumeration, def.ToString());
                m_parameters.Add(p);
            }
        }

        public IEnumerable<Parameter> Parameters
        {
            get { return m_parameters; }
        }

        public ID<NodeTemp> NodeID
        {
            get { throw new NotImplementedException(); }
        }

        public ID<NodeTypeTemp> NodeTypeID
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { return "Customize parameter editors"; }
        }

        public List<NodeData.ConfigData> Config
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

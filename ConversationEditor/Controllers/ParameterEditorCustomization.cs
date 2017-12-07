using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using System.Collections.ObjectModel;

namespace ConversationEditor
{
    internal class ParameterEditorCustomization : IConversationNodeData
    {
        public static Guid DefaultEditor(ParameterType type, WillEdit willEdit)
        {
            if (willEdit.IsDecimal(type))
                return DefaultDecimalEditorFactory.StaticId;
            else if (willEdit.IsDynamicEnum(type))
                return DefaultDynamicEnumEditorFactory.StaticId;
            else if (willEdit.IsLocalDynamicEnum(type))
                return DefaultLocalDynamicEnumEditorFactory.StaticId;
            else if (willEdit.IsEnum(type))
                return DefaultEnumEditorFactory.StaticId;
            else if (willEdit.IsInteger(type))
                return DefaultIntegerEditorFactory.StaticId;
            else if (type == BaseTypeBoolean.PARAMETER_TYPE)
                return DefaultBooleanEditorFactory.StaticId;
            else if (type == BaseTypeString.ParameterType)
                return DefaultStringEditorFactory.StaticId;
            else if (willEdit.IsLocalizedString(type))
                return DefaultLocalizedStringEditorFactory.StaticId;
            else if (type == BaseTypeAudio.PARAMETER_TYPE)
                return DefaultAudioEditorFactory.StaticId;
            else if (type.IsSet)
                return DefaultSetEditorFactory.StaticId;
            else
                return Guid.Empty;
        }

        private List<Parameter> m_parameters;
        private MapConfig<ParameterType, Guid> m_typeMapConfig;
        public ParameterEditorCustomization()
        {
            m_parameters = new List<Parameter>();
            m_parameters.Add(new EnumParameter("Character", Id<Parameter>.New(), new ImmutableEnumeration(new[] { Tuple.Create(Guid.NewGuid(), "value") }, ParameterType.Basic.New(), ""), null));
        }

        public ParameterEditorCustomization(IDataSource datasource, MapConfig<ParameterType, Guid> typeMapConfig, IEnumerable<IParameterEditorFactory> allEditors)
        {
            m_typeMapConfig = typeMapConfig;
            m_parameters = new List<Parameter>();
            foreach (var type in datasource.ParameterTypes)
            {
                var options = allEditors.Where(e => e.WillEdit(type, WillEdit.Create(datasource))).Select(e => Tuple.Create(e.Guid, e.Name)).ToList();
                Guid def = m_typeMapConfig[type];
                var enumeration = new ImmutableEnumeration(options, type, def);
                var p = new EnumParameter(datasource.GetTypeName(type), Id<Parameter>.ConvertFrom(type), enumeration, def.ToString());
                m_parameters.Add(p);
            }
        }

        public IEnumerable<IParameter> Parameters
        {
            get { return m_parameters; }
        }

        public Id<NodeTemp> NodeId
        {
            get { throw new NotImplementedException(); }
        }

        public Id<NodeTypeTemp> NodeTypeId
        {
            get { throw new NotImplementedException(); }
        }
        
        public string Name
        {
            get { return "Customize parameter editors"; }
        }

        public IReadOnlyList<NodeData.ConfigData> Config
        {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Output> Connectors
        {
            get { return Enumerable.Empty<Output>(); }
        }

        public event Action Linked { add { } remove { } }

        public void ChangeId(Id<NodeTemp> id)
        {
            throw new NotImplementedException();
        }

        public Utilities.SimpleUndoPair RemoveUnknownParameter(UnknownParameter p)
        {
            throw new NotImplementedException();
        }
    }
}

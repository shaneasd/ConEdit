using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public struct DynamicEnumerationData
    {
        //Global variables evil. This is a hack to get around the fact that every time you call DomainFile.Data this type gets recreated from the nodes so there's no permanent mapping between dynamic enumeration and source
        public static Dictionary<ID<ParameterType>, DynamicEnumParameter.Source> sourcesHack = new Dictionary<ID<ParameterType>, DynamicEnumParameter.Source>();

        public DynamicEnumerationData(string name, ID<ParameterType> typeID)
        {
            Name = name;
            TypeID = typeID;
            if (!sourcesHack.ContainsKey(typeID))
                sourcesHack[typeID] = new DynamicEnumParameter.Source();
            m_source = sourcesHack[typeID];
        }
        public string Name;
        public ID<ParameterType> TypeID;
        private DynamicEnumParameter.Source m_source;

        public IEnumerable<string> Options { get { return m_source.Options; } }

        public Parameter Make(string name, ID<Parameter> id, string defaultValue)
        {
            return new DynamicEnumParameter(name, id, m_source, TypeID, defaultValue);
        }
    }
}

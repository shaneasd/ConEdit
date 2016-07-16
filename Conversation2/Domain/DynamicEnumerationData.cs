using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.IO;

namespace Conversation
{
    public struct DynamicEnumerationData
    {
        public DynamicEnumerationData(string name, ParameterType typeId)
        {
            Name = name;
            m_typeId = typeId;
            //m_source = source;
        }
        public string Name { get; set; }
        private readonly ParameterType m_typeId;
        public ParameterType TypeId { get { return m_typeId; } }
        //private DynamicEnumParameter.Source m_source;

        //public IEnumerable<string> Options { get { return m_source.Options; } }

        public Parameter Make(string name, Id<Parameter> id, string defaultValue, DynamicEnumParameter.Source source)
        {
            return new DynamicEnumParameter(name, id, source, TypeId, defaultValue, false);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.IO;

namespace Conversation
{
    public struct LocalDynamicEnumerationData
    {
        public LocalDynamicEnumerationData(string name, ParameterType typeId)
        {
            Name = name;
            m_typeId = typeId;
        }
        public string Name { get; set; }
        private readonly ParameterType m_typeId;
        public ParameterType TypeId { get { return m_typeId; } }

        public Parameter Make(string name, Id<Parameter> id, string defaultValue, DynamicEnumParameter.Source source)
        {
            return new DynamicEnumParameter(name, id, source, TypeId, defaultValue, true);
        }
    }
}

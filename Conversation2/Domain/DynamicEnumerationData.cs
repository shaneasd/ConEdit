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
            TypeId = typeId;
        }
        public string Name { get; }
        public ParameterType TypeId { get; }

        public Parameter Make(string name, Id<Parameter> id, string defaultValue, DynamicEnumParameter.Source source)
        {
            return new DynamicEnumParameter(name, id, source, TypeId, defaultValue, false);
        }
    }
}

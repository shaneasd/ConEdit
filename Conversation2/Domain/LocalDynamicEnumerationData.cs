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
        public LocalDynamicEnumerationData(string name, ParameterType typeID)
        {
            Name = name;
            TypeId = typeID;
        }
        public string Name;
        public ParameterType TypeId;

        public IEnumerable<string> GetOptions(DynamicEnumParameter.Source source) { return source.Options; }

        public Parameter Make(string name, Id<Parameter> id, string defaultValue, DynamicEnumParameter.Source source)
        {
            return new DynamicEnumParameter(name, id, source, TypeId, defaultValue, true);
        }
    }
}

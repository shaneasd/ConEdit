using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conversation
{
    public struct LocalizedStringData
    {
        public LocalizedStringData(string name, ParameterType typeId)
        {
            Name = name;
            TypeId = typeId;
        }

        public string Name { get; }
        public ParameterType TypeId { get; }
    }
}

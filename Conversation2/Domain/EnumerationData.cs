using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    public struct EnumerationData
    {
        public struct Element
        {
            public string Name { get; set; }
            public Guid Guid { get; set; }
            public Element(string name, Guid guid)
            {
                Name = name;
                Guid = guid;
            }
        }

        public EnumerationData(string name, ParameterType typeId, IEnumerable<Element> elements)
        {
            Name = name;
            TypeId = typeId;
            Elements = elements.ToList();
        }
        public string Name { get; }
        public ParameterType TypeId { get; }
        public IEnumerable<Element> Elements { get; }
    }
}

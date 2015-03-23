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
            public string Name;
            public Guid Guid;
            public Element(string name, Guid guid)
            {
                Name = name;
                Guid = guid;
            }
        }

        public EnumerationData(string name, ParameterType guid, IEnumerable<Element> elements)
        {
            Name = name;
            TypeID = guid;
            Elements = elements.ToList();
            //Default = null;
        }
        //public EnumerationData(string name, Guid guid, IEnumerable<Element> elements, Guid def)
        //{
        //    Name = name;
        //    Guid = guid;
        //    Elements = elements.ToList();
        //    Default = def;
        //}
        //public EnumerationData(string name, Guid guid, IEnumerable<Element> elements, string def)
        //{
        //    Name = name;
        //    Guid = guid;
        //    Elements = elements.ToList();
        //    Default = def;
        //}
        public string Name;
        public ParameterType TypeID;
        //public Or<string, Guid> Default; //can be null
        public List<Element> Elements;

        public IEnumeration Make()
        {
            var elements = Elements.Select(e => Tuple.Create(e.Guid, e.Name));

            //if (Default == null)
                return new Enumeration(elements, TypeID);
            //else
            //{
            //    var name = Name;
            //    var guid = Guid;
            //    return Default.Transformed(s => new Enumeration(elements,  guid, s), g => new Enumeration(elements, guid, g));
            //}
        }
    }
}

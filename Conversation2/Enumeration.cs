using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    public interface IEnumeration
    {
        IEnumerable<Guid> Options { get; }
        string GetName(Guid value);
        ParameterType TypeId { get; }
        Or<string, Guid> Default { get; }
    }

    public static class EnumerationUtil
    {
        public static Parameter ParameterEnum(this IEnumeration e, string name, ID<Parameter> id, string defaultValue)
        {
            return new EnumParameter(name, id, e, defaultValue);
        }

        public static Parameter ParameterSet(this IEnumeration e, string name, ID<Parameter> id, string defaultValue)
        {
            return new SetParameter(name, id, e, defaultValue);
        }
    }


    public class WrapperEnumeration : IEnumeration
    {
        private Func<IEnumerable<Guid>> m_options;
        private Func<Guid, string> m_getName;
        private ParameterType m_typeID;
        private Func<Or<string, Guid>> m_default;
        public WrapperEnumeration(Func<IEnumerable<Guid>> options, Func<Guid, string> getName, Func<Or<string, Guid>> @default, ParameterType typeId)
        {
            m_options = options;
            m_getName = getName;
            m_typeID = typeId;
            m_default = @default;
        }

        public IEnumerable<Guid> Options
        {
            get { return m_options(); }
        }

        public string GetName(Guid value)
        {
            return m_getName(value);
        }

        public ParameterType TypeId
        {
            get { return m_typeID; }
        }

        public Or<string, Guid> Default
        {
            get { return m_default(); }
        }
    }

    public class Enumeration : IEnumeration
    {
        protected Dictionary<Guid, string> m_options;

        public string GetName(Guid value)
        {
            if (value == Guid.Empty)
                return Default.Transformed(s => s, g => null);
            if (!m_options.ContainsKey(value))
                return null;
            return m_options[value];
        }

        public Enumeration(IEnumerable<Tuple<Guid, string>> options, ParameterType typeId)
        {
            m_options = options.ToDictionary(t => t.Item1, t => t.Item2);
            m_typeId = typeId;
            Default = Guid.Empty;
        }

        public Enumeration(IEnumerable<Tuple<Guid, string>> options, ParameterType typeId, Or<string, Guid> def)
            : this(options, typeId)
        {
            Default = def;
        }

        public IEnumerable<Guid> Options { get { return m_options.Keys; } }
        public Or<string, Guid> Default { get; protected set; }

        private readonly ParameterType m_typeId;
        public ParameterType TypeId { get { return m_typeId; } }
    }

    public class MutableEnumeration : Enumeration
    {
        public MutableEnumeration(IEnumerable<Tuple<Guid, string>> options, ParameterType typeId, Or<string, Guid> def)
            : base(options, typeId, def)
        {
        }

        //public void SetName(Guid guid, string name)
        //{
        //    m_options[guid] = name;
        //}

        public void Add(Guid guid, string name)
        {
            m_options.Add(guid, name);
        }

        public void Remove(Guid guid)
        {
            m_options.Remove(guid);
        }

        public EnumerationData GetData(string name)
        {
            EnumerationData data;
            data.TypeID = this.TypeId;
            data.Name = name;
            data.Elements = m_options.Select(o => new EnumerationData.Element(o.Value, o.Key)).ToList();
            return data;
        }

        internal void SetOptions(List<EnumerationData.Element> elements)
        {
            m_options.Clear();
            foreach (var option in elements)
            {
                Add(option.Guid, option.Name);
            }
        }
    }
}

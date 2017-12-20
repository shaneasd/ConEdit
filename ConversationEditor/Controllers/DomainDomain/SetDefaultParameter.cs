using Conversation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace ConversationEditor
{
    public class SetDefaultParameter : Parameter<ReadOnlySet<Guid>>, ISetParameter
    {
        public new static ParameterType TypeId { get; } = ParameterType.ValueSetType.Parse("set:452fbad8-976c-47d3-8b6f-06f871e06044");
        private readonly Func<Dictionary<ParameterType, IEnumerable<EnumerationData.Element>>> m_getEnumeration;
        private readonly Func<ParameterType> m_getCurrentEnumType;
        IEnumerable<EnumerationData.Element> Enumeration
        {
            get
            {
                var selectedEnumerationType = m_getCurrentEnumType();
                if (selectedEnumerationType.Guid != Guid.Empty)
                    return m_getEnumeration()[selectedEnumerationType];
                else
                    return Enumerable.Empty<EnumerationData.Element>();
            }
        }

        public SetDefaultParameter(Func<Dictionary<ParameterType, IEnumerable<EnumerationData.Element>>> getEnumeration, Func<ParameterType> getCurrentEnumType)
            : base("Default", DomainIDs.ParameterDefault, TypeId, "", Tuple.Create(new ReadOnlySet<Guid>(), false))
        {
            m_getEnumeration = getEnumeration;
            m_getCurrentEnumType = getCurrentEnumType;
        }

        public IEnumerable<Guid> Options
        {
            get
            {
                return Enumeration.Select(e => e.Guid);
            }
        }

        public string GetName(Guid value)
        {
            foreach (var e in Enumeration)
            {
                if (e.Guid == value)
                    return e.Name;
            }
            return null;
        }

        protected override bool ValueValid(ReadOnlySet<Guid> value)
        {
            return SetParameter.StaticValueValid(Enumeration.Select(e => e.Guid), value);
        }

        protected override Tuple<ReadOnlySet<Guid>, bool> DeserializeValueInner(string value)
        {
            var result = SetParameter.StaticDeserialize(Enumeration.Select(e => e.Guid), value);
            return result;
        }

        protected override string InnerValueAsString()
        {
            return SetParameter.SerializeSet(Value);
        }

        private static string DisplayStringForSet(ReadOnlySet<Guid> value, Func<Guid, string> GetName)
        {
            return string.Join(" + ", value.Select(v => GetName(v) ?? SetParameter.InvalidValue).OrderBy(a => a));
        }

        public override string DisplayValue(Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localize)
        {
            if (Corrupted)
                return ValueAsString();
            else
                return DisplayStringForSet(Value, GetName);
        }

        public string SerializedValue
        {
            get
            {
                if (Corrupted)
                    return ValueAsString();
                else
                    return SetParameter.SerializeSet(Value);
            }
        }
    }
}

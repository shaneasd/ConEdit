﻿using Conversation;
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

        //TODO: Isn't there already a mechanism for this at a higher level? (Copied from SetParameter)
        string m_textOverride = null; //initial string representation of parameter that failed parsing (or null if parsing succeeded or a new value has been specified.

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
            if (result.Item2)
                m_textOverride = value;
            else
                m_textOverride = null;
            return result;
        }

        protected override void OnSetValue(ReadOnlySet<Guid> value)
        {
            m_textOverride = null;
        }

        protected override string InnerValueAsString()
        {
            if (m_textOverride != null)
                return m_textOverride;
            return SetParameter.SerializeSet(Value);
        }

        public override string DisplayValue(Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localize)
        {
            if (m_textOverride != null)
                return m_textOverride;
            return string.Join(" + ", Value.Select(v => GetName(v) ?? SetParameter.InvalidValue).OrderBy(a => a));
        }

        public string SerializedValue
        {
            get
            {
                if (m_textOverride != null)
                    return m_textOverride;
                else
                    return SetParameter.SerializeSet(Value);
            }
        }
    }
}

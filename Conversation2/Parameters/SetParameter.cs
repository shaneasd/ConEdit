﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Utilities;

namespace Conversation
{
    public class SetParameter : Parameter<ReadonlySet<Guid>>, ISetParameter
    {
        IEnumeration m_enumeration;
        public SetParameter(string name, Id<Parameter> id, IEnumeration enumeration, string defaultValue)
            : base(name, id, ParameterType.ValueSetType.Of(enumeration.TypeId), defaultValue ?? enumeration.DefaultValue.Transformed(a => a, b => b.ToString()), StaticDeserialize(enumeration, defaultValue ?? enumeration.DefaultValue.Transformed(a => a, b => b.ToString())))
        {
            m_enumeration = enumeration;
        }

        //TODO: Isn't there already a mechanism for this at a higher level?
        string m_textOverride = null; //initial string representation of parameter that failed parsing (or null if parsing succeeded or a new value has been specified.

        protected override Tuple<ReadonlySet<Guid>, bool> DeserializeValueInner(string value)
        {
            var result = StaticDeserialize(m_enumeration, value);
            if ( result.Item2 )
                m_textOverride = value;
            else
                m_textOverride = null;
            return result;
        }

        private static Tuple<ReadonlySet<Guid>, bool> StaticDeserialize(IEnumeration enumeration, string value)
        {
            if ( value == null )
                return Tuple.Create((ReadonlySet<Guid>)null, true);

            string[] values = value.Split('+').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
            Guid[] guids = new Guid[values.Length];
            bool valid = true;
            for (int i = 0; i < values.Length; i++)
            {
                Guid g;
                bool v = Guid.TryParse(values[i], out g);
                guids[i] = v ? g : Guid.Empty;
                valid &= v;
            }

            if (!valid)
            {
                return Tuple.Create((ReadonlySet<Guid>)null, true);
            }
            else
            {
                var val = new ReadonlySet<Guid>(guids);
                return Tuple.Create(val, !StaticValueValid(enumeration, val));
            }
        }

        protected override void OnSetValue(ReadonlySet<Guid> value)
        {
            m_textOverride = null;
        }

        protected override bool ValueValid(ReadonlySet<Guid> value)
        {
            return StaticValueValid(m_enumeration, value);
        }

        private static bool StaticValueValid(IEnumeration enumeration, ReadonlySet<Guid> value)
        {
            return value.All(v => enumeration.Options.Contains(v));
        }

        protected override string InnerValueAsString()
        {
            if (m_textOverride != null)
                return m_textOverride;
            return string.Join("+", Value.Select(v => v.ToString()));
        }

        public IEnumerable<Guid> Options
        {
            get
            {
                return m_enumeration.Options;
            }
        }

        public string GetName(Guid value)
        {
            if (value == Guid.Empty)
                return InvalidValue;
            else
                return m_enumeration.GetName(value);
        }

        public override string DisplayValue(Func<Id<LocalizedText>, string> localize)
        {
            if (m_textOverride != null)
                return m_textOverride;
            return string.Join(" + ", Value.Select(v => GetName(v) ?? InvalidValue).OrderBy(a => a));
        }

        public const string InvalidValue = "ERROR: Unknown enumeration value";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Utilities;

namespace Conversation
{
    public class SetParameter : Parameter<ReadOnlySet<Guid>>, ISetParameter
    {
        IEnumeration m_enumeration;
        public SetParameter(string name, Id<Parameter> id, IEnumeration enumeration, string defaultValue)
            : base(name, id, ParameterType.ValueSetType.Of(enumeration.TypeId), defaultValue ?? enumeration.DefaultValue.Transformed(a => a, b => b.ToString()), StaticDeserialize(enumeration.Options, defaultValue ?? enumeration.DefaultValue.Transformed(a => a, b => b.ToString())))
        {
            m_enumeration = enumeration;
        }

        //TODO: Isn't there already a mechanism for this at a higher level?
        string m_textOverride = null; //initial string representation of parameter that failed parsing (or null if parsing succeeded or a new value has been specified.

        protected override Tuple<ReadOnlySet<Guid>, bool> DeserializeValueInner(string value)
        {
            var result = StaticDeserialize(m_enumeration.Options, value);
            if ( result.Item2 )
                m_textOverride = value;
            else
                m_textOverride = null;
            return result;
        }

        public static Tuple<ReadOnlySet<Guid>, bool> StaticDeserialize(IEnumerable<Guid> options, string value)
        {
            if ( value == null )
                return Tuple.Create((ReadOnlySet<Guid>)null, true);

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
                return Tuple.Create((ReadOnlySet<Guid>)null, true);
            }
            else
            {
                var val = new ReadOnlySet<Guid>(guids);
                return Tuple.Create(val, !StaticValueValid(options, val));
            }
        }

        protected override void OnSetValue(ReadOnlySet<Guid> value)
        {
            m_textOverride = null;
        }

        protected override bool ValueValid(ReadOnlySet<Guid> value)
        {
            return StaticValueValid(m_enumeration.Options, value);
        }

        public static bool StaticValueValid(IEnumerable<Guid> options, ReadOnlySet<Guid> value)
        {
            return value.All(v => options.Contains(v));
        }

        protected override string InnerValueAsString()
        {
            if (m_textOverride != null)
                return m_textOverride;
            return SerializeSet(Value);
        }

        public static string SerializeSet(ReadOnlySet<Guid> value)
        {
            return string.Join("+", value.Select(v => v.ToString()));
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

        public override string DisplayValue(Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localize)
        {
            if (m_textOverride != null)
                return m_textOverride;
            return string.Join(" + ", Value.Select(v => GetName(v) ?? InvalidValue).OrderBy(a => a));
        }

        public const string InvalidValue = "ERROR: Unknown enumeration value";
    }
}

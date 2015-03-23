using System;
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
        public SetParameter(string name, ID<Parameter> id, IEnumeration enumeration, string defaultValue = null)
            : base(name, id, ParameterType.Set.Of(enumeration.TypeId), defaultValue)
        {
            m_enumeration = enumeration;
        }

        string m_textOverride = null; //initial string representation of parameter that failed parsing (or null if parsing succeeded or a new value has been specified.

        protected override bool DeserialiseValue(string value)
        {
            string[] values = value.Split('+').Select(s=>s.Trim()).Where(s=>s.Length > 0).ToArray();
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
                m_textOverride = value;
                return false;
            }
            else
            {
                Value = new ReadonlySet<Guid>(guids);
                m_textOverride = null;
                return true;
            }
        }

        public override ReadonlySet<Guid> Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                base.Value = value;
                m_textOverride = null;
            }
        }

        protected override string InnerValueAsString()
        {
            if (m_textOverride != null)
                return m_textOverride;
            return string.Join("+", m_value.Select(v => v.ToString()));
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
                return INVALID_VALUE;
            else
                return m_enumeration.GetName(value);
        }

        public override string DisplayValue(Func<ID<LocalizedText>, string> localize)
        {
            if (m_textOverride != null)
                return m_textOverride;
            return string.Join(" + ", m_value.Select(v => GetName(v) ?? INVALID_VALUE).OrderBy(a => a));
        }

        public const string INVALID_VALUE = "ERROR: Unknown enumeration value";
    }
}

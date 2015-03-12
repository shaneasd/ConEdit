using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Utilities;

namespace Conversation
{
    public class SetParameter : Parameter<GuidSet>, ISetParameter
    {
        IEnumeration m_enumeration;
        public SetParameter(string name, ID<Parameter> id, IEnumeration enumeration, string defaultValue = null)
            //: base(name, id, enumeration.TypeId, enumeration.Default.Transformed(a => Guid.Empty.ToString(), a => a.ToString()))
            : base(name, id, enumeration.TypeId, defaultValue)
        {
            m_enumeration = enumeration;
        }

        string m_textOverride = null;

        protected override bool DeserialiseValue(string value)
        {
            string[] values = value.Split('+');
            Guid[] guids = new Guid[values.Length];
            bool valid = true;
            for (int i = 0; i < values.Length; i++)
            {
                Guid g;
                bool v = Guid.TryParse(values[i], out g);
                guids[i] = v ? g : Guid.Empty;
                valid &= v;
            }

            Value = new GuidSet(guids);

            if (!valid)
            {
                m_textOverride = value;
                return false;
            }
            else
            {
                m_textOverride = null;
                return true;
            }
        }

        public override GuidSet Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                base.Value = value;
                EditorSelected = value;
            }
        }

        protected override string InnerValueAsString()
        {
            if (m_textOverride != null)
                return m_textOverride;
            return string.Join("+", m_value.Values.Select(v => v.ToString()));
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
            return string.Join(" + ", m_value.Values.Select(v => GetName(v) ?? INVALID_VALUE).OrderBy(a => a));
        }

        public const string INVALID_VALUE = "ERROR: Unknown enumeration value";

        public GuidSet EditorSelected
        {
            get;
            set;
        }
    }
}

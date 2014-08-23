using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Utilities;

namespace Conversation
{
    public class EnumParameter : Parameter<Guid>, IEnumParameter
    {
        IEnumeration m_enumeration;
        public EnumParameter(string name, ID<Parameter> id, IEnumeration enumeration, string defaultValue = null)
            //: base(name, id, enumeration.TypeId, enumeration.Default.Transformed(a => Guid.Empty.ToString(), a => a.ToString()))
            : base(name, id, enumeration.TypeId, defaultValue)
        {
            m_enumeration = enumeration;
        }

        string m_textOverride = null;

        protected override bool DeserialiseValue(string value)
        {
            Guid guid;
            if (!Guid.TryParse(value, out guid))
            {
                m_textOverride = value;
                guid = Guid.Empty;
            }
            else
            {
                m_textOverride = null;
            }
            //return false;
            //TODO: Should this cause it to fail?
            //if ( !m_enumeration.Options.Contains(guid))
            //    return false;
            Value = guid;
            return true;
        }

        public override Guid Value
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
            return m_value.ToString();
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
            if (m_textOverride != null && value == Guid.Empty)
                return m_textOverride;
            else
                return m_enumeration.GetName(value);
        }

        public override string DisplayValue(Func<ID<LocalizedText>, string> localize)
        {
            return GetName(m_value) ?? INVALID_VALUE;
        }

        public const string INVALID_VALUE = "ERROR: Unknown enumeration value";


        public Guid EditorSelected
        {
            get;
            set;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    public class DynamicEnumParameter : Parameter<string>, IDynamicEnumParameter
    {
        public class Source
        {
            public Source() { }
            Dictionary<DynamicEnumParameter, string> m_options = new Dictionary<DynamicEnumParameter, string>();
            public IEnumerable<string> Options { get { return m_options.Values.Distinct().Except("".Only()); } }
            public void RegisterUsage(DynamicEnumParameter user, string value)
            {
                m_options[user] = value;
            }

            public void Clear()
            {
                m_options.Clear();
            }

            internal void DeregisterUsage(DynamicEnumParameter dynamicEnumParameter)
            {
                m_options.Remove(dynamicEnumParameter);
            }
        }

        Source m_source;

        public DynamicEnumParameter(string name, ID<Parameter> id, Source source, ParameterType typeId, string defaultValue = null)
            : base(name, id, typeId, defaultValue)
        {
            m_source = source;
        }

        protected override bool DeserialiseValue(string value)
        {
            Value = value;
            return true;
        }

        protected override string InnerValueAsString()
        {
            return m_value;
        }

        public IEnumerable<string> Options
        {
            get { return m_source.Options; }
        }

        public override string Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                base.Value = value;
                if (m_source != null) //m_source is null during construction but we just use a value of "" anyway so it doesn't matter if it doesn't get registered
                {
                    m_source.RegisterUsage(this, value);
                }
            }
        }

        /// <summary>
        /// Notifies the parameter that its parent node has been removed and as such it is no longer a valid usage of the underlying data value
        /// OR its parent nodes removal has been undone and it is once again a valid usage of the underlying data value
        /// </summary>
        /// <param name="removed"></param>
        public void Removed(bool removed)
        {
            if (removed)
                m_source.DeregisterUsage(this);
            else
                Value = Value;
        }

        public override string DisplayValue(Func<ID<LocalizedText>, string> localize)
        {
            return m_value;
        }
    }
}

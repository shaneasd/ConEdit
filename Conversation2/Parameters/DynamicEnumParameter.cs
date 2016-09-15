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
            //TODO: May not want the weakreference thing

            public Source() { }
            Dictionary<WeakReference<DynamicEnumParameter>, string> m_options = new Dictionary<WeakReference<DynamicEnumParameter>, string>(new WeakReferenceComparer<DynamicEnumParameter>());
            public IEnumerable<string> Options { get { PurgeOptions(); return m_options.Values.Distinct().Except("".Only()); } }
            public void RegisterUsage(DynamicEnumParameter user, string value)
            {
                m_options[new WeakReference<DynamicEnumParameter>(user)] = value;
            }

            public void Clear()
            {
                m_options.Clear();
            }

            internal void DeregisterUsage(DynamicEnumParameter dynamicEnumParameter)
            {
                m_options.Remove(new WeakReference<DynamicEnumParameter>(dynamicEnumParameter));
            }

            void PurgeOptions()
            {
                foreach (var key in m_options.Keys)
                {
                    DynamicEnumParameter val;
                    if (!key.TryGetTarget(out val))
                        m_options.Remove(key);
                }
            }
        }

        Source m_source;
        private bool m_local;

        public DynamicEnumParameter(string name, Id<Parameter> id, Source source, ParameterType typeId, string defaultValue, bool local)
            : base(name, id, typeId, defaultValue, StaticDeserialize(defaultValue))
        {
            m_source = source;
            m_local = local;

            //TODO: This appears to not get deregistered properly for default values
            // Specifically when deleting a node or cancelling creation of a node, the node's parameters do not get cleaned up (i.e. deregistered)
            if (!Corrupted)
                m_source.RegisterUsage(this, this.Value);
        }

        protected override Tuple<string, bool> DeserializeValueInner(string value)
        {
            //The static version called from the destructor cannot do this as it can't refer to this.
            //We don't need it to as we can just register the usage within the construction
            if (value != null)
            {
                if (m_source != null) //m_source is null during construction so we explicitly do the registration there for that case
                {
                    m_source.RegisterUsage(this, value);
                }
            }

            return StaticDeserialize(value);
        }

        private static Tuple<string, bool> StaticDeserialize(string value)
        {
            return Tuple.Create(value, value == null);
        }

        protected override string InnerValueAsString()
        {
            return Value;
        }

        public IEnumerable<string> Options
        {
            get { return m_source.Options; }
        }

        protected override bool ValueValid(string value)
        {
            return value != null;
        }

        protected override void OnSetValue(string value)
        {
            if (m_source != null) //m_source is null during construction so we explicitly do the registration there for that case
            {
                m_source.RegisterUsage(this, value);
            }
        }

        public bool Local
        {
            get
            {
                return m_local;
            }
        }

        ///// <summary>
        ///// Notifies the parameter that its parent node has been removed and as such it is no longer a valid usage of the underlying data value
        ///// OR its parent nodes removal has been undone and it is once again a valid usage of the underlying data value
        ///// </summary>
        ///// <param name="removed"></param>
        //public void Removed(bool removed)
        //{
        //    if (removed)
        //        m_source.DeregisterUsage(this);
        //    else
        //        Value = Value;
        //}

        public override string DisplayValue(Func<Id<LocalizedText>, string> localize)
        {
            return Value;
        }

        /// <summary>
        /// Change data source to newSource and update usage in newSource to reflect current value
        /// </summary>
        /// <param name="newSource">The new data source to use</param>
        public void MergeInto(Source newSource)
        {
            m_source.DeregisterUsage(this);
            newSource.RegisterUsage(this, this.Value);
            m_source = newSource;
        }
    }
}

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
        public EnumParameter(string name, Id<Parameter> id, IEnumeration enumeration, string defaultValue)
            : base(name, id, enumeration.TypeId, defaultValue ?? enumeration.DefaultValue.Transformed(a => a, b => b.ToString()), StaticDeserialize(enumeration, defaultValue ?? enumeration.DefaultValue.Transformed(a => a, b => b.ToString())))
        {
            m_enumeration = enumeration;
        }

        protected override Tuple<Guid, bool> DeserializeValueInner(string value)
        {
            return StaticDeserialize(m_enumeration, value);
        }

        private static Tuple<Guid, bool> StaticDeserialize(IEnumeration enumeration, string value)
        {
            Guid guid;
            if (!Guid.TryParse(value, out guid))
            {
                return Tuple.Create(default(Guid), true);
            }
            else
            {
                return Tuple.Create(guid, !StaticValueValid(enumeration, guid));
            }
        }

        protected override void OnSetValue(Guid value)
        {
            EditorSelected = value;
        }
        protected override bool ValueValid(Guid value)
        {
            return StaticValueValid(m_enumeration, value);
        }

        private static bool StaticValueValid(IEnumeration enumeration, Guid value)
        {
            if (enumeration == null)
                throw new ArgumentNullException(nameof(enumeration));
            return enumeration.Options.Contains(value);
        }

        protected override string InnerValueAsString()
        {
            return Value.ToString();
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
            return m_enumeration.GetName(value);
        }

        public override string DisplayValue(Func<Id<LocalizedText>, string> localize)
        {
            if (Corrupted)
                return ValueAsString();
            else
                return GetName(Value);
        }

        //When adding/editing a node parameter definition which is of an enum type the user must be
        //able to select which enum type in an enum editor but they must also be able to select a
        //default value for the parameter in another enum editor. The valid options for the default
        //value editor are dependent on the selection in the enum type editor. The enum type parameter
        //doesn't actually get changed though until the user clicks ok. As such the Value of the type
        //parameter cannot be used to communicate to the enum default parameter (which is a special
        //type, EnumDefaultParameter, defined in DomainDomain) what the current enum type selection 
        //is. To get around this, the EditorSelected member has been added to represent the value of
        //the parameter that has been selected most recently in the editor regardless of the true
        //current Value of the parameter.
        //TODO: This member could go into a specialized type in DomainDomain alongside and
        //      communicating with EnumDefaultParameter to avoid poluting the EnumParameter interface.
        public Guid EditorSelected
        {
            get;
            set;
        }
    }
}

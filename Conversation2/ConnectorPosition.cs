using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conversation
{
    //TODO: This is very UI centric. Is there some way to shift it up a level
    //Possibly simply make this an IEnumParameter?
    public abstract class ConnectorPosition
    {
        private class CTop : ConnectorPosition
        {
            public static ConnectorPosition Instance { get; } = new CTop();
            private CTop() : base("Top", Guid.Parse("24c96d32-1704-4c85-b2bf-b8da8731ea47")) { }
            public override T ForPosition<T>(Func<T> top, Func<T> bottom, Func<T> left, Func<T> right) { return top(); }
        }

        private class CBottom : ConnectorPosition
        {
            public static ConnectorPosition Instance { get; } = new CBottom();
            private CBottom() : base("Bottom", Guid.Parse("b5461736-18f1-417c-8a54-2c5a1726483b")) { }
            public override T ForPosition<T>(Func<T> top, Func<T> bottom, Func<T> left, Func<T> right) { return bottom(); }
        }

        private class CLeft : ConnectorPosition
        {
            public static ConnectorPosition Instance { get; } = new CLeft();
            private CLeft() : base("Left", Guid.Parse("adb2301c-a858-44e8-b76c-93e538231960")) { }
            public override T ForPosition<T>(Func<T> top, Func<T> bottom, Func<T> left, Func<T> right) { return left(); }
        }

        private class CRight : ConnectorPosition
        {
            public static ConnectorPosition Instance { get; } = new CRight();
            private CRight() : base("Right", Guid.Parse("d8b8efae-3949-47b3-af7b-8db1e402489e")) { }
            public override T ForPosition<T>(Func<T> top, Func<T> bottom, Func<T> left, Func<T> right) { return right(); }
        }

        public static ConnectorPosition Top { get; } = CTop.Instance;
        public static ConnectorPosition Bottom { get; } = CBottom.Instance;
        public static ConnectorPosition Left { get; } = CLeft.Instance;
        public static ConnectorPosition Right { get; } = CRight.Instance;

        public abstract T ForPosition<T>(Func<T> top, Func<T> bottom, Func<T> left, Func<T> right);

        public static bool operator ==(ConnectorPosition a, ConnectorPosition b)
        {
            return object.Equals(a, b);
        }

        public static bool operator !=(ConnectorPosition a, ConnectorPosition b)
        {
            return !object.Equals(a, b);
        }

        public override bool Equals(object obj)
        {
            ConnectorPosition other = obj as ConnectorPosition;
            if (other == null)
                return false;
            else
                return other.m_guid == m_guid;
        }

        public override int GetHashCode()
        {
            return m_guid.GetHashCode();
        }

        public static ConnectorPosition Read(IEnumParameter parameter)
        {
            return (new[] { Top, Bottom, Left, Right }).First(a => a.m_guid == parameter.Value);
        }

        private ConnectorPosition(string name, Guid guid)
        {
            m_name = name;
            m_guid = guid;
        }
        private Guid m_guid;
        private string m_name;

        private Tuple<Guid, string> Tuple => System.Tuple.Create(m_guid, m_name);

        public static ParameterType EnumId { get; } = ParameterType.Parse("2b075746-9b6e-4d6e-ad39-a083049374f2");
        public static Id<Parameter> ParameterId { get; } = Id<Parameter>.Parse("43903044-1ef9-4c9f-a782-6219fb8e7826");

        public static EnumParameter MakeParameter()
        {
            IEnumeration enumeration = new ImmutableEnumeration(new[] { Top.Tuple, Bottom.Tuple, Left.Tuple, Right.Tuple }, EnumId, Bottom.m_guid);
            return new EnumParameter("Position", ParameterId, enumeration, null);
        }

        public EnumerationData.Element Element => new EnumerationData.Element(m_name, m_guid);

        public static EnumerationData PositionConnectorDefinition => new EnumerationData("Position", EnumId, new List<EnumerationData.Element>() { Top.Element, Bottom.Element, Left.Element, Right.Element });
    }
}

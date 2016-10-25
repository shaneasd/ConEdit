using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ConversationEditor
{
    internal abstract class TypeChoice
    {
        public readonly Type m_type;
        public readonly string m_typeName;
        public readonly string m_assembly;

        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType())
                return false;
            var other = obj as TypeChoice;
            return m_type == other.m_type && m_typeName == other.m_typeName && m_assembly == other.m_assembly;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        protected TypeChoice(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            m_type = type;
            m_typeName = type.FullName;
            m_assembly = type.Assembly.Location;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.Reflection.Assembly.LoadFile", Justification ="Can't see an alternative method...")]
        protected TypeChoice(string assembly, string type)
             : this(Assembly.LoadFile(assembly).GetType(type))
        {
        }

        public abstract string DisplayName
        {
            get;
        }
    }
}

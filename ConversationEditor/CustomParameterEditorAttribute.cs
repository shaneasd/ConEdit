using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConversationEditor
{
    //Not using this anymore

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomParameterEditorAttribute : Attribute
    {
        public readonly string TypeName;
        public CustomParameterEditorAttribute(string typeName)
        {
            TypeName = typeName;
        }
    }
}

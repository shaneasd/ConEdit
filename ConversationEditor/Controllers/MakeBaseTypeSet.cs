using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using System.IO;

namespace ConversationEditor
{
    internal static class BaseTypeSet
    {
        //TODO: Could use a redesign (or at least better documentation to explain the BaseTypeSet <-> TypeSet relationship
        public static TypeSet Make()
        {
            var result = new TypeSet();
            //Populate with the standard types
            result.AddInteger(BaseTypeInteger.Data);
            //result.AddInteger(BaseTypeInteger.PARAMETER_TYPE, (name, id) => new IntegerParameter(name, id, BaseTypeInteger.PARAMETER_TYPE));
            result.AddDecimal(BaseTypeDecimal.Data);
            //result.AddDecimal(BaseTypeDecimal.PARAMETER_TYPE, (name, id) => new DecimalParameter(name, id, BaseTypeDecimal.PARAMETER_TYPE));
            result.AddOther(StringParameter.ParameterType, "String", (name, id, def, document) => new StringParameter(name, id, def));
            result.AddOther(LocalizedStringParameter.ParameterType, "Localized String", (name, id, def, document) => new LocalizedStringParameter(name, id));
            result.AddOther(BooleanParameter.ParameterType, "Boolean", (name, id, def, document) => new BooleanParameter(name, id, def));
            result.AddOther(AudioParameter.ParameterType, "Audio", (name, id, def, document) => new AudioParameter(name, id));
            return result;
        }

        public static Dictionary<ParameterType, string> BasicTypeMap()
        {
            var typeMap = new Dictionary<ParameterType, string>();
            typeMap[BaseTypeInteger.PARAMETER_TYPE] = "Int32";
            typeMap[BaseTypeDecimal.PARAMETER_TYPE] = "Decimal";
            typeMap[StringParameter.ParameterType] = "String";
            typeMap[LocalizedStringParameter.ParameterType] = "RuntimeConversation.LocalizedString";
            typeMap[BooleanParameter.ParameterType] = "Boolean";
            typeMap[AudioParameter.ParameterType] = "RuntimeConversation.Audio";
            return typeMap;
        }
    }
}

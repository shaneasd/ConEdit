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
            result.AddInteger(BaseTypeInteger.Data, false);
            //result.AddInteger(BaseTypeInteger.PARAMETER_TYPE, (name, id) => new IntegerParameter(name, id, BaseTypeInteger.PARAMETER_TYPE));
            result.AddDecimal(BaseTypeDecimal.Data);
            //result.AddDecimal(BaseTypeDecimal.PARAMETER_TYPE, (name, id) => new DecimalParameter(name, id, BaseTypeDecimal.PARAMETER_TYPE));
            result.AddOther(BaseTypeString.ParameterType, "String", (name, id, val, document) => new StringParameter(name, id, BaseTypeString.ParameterType, val));
            result.AddOther(BaseTypeLocalizedString.PARAMETER_TYPE, "Localized String", (name, id, val, document) => new LocalizedStringParameter(name, id, BaseTypeLocalizedString.PARAMETER_TYPE, val));
            result.AddOther(BaseTypeBoolean.PARAMETER_TYPE, "Boolean", (name, id, val, document) => new BooleanParameter(name, id, BaseTypeBoolean.PARAMETER_TYPE, val));
            result.AddOther(BaseTypeAudio.PARAMETER_TYPE, "Audio", (name, id, val, document) => new AudioParameter(name, id, BaseTypeAudio.PARAMETER_TYPE));
            return result;
        }

        public static Dictionary<ParameterType, string> BasicTypeMap()
        {
            var typeMap = new Dictionary<ParameterType, string>();
            typeMap[BaseTypeInteger.PARAMETER_TYPE] = "Int32";
            typeMap[BaseTypeDecimal.PARAMETER_TYPE] = "Decimal";
            typeMap[BaseTypeString.ParameterType] = "String";
            typeMap[BaseTypeLocalizedString.PARAMETER_TYPE] = "RuntimeConversation.LocalizedString";
            typeMap[BaseTypeBoolean.PARAMETER_TYPE] = "Boolean";
            typeMap[BaseTypeAudio.PARAMETER_TYPE] = "RuntimeConversation.Audio";
            return typeMap;
        }
    }
}

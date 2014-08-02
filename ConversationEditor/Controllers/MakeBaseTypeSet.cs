using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    public static class BaseTypeSet
    {
        public static TypeSet Make()
        {
            var result = new TypeSet();
            //Populate with the standard types
            result.AddInteger(BaseTypeInteger.Data);
            //result.AddInteger(BaseTypeInteger.PARAMETER_TYPE, (name, id) => new IntegerParameter(name, id, BaseTypeInteger.PARAMETER_TYPE));
            result.AddDecimal(BaseTypeDecimal.Data);
            //result.AddDecimal(BaseTypeDecimal.PARAMETER_TYPE, (name, id) => new DecimalParameter(name, id, BaseTypeDecimal.PARAMETER_TYPE));
            result.AddOther(BaseTypeString.PARAMETER_TYPE, (name, id) => new StringParameter(name, id, BaseTypeString.PARAMETER_TYPE));
            result.AddOther(BaseTypeLocalizedString.PARAMETER_TYPE, (name, id) => new LocalizedStringParameter(name, id, BaseTypeLocalizedString.PARAMETER_TYPE));
            result.AddOther(BaseTypeBoolean.PARAMETER_TYPE, (name, id) => new BooleanParameter(name, id, BaseTypeBoolean.PARAMETER_TYPE));
            result.AddOther(BaseTypeAudio.PARAMETER_TYPE, (name, id) => new AudioParameter(name, id, new Audio(Guid.NewGuid().ToString() + ".ogg"), BaseTypeAudio.PARAMETER_TYPE));
            return result;
        }

        public static Dictionary<Guid, string> BasicTypeMap()
        {
            var typeMap = new Dictionary<Guid, string>();
            typeMap[BaseTypeInteger.PARAMETER_TYPE.Guid] = "Int32";
            typeMap[BaseTypeDecimal.PARAMETER_TYPE.Guid] = "Decimal";
            typeMap[BaseTypeString.PARAMETER_TYPE.Guid] = "String";
            typeMap[BaseTypeLocalizedString.PARAMETER_TYPE.Guid] = "RuntimeConversation.LocalizedString";
            typeMap[BaseTypeBoolean.PARAMETER_TYPE.Guid] = "Boolean";
            typeMap[BaseTypeAudio.PARAMETER_TYPE.Guid] = "RuntimeConversation.Audio";
            return typeMap;
        }
    }
}

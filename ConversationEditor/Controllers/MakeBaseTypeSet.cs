﻿using System;
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
            result.AddOther(BaseTypeString.PARAMETER_TYPE, "String", (name, id, val) => new StringParameter(name, id, BaseTypeString.PARAMETER_TYPE, val));
            result.AddOther(BaseTypeLocalizedString.PARAMETER_TYPE, "Localized String", (name, id, val) => new LocalizedStringParameter(name, id, BaseTypeLocalizedString.PARAMETER_TYPE, val));
            result.AddOther(BaseTypeBoolean.PARAMETER_TYPE, "Boolean", (name, id, val) => new BooleanParameter(name, id, BaseTypeBoolean.PARAMETER_TYPE, val));
            result.AddOther(BaseTypeAudio.PARAMETER_TYPE, "Audio", (name, id, val) => new AudioParameter(name, id, BaseTypeAudio.PARAMETER_TYPE, val));//, Guid.NewGuid().ToString() + ".ogg"));
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

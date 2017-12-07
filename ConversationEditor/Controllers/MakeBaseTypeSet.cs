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
            result.AddLocalizedString(BaseTypeLocalizedString.Data);
            result.AddOther(StringParameter.ParameterType, "String", (name, id, def, document) => new StringParameter(name, id, def));
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
            typeMap[BaseTypeLocalizedString.ParameterType] = "RuntimeConversation.LocalizedString"; //TODO: LOC: Doesn't properly deal with user defined localized strings
            typeMap[BooleanParameter.ParameterType] = "Boolean";
            typeMap[AudioParameter.ParameterType] = "RuntimeConversation.Audio";
            return typeMap;
        }

        internal static ConstantTypeSet MakeConstant(IEnumerable<DynamicEnumerationData> dynamicEnumerations, IEnumerable<LocalDynamicEnumerationData> localDynamicEnumerations, IEnumerable<EnumerationData> enumerations, IEnumerable<DecimalData> decimals, IEnumerable<IntegerData> integers, IEnumerable<LocalizedStringData> localizedStrings)
        {
            IEnumerable<Tuple<ParameterType, string, ParameterGenerator>> others = new List<Tuple<ParameterType, string, ParameterGenerator>>()
            {
                Tuple.Create<ParameterType, string, ParameterGenerator>(StringParameter.ParameterType, "String", (name, id, def, document) => new StringParameter(name, id, def)),
                Tuple.Create<ParameterType, string, ParameterGenerator>(BooleanParameter.ParameterType, "Boolean", (name, id, def, document) => new BooleanParameter(name, id, def)),
                Tuple.Create<ParameterType, string, ParameterGenerator>(AudioParameter.ParameterType, "Audio", (name, id, def, document) => new AudioParameter(name, id)),
            };

            var result = new ConstantTypeSet(dynamicEnumerations, localDynamicEnumerations, enumerations, decimals.Concat(BaseTypeDecimal.Data.Only()), integers.Concat(BaseTypeInteger.Data.Only()), localizedStrings.Concat(BaseTypeLocalizedString.Data.Only()), others);


            return result;
        }
    }
}

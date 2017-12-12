using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;
using Utilities;
using System.IO;

namespace ConversationEditor
{
    /// <summary>
    /// Factory methods for constructing type sets to be used by conversations including all of the standard types (default int, string, boolean, etc)
    /// </summary>
    internal static class ConversationTypeSetFactory
    {
        public static TypeSet Make()
        {
            var result = new TypeSet();
            result.AddInteger(BaseTypeInteger.Data);
            result.AddDecimal(BaseTypeDecimal.Data);
            result.AddLocalizedString(BaseTypeLocalizedString.Data);
            result.AddOther(StringParameter.ParameterType, "String", (name, id, def, document) => new StringParameter(name, id, def));
            result.AddOther(BooleanParameter.ParameterType, "Boolean", (name, id, def, document) => new BooleanParameter(name, id, def));
            result.AddOther(AudioParameter.ParameterType, "Audio", (name, id, def, document) => new AudioParameter(name, id));
            return result;
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

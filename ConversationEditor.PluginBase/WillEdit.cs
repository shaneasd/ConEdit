using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    public class WillEdit
    {
        public Func<ParameterType, bool> IsInteger { get; }
        public Func<ParameterType, bool> IsDecimal { get; }
        public Func<ParameterType, bool> IsEnum { get; }
        public Func<ParameterType, bool> IsDynamicEnum { get; }
        public Func<ParameterType, bool> IsLocalDynamicEnum { get; }
        public Func<ParameterType, bool> IsLocalizedString { get; }

        public WillEdit(Func<ParameterType, bool> isInteger, Func<ParameterType, bool> isDecimal, Func<ParameterType, bool> isEnum, Func<ParameterType, bool> isDynamicEnum, Func<ParameterType, bool> isLocalDynamicEnum, Func<ParameterType, bool> isLocalizedString)
        {
            IsInteger = isInteger;
            IsDecimal = isDecimal;
            IsEnum = isEnum;
            IsDynamicEnum = isDynamicEnum;
            IsLocalDynamicEnum = isLocalDynamicEnum;
            IsLocalizedString = isLocalizedString;
        }

        public static WillEdit Create(IDataSource datasource)
        {
            return new WillEdit
            (
                isInteger: datasource.IsInteger,
                isDecimal: datasource.IsDecimal,
                isEnum: datasource.IsEnum,
                isDynamicEnum: datasource.IsDynamicEnum,
                isLocalDynamicEnum: datasource.IsLocalDynamicEnum,
                isLocalizedString: datasource.IsLocalizedString
            );
        }
    }

}

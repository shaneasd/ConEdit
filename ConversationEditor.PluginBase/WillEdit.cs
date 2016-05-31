using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    public class WillEdit
    {
        public readonly Func<ParameterType, bool> IsInteger;
        public readonly Func<ParameterType, bool> IsDecimal;
        public readonly Func<ParameterType, bool> IsEnum;
        public readonly Func<ParameterType, bool> IsDynamicEnum;
        public readonly Func<ParameterType, bool> IsLocalDynamicEnum;

        public WillEdit(Func<ParameterType, bool> isInteger, Func<ParameterType, bool> isDecimal, Func<ParameterType, bool> isEnum, Func<ParameterType, bool> isDynamicEnum, Func<ParameterType,bool> isLocalDynamicEnum)
        {
            this.IsInteger = isInteger;
            this.IsDecimal = isDecimal;
            this.IsEnum = isEnum;
            this.IsDynamicEnum = isDynamicEnum;
            this.IsLocalDynamicEnum = isLocalDynamicEnum;
        }

        public static WillEdit Create(IDataSource datasource)
        {
            return new WillEdit
            (
                isInteger : type => datasource.IsInteger(type),
                isDecimal : type => datasource.IsDecimal(type),
                isEnum : type => datasource.IsEnum(type),
                isDynamicEnum : type => datasource.IsDynamicEnum(type),
                isLocalDynamicEnum: type => datasource.IsLocalDynamicEnum(type)
            );
        }
    }

}

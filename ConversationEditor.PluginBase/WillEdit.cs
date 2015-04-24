using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    public struct WillEdit
    {
        public Func<ParameterType, bool> IsInteger;
        public Func<ParameterType, bool> IsDecimal;
        public Func<ParameterType, bool> IsEnum;
        public Func<ParameterType, bool> IsDynamicEnum;

        public static WillEdit Create(IDataSource datasource)
        {
            return new WillEdit
            {
                IsInteger = type => datasource.IsInteger(type),
                IsDecimal = type => datasource.IsDecimal(type),
                IsEnum = type => datasource.IsEnum(type),
                IsDynamicEnum = type => datasource.IsDynamicEnum(type),
            };
        }
    }

}

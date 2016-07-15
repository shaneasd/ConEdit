using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Conversation;

namespace ConversationEditor
{
    public class WillEdit
    {
        private readonly Func<ParameterType, bool> m_isInteger;
        private readonly Func<ParameterType, bool> m_isDecimal;
        private readonly Func<ParameterType, bool> m_isEnum;
        private readonly Func<ParameterType, bool> m_isDynamicEnum;
        private readonly Func<ParameterType, bool> m_isLocalDynamicEnum;

        public Func<ParameterType, bool> IsInteger { get { return m_isInteger; } }
        public Func<ParameterType, bool> IsDecimal { get { return m_isDecimal; } }
        public Func<ParameterType, bool> IsEnum { get { return m_isEnum; } }
        public Func<ParameterType, bool> IsDynamicEnum { get { return m_isDynamicEnum; } }
        public Func<ParameterType, bool> IsLocalDynamicEnum { get { return m_isLocalDynamicEnum; } }

        public WillEdit(Func<ParameterType, bool> isInteger, Func<ParameterType, bool> isDecimal, Func<ParameterType, bool> isEnum, Func<ParameterType, bool> isDynamicEnum, Func<ParameterType, bool> isLocalDynamicEnum)
        {
            m_isInteger = isInteger;
            m_isDecimal = isDecimal;
            m_isEnum = isEnum;
            m_isDynamicEnum = isDynamicEnum;
            m_isLocalDynamicEnum = isLocalDynamicEnum;
        }

        public static WillEdit Create(IDataSource datasource)
        {
            return new WillEdit
            (
                isInteger: type => datasource.IsInteger(type),
                isDecimal: type => datasource.IsDecimal(type),
                isEnum: type => datasource.IsEnum(type),
                isDynamicEnum: type => datasource.IsDynamicEnum(type),
                isLocalDynamicEnum: type => datasource.IsLocalDynamicEnum(type)
            );
        }
    }

}

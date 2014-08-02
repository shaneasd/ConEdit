using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public struct IntegerData
    {
        public IntegerData(string name, ID<ParameterType> typeID, int? max = null, int? min = null/*, int? def = null*/)
        {
            Name = name;
            TypeID = typeID;
            Max = max;
            Min = min;
            //Default = def;
        }
        public string Name;
        public ID<ParameterType> TypeID;
        public int? Max;
        public int? Min;
        //public int? Default;

        //public IntegerParameter Make(string name, ID<Parameter> id)
        //{
        //    return new IntegerParameter(name, id, TypeID, Max ?? int.MaxValue, Min ?? int.MinValue);
        //}

        public IntegerParameter.Definition Definition()
        {
            return new IntegerParameter.Definition() { Max = Max, Min = Min };
        }
    }
}

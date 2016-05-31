using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public struct DecimalData
    {
        public DecimalData(string name, ParameterType typeId, decimal? max, decimal? min /*, decimal? def = null*/)
        {
            Name = name;
            TypeID = typeId;
            Max = max;
            Min = min;
            //Default = def;
        }
        public string Name;
        public ParameterType TypeID;
        public decimal? Max;
        public decimal? Min;
        //public decimal? Default;

        //public DecimalParameter Make(string name, ID<Parameter> id)
        //{
        //    return new DecimalParameter(name, id, TypeID, Max ?? decimal.MaxValue, Min ?? decimal.MinValue/*, Default ?? 0*/);
        //}

        public DecimalParameter.Definition Definition()
        {
            return new DecimalParameter.Definition() { Max = Max, Min = Min };
        }
    }
}

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
            TypeId = typeId;
            Max = max;
            Min = min;
            //Default = def;
        }
        public string Name { get; set; }
        public ParameterType TypeId { get; set; }
        public decimal? Max { get; set; }
        public decimal? Min { get; set; }
        //public decimal? Default;

        //public DecimalParameter Make(string name, ID<Parameter> id)
        //{
        //    return new DecimalParameter(name, id, TypeID, Max ?? decimal.MaxValue, Min ?? decimal.MinValue/*, Default ?? 0*/);
        //}

        public DecimalParameter.Definition Definition()
        {
            return new DecimalParameter.Definition(Max, Min);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public struct IntegerData
    {
        public IntegerData(string name, ParameterType typeId, int? max, int? min /*, int? def = null*/)
        {
            Name = name;
            TypeId = typeId;
            Max = max;
            Min = min;
            //Default = def;
        }
        public string Name { get; set; }
        public ParameterType TypeId { get; private set; }
        public int? Max { get; set; }
        public int? Min { get; set; }
        //public int? Default;

        //public IntegerParameter Make(string name, ID<Parameter> id)
        //{
        //    return new IntegerParameter(name, id, TypeID, Max ?? int.MaxValue, Min ?? int.MinValue);
        //}

        public IntegerParameter.Definition Definition()
        {
            return new IntegerParameter.Definition(Max, Min);
        }
    }
}

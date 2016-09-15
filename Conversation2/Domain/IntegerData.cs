using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public struct IntegerData
    {
        public IntegerData(string name, ParameterType typeId, int? max, int? min)
        {
            Name = name;
            TypeId = typeId;
            Max = max;
            Min = min;
        }
        public string Name { get; }
        public ParameterType TypeId { get; }
        public int? Max { get; }
        public int? Min { get; }

        public IntegerParameter.Definition Definition()
        {
            return new IntegerParameter.Definition(Min, Max);
        }
    }
}

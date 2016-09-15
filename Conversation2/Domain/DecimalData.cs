using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Conversation
{
    public struct DecimalData
    {
        public DecimalData(string name, ParameterType typeId, decimal? max, decimal? min )
        {
            Name = name;
            TypeId = typeId;
            Max = max;
            Min = min;
        }
        public string Name { get; }
        public ParameterType TypeId { get; }
        public decimal? Max { get; }
        public decimal? Min { get; }

        public DecimalParameter.Definition Definition()
        {
            return new DecimalParameter.Definition(Min, Max);
        }
    }
}

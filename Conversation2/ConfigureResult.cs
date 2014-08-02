using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;

namespace Conversation
{
    //public enum ConfigureResult
    //{
    //    Ok,
    //    Cancel,
    //    NotApplicable
    //}

    public enum ConfigureResultNotOk
    {
        Cancel,
        NotApplicable
    }

    public class ConfigureResult : Or<SimpleUndoPair, ConfigureResultNotOk>
    {
        public static ConfigureResultNotOk Cancel = ConfigureResultNotOk.Cancel;
        public static ConfigureResultNotOk NotApplicable = ConfigureResultNotOk.NotApplicable;

        public ConfigureResult(SimpleUndoPair pair)
            : base(pair)
        {
        }

        public ConfigureResult(ConfigureResultNotOk a)
            : base(a)
        {
        }


        public static implicit operator ConfigureResult(SimpleUndoPair pair)
        {
            return new ConfigureResult(pair);
        }

        public static implicit operator ConfigureResult(ConfigureResultNotOk a)
        {
            return new ConfigureResult(a);
        }
    }
}

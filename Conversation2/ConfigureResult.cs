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

    public class ConfigureResult : Either<SimpleUndoPair, ConfigureResultNotOk>
    {
        public const ConfigureResultNotOk Cancel = ConfigureResultNotOk.Cancel;
        public const ConfigureResultNotOk NotApplicable = ConfigureResultNotOk.NotApplicable;

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


    public class UpdateParameterData
    {
        /// <summary>
        /// Undo/Redo actions to perform to change the state of the parameter based on the editor selection
        /// </summary>
        public SimpleUndoPair? Actions = null;
        /// <summary>
        /// An audio file whose inclusion in the project should be updated
        /// </summary>
        public Audio? Audio = null;

        public static implicit operator UpdateParameterData(SimpleUndoPair? actions)
        {
            return new UpdateParameterData() { Actions = actions };
        }
    }


    public class ConfigureResult2 : Either<UpdateParameterData[], ConfigureResultNotOk>
    {
        public const ConfigureResultNotOk Cancel = ConfigureResultNotOk.Cancel;
        public const ConfigureResultNotOk NotApplicable = ConfigureResultNotOk.NotApplicable;

        public ConfigureResult2(UpdateParameterData[] updates)
            : base(updates)
        {
        }

        public ConfigureResult2(ConfigureResultNotOk a)
            : base(a)
        {
        }


        public static implicit operator ConfigureResult2(UpdateParameterData[] updates)
        {
            return new ConfigureResult2(updates);
        }

        public static implicit operator ConfigureResult2(ConfigureResultNotOk a)
        {
            return new ConfigureResult2(a);
        }
    }
}

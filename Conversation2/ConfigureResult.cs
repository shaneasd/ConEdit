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
        /// <summary>
        /// The user instigated an edit but subsequently cancelled that operation prior to its taking effect
        /// </summary>
        Cancel,

        /// <summary>
        /// The user instigated an edit but no editing of the target is possible irrespective of user action
        /// </summary>
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


    /// <summary>
    /// Represents an update action (and counteraction) of a parameters's data
    /// </summary>
    public class UpdateParameterData
    {
        /// <summary>
        /// Undo/Redo actions to perform to change the state of the parameter based on the editor selection
        /// </summary>
        public SimpleUndoPair? Actions { get; private set; } = null;
        /// <summary>
        /// An audio file whose inclusion in the project should be updated
        /// </summary>
        public Audio? Audio { get; set; } = null;

        public static implicit operator UpdateParameterData(SimpleUndoPair? actions)
        {
            return new UpdateParameterData() { Actions = actions };
        }
    }

    /// <summary>
    /// Represents the result of a user's attempt to modify a node
    /// If a UpdateParameterData[], it is the actions required to modify those parameters on the node that the user has specified to their new, user specified values
    /// If a ConfigureResultNotOk indicates that no changes are required for the reason indicated by the specified value of the ConfigureResultNotOk
    /// </summary>
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

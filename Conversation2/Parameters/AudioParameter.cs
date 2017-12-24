using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.IO;

namespace Conversation
{
    public sealed class Failed
    {
        private Failed() { }
        public static Failed Instance => null;
    }

    public struct Audio
    {
        public Audio(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            Value = value;
        }

        public string Value { get; }

        public static Either<Failed, Audio> Deserialize(string value)
        {
            if (string.IsNullOrEmpty(value))
                return Failed.Instance;
            return new Audio(value);
        }

        public string Serialize()
        {
            return Value.ToString();
        }

        public string DisplayValue()
        {
            int lastSlash = Value.LastIndexOf('\\');
            if (lastSlash == -1)
                return Value;
            else if (lastSlash == Value.Length - 1)
                return "";
            else
                return Value.Substring(lastSlash + 1);
        }
    }

    public class AudioParameter : Parameter<Audio>, IAudioParameter
    {
        public static ParameterType ParameterType { get; } = ParameterType.Parse("05b29166-31c5-449f-bb91-d63a603183db");

        public AudioParameter(string name, Id<Parameter> id)
            : base(name, id, ParameterType, null, Tuple.Create(default(Audio), true)) //Audio parameters may require information from the conversation that contains them and as such do not support a constant default value
        {
        }

        protected override Tuple<Audio, bool> DeserializeValueInner(string value)
        {
            var deserialized = Audio.Deserialize(value);
            return deserialized.Transformed<Tuple<Audio, bool>>(
                                            f => { return Tuple.Create(default(Audio), true); },
                                            a => { return Tuple.Create(a, false); });
        }

        protected override string InnerValueAsString()
        {
            return Value.Serialize();
        }

        public override string DisplayValue(Func<Id<LocalizedStringType>, Id<LocalizedText>, string> localize)
        {
            return Value.DisplayValue();
        }

        protected override bool ValueValid(Audio value)
        {
            return value.Value != null;
        }
    }
}

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
        public static Failed Instance = null;
    }

    public struct Audio
    {
        public Audio(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            Value = value;
        }

        public readonly string Value;

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
        public AudioParameter(string name, Id<Parameter> id, ParameterType typeId)
            : base(name, id, typeId, null) //Audio parameters may require information from the conversation that contains them and as such do not support a constant default value
        {
        }

        protected override bool DeserialiseValue(string value)
        {
            var deserialized = Audio.Deserialize(value);
            return deserialized.Transformed(f => { return false; }, a => { m_value = a; return true; });
        }

        protected override string InnerValueAsString()
        {
            return m_value.Serialize();
        }

        public override string DisplayValue(Func<Id<LocalizedText>, string> localize)
        {
            return m_value.DisplayValue();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilities;
using System.IO;

namespace Conversation
{
    public class Failed
    {
        private Failed() { }
        public static Failed Instance = null;
    }

    public struct Audio
    {
        public Audio(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException("value");
            Value = value;
        }

        public readonly string Value;

        public static Or<Failed, Audio> Deserialize(string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return Failed.Instance;
                return new Audio(value);
            }
            catch
            {
                return Failed.Instance;
            }
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
        public AudioParameter(string name, ID<Parameter> id, ParameterType typeId, string defaultValue)
            : base(name, id, typeId, defaultValue)
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

        public override string DisplayValue(Func<ID<LocalizedText>, string> localize)
        {
            return m_value.DisplayValue();
        }

        protected override void DecorruptFromNull()
        {
            //TODO: Something here?
        }
    }
}

﻿using System;
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
            Value = value;
        }

        public readonly string Value;

        public static Or<Failed, Audio> Deserialize(string value)
        {
            try
            {
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
            return Value.ToString();
        }
    }

    public class AudioParameter : Parameter<Audio>, IAudioParameter
    {
        public AudioParameter(string name, ID<Parameter> id, ID<ParameterType> typeId, string defaultValue) : base(name, id, typeId, defaultValue) { }

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
    }
}

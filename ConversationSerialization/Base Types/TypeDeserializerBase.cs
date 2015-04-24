using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace RuntimeConversation
{
    public abstract class TypeDeserializerBase
    {
        public static void Deserialize(out int a, string value)
        {
            a = int.Parse(value, CultureInfo.InvariantCulture);
        }

        public static void Deserialize(out decimal a, string value)
        {
            a = decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        public static void Deserialize(out string a, string value)
        {
            a = value;
        }

        public static void Deserialize(out bool a, string value)
        {
            a = bool.Parse(value);
        }

        public static void Deserialize(out LocalizedString a, string value)
        {
            a = new LocalizedString(value);
        }

        public static void Deserialize(out Audio a, string value)
        {
            a = new Audio(value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuntimeConversation
{
    public class LocalizedString
    {
        public readonly Guid Id;
        public LocalizedString(string value)
        {
            Id = Guid.Parse(value);
        }

        public string Localized(Func<Guid, string> localizer)
        {
            return localizer(Id);
        }
    }

    public class Audio
    {
        public readonly string Path;
        public Audio(string value)
        {
            Path = value;
        }
    }
}

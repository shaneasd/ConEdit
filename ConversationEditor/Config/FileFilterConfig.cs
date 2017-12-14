using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConversationEditor
{
    public class FileFilterConfig : IConfigParameter
    {
        public ConfigParameterBool Folders { get; } = new ConfigParameterBool("Folders", true);
        public ConfigParameterBool Domains { get; } = new ConfigParameterBool("Domains", true);
        public ConfigParameterBool Audio { get; } = new ConfigParameterBool("Audio", true);
        public ConfigParameterBool Conversations { get; } = new ConfigParameterBool("Conversations", true);
        public ConfigParameterBool Localizations { get; } = new ConfigParameterBool("Localizations", true);

        public FileFilterConfig()
        {
            Folders.ValueChanged += () => ValueChanged();
            Domains.ValueChanged += () => ValueChanged();
            Audio.ValueChanged += () => ValueChanged();
            Conversations.ValueChanged += () => ValueChanged();
            Localizations.ValueChanged += () => ValueChanged();
        }

        public void Load(System.Xml.Linq.XElement root)
        {
            Folders.Load(root);
            Domains.Load(root);
            Audio.Load(root);
            Conversations.Load(root);
            Localizations.Load(root);
        }

        public void Write(System.Xml.Linq.XElement root)
        {
            Folders.Write(root);
            Domains.Write(root);
            Audio.Write(root);
            Conversations.Write(root);
            Localizations.Write(root);
        }

        public event Action ValueChanged;
    }
}

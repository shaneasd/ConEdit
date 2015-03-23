using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConversationEditor;
using System.IO;
using System.Windows.Forms;
using Utilities;

namespace Clandestine.Audio
{
    class AlternateAudioProviderCustomization : IAudioProviderCustomization
    {
        static Dictionary<Tuple<string,string>, string> mapping;
        static Tuple<string, string>[] keys;
        static AlternateAudioProviderCustomization()
        {
            try
            {
                const string SUBTITLES = @"c:\subtitles.txt";
                const string FILES = @"c:\filepaths.txt";
                const string CONVERSATIONS = @"c:\conversationfiles.txt";
                var subtitles = File.ReadAllLines(SUBTITLES, Encoding.UTF8);
                var files = File.ReadAllLines(FILES);
                var conversations = File.ReadAllLines(CONVERSATIONS);

                if (subtitles.Length != files.Length || subtitles.Length != conversations.Length)
                {
                    MessageBox.Show("subtitles.txt and filepaths.txt and conversationfiles.txt have inconsistent line count");
                }

                keys = new Tuple<string, string>[subtitles.Length];
                for (int i = 0; i < keys.Length; i++)
                    keys[i] = Tuple.Create(subtitles[i], conversations[i]);

                if (keys.Distinct().Count() != keys.Length)
                {
                    var dupes = keys.GroupBy(s => s).Where(g => g.Count() > 1).Select(g => "Count: " + g.Count() + "  " + g.Key.Item1 + "  :  " + g.Key.Item2);
                    MessageBox.Show("subtitles.txt contains duplicates:\n\n" + string.Join("\n\n", dupes));
                }
                if (files.Distinct().Count() != files.Length)
                    MessageBox.Show("filepaths.txt contains duplicates");
                mapping = new Dictionary<Tuple<string, string>, string>();
                for (int i = 0; i < subtitles.Length; i++)
                {
                    mapping[keys[i]] = files[i];
                }
            }
            catch //should really limit to IO exceptions etc as I don't want to catch OutOfMemory etc but I'm lazy and this code shouldn't be around forever
            {
            }
        }

        public Conversation.Audio Generate(AudioGenerationParameters parameters)
        {
                var file = parameters.File.File.File.Name;
                var subtitle = parameters.Parameters.Single(p => p.Id == CsvData.SPEECH_SUBTITLE || p.Id == CsvData.OPTION_SUBTITLES).DisplayValue(parameters.Localize);
                var key = Tuple.Create(subtitle, file);
            try
            {
                var value = mapping[key];
                value += ".ogg";
                return new Conversation.Audio(value);
            }
            catch
            {
                MessageBox.Show(key.ToString());
            }
            return new Conversation.Audio("shane");
        }

        public string Name
        {
            get { return "Shane's fuck up recovery"; }
        }
    }
}

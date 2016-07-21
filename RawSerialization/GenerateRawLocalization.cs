using ConversationEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RawSerialization
{
    public class GenerateRawLocalization : ILocalizationContextMenuItem
    {
        public string Name { get { return "Generate Binary Localization"; } }

        private Stream DetermineFileStream()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.DefaultExt = "binloc";
                sfd.AddExtension = true;
                //TODO: Add file pattern
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    return sfd.OpenFile();
                }
                else
                {
                    return null;
                }
            }
        }

        public void Execute(ILocalizationFile localization)
        {
            using (Stream stream = DetermineFileStream())
            {
                Write(localization, stream);
            }
        }

        public static void Write(ILocalizationFile localization, Stream stream)
        {
            //File format is
            // HEADER
            // LOOKUPS
            // PAYLOAD
            //
            // HEADER consists of
            // Number of items (N)  (unsigned 4 bytes)
            //
            // LOOKUPS consists of
            // N occurences of LOOKUP
            //
            // LOOKUP consists of
            // GUID (16 bytes)
            // Start index of string in payload (unsigned 4 bytes)
            // Length of string (unsigned 4 bytes)
            //
            // PAYLOAD consists of
            // N occurences of STRING
            //
            // STRING is a utf-8 representation of a string element with no terminator

            //When allocating initial memory we'll guess that each string is on average 100 bytes including the header
            const uint BYTES_PER_STRING_GUESS = 100;
            const uint BYTES_PER_LOOKUP = 16 + 4 + 4;
            const uint BYTES_HEADER = 4;

            var data = localization.ExistingLocalizations.ToDictionary(x => x, x => localization.Localize(x));

            stream.SetLength(data.Count * BYTES_PER_STRING_GUESS); //Header size is negligible
            uint[] starts = new uint[data.Count];
            uint[] lengths = new uint[data.Count];


            //PAYLOAD
            {
                stream.Position = BYTES_HEADER + data.Count * BYTES_PER_LOOKUP;
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                {
                    int index = 0;
                    foreach (string s in data.Values)
                    {
                        starts[index] = (uint)stream.Position;
                        writer.Write(s);
                        writer.Flush(); //We're using the stream position to figure out how long the string was so it needs to be up to date
                        lengths[index] = (uint)stream.Position - starts[index];
                        index++;
                    }
                }
            }

            //HEADER
            {
                stream.Position = 0;
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    writer.Write((uint)data.Count);
                }
            }

            //LOOKUPS
            {
                stream.Position = BYTES_HEADER;
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    int index = 0;
                    foreach (var key in data.Keys)
                    {
                        var guidBytes = key.Guid.ToByteArray();
                        stream.Write(guidBytes, 0, guidBytes.Length);
                        writer.Write(starts[index]);
                        writer.Write(lengths[index]);
                        index++;
                    }
                }
            }

            //Chop off the end
            stream.SetLength(starts.Last() + lengths.Last());
        }
    }
}

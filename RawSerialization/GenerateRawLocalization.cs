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
        public string Name => "Generate Binary Localization";

        private static Stream DetermineFileStream()
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
            // VERSION
            // HEADER
            // PAYLOAD
            //
            // VERSION consists of
            // version identifier (unsigned 4 bytes)
            //
            // HEADER consists of
            // Number of items (N)  (unsigned 4 bytes)
            //
            // PAYLOAD consists of
            // N occurences of ITEM
            //
            // ITEM consists of
            // GUID (16 bytes)
            // STRING
            //
            // STRING is string represented however BinaryWriter/BinaryReader represent it

            //When allocating initial memory we'll guess that each string is on average 100 bytes including the header
            const uint BYTES_PER_STRING_GUESS = 100;

            var data = localization.ExistingLocalizations.ToDictionary(x => x, x => localization.Localize(x));

            stream.SetLength(data.Count * BYTES_PER_STRING_GUESS); //Header size is negligible

            uint VERSION = 0;

            using (var binaryWriter = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                binaryWriter.Write(VERSION);
                binaryWriter.Write((uint)data.Count);
                foreach (var kvp in data)
                {
                    var guidBytes = kvp.Key.Guid.ToByteArray();
                    binaryWriter.Write(guidBytes, 0, guidBytes.Length);
                    binaryWriter.Write(kvp.Value);
                }
            }

            stream.SetLength(stream.Position);
        }
    }
}

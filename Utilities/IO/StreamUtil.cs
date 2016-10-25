using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class StreamUtil
    {
        public static MemoryStream Copy(Stream stream)
        {
            MemoryStream m = null;
            try
            {
                m = new MemoryStream((int)stream.Length);
                stream.CopyTo(m);
                m.Position = 0;
                var result = m;
                m = null;
                return result;
            }
            finally
            {
                if (m != null)
                    m.Dispose();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Dominos.Services.Common.Tools.Redis.Extensions
{
    public static class Compression
    {

        public static string GZipStringCompress(this string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s);
            using (var memoryStreamIn = new MemoryStream(bytes))
            using (var memoryStreamOut = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(memoryStreamOut, CompressionMode.Compress))
                {
                    memoryStreamIn.CopyTo(gZipStream);
                }

                return Convert.ToBase64String(memoryStreamOut.ToArray());
            }
        }

        public static string GZipStringDecompress(this string s)
        {
            var bytes = Convert.FromBase64String(s);
            using (var memoryStreamIn = new MemoryStream(bytes))
            using (var memoryStreamOut = new MemoryStream())
            {
                using (var gZipStream = new GZipStream(memoryStreamIn, CompressionMode.Decompress))
                {
                    gZipStream.CopyTo(memoryStreamOut);
                }

                return Encoding.Unicode.GetString(memoryStreamOut.ToArray());
            }
        }
    }
}

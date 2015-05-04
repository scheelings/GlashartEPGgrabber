using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlashartLibrary.Helpers
{
    public sealed class CompressionHelper
    {
        /// <summary>
        /// Decompresses the specified compressed file.
        /// </summary>
        /// <param name="compressedFile">The compressed file.</param>
        /// <param name="uncompressedFile">The uncompressed file.</param>
        public static void Decompress(string compressedFile, string uncompressedFile)
        {
            byte[] file = File.ReadAllBytes(compressedFile);
            byte[] decompressed = Decompress(file);
            File.WriteAllBytes(uncompressedFile, decompressed);
        }

        /// <summary>
        /// Decompresses the specified compressed file.
        /// </summary>
        /// <param name="compressedFile">The compressed file.</param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] compressedFile)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(compressedFile), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];
                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    return memory.ToArray();
                }
            }
        }
    }
}

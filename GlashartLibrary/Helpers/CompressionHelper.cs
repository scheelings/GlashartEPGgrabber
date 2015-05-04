using System;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;

namespace GlashartLibrary.Helpers
{
    public sealed class CompressionHelper
    {
        /// <summary>
        /// Decompresses the specified compressed file.
        /// </summary>
        /// <param name="compressedFile">The compressed file.</param>
        /// <param name="uncompressedFile">The uncompressed file.</param>
        //public static void Decompress(string compressedFile, string uncompressedFile)
        //{
        //    byte[] file = File.ReadAllBytes(compressedFile);
        //    byte[] decompressed = Decompress(file);
        //    File.WriteAllBytes(uncompressedFile, decompressed);
        //}

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

        /// <summary>
        /// Decompresses the specified compressed file using SharpZipLib
        /// MkBundle doesn't support GZipStream
        /// </summary>
        /// <param name="compressedFile">The compressed file.</param>
        /// <param name="uncompressedFile">The uncompressed file.</param>
        public static bool Decompress(string gzipFileName, string filename)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(gzipFileName))
                {
                    throw new Exception("Null gzipFieName");
                }

                // Use a 4K buffer. Any larger is a waste.    
                var dataBuffer = new byte[4096];

                using (Stream fs = new FileStream(gzipFileName, FileMode.Open, FileAccess.Read))
                using (var gzipStream = new GZipInputStream(fs))
                {
                    // Change this to your needs
                    //var fnOut = Path.Combine(targetDir, Path.GetFileNameWithoutExtension(gzipFileName));
                    using (var fsOut = File.Create(filename))
                    {
                        StreamUtils.Copy(gzipStream, fsOut, dataBuffer);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ApplicationLog.WriteError(ex, "Failed to extract {0}", gzipFileName);
                return false;
            }
        }
    }
}

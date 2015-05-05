using System;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.GZip;
using log4net;

namespace GlashartLibrary.Helpers
{
    public sealed class CompressionHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CompressionHelper));

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
                Logger.Error(ex, "Failed to extract {0}", gzipFileName);
                return false;
            }
        }
    }
}

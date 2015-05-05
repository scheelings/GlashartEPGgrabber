using System;
using System.Net;
using System.IO;
using System.Text;
using log4net;

namespace GlashartLibrary.Helpers
{
    public class HttpDownloader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(HttpDownloader));

        private const int DownloadBlockSize = 4096;

        /// <summary>
        /// Get/set the user agent definition to use for connections
        /// </summary>
        public static string WebrequestUserAgent = ""; //"Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.0)";
        /// <summary>
        /// Get/set the HTTP Accept definition to use for connections
        /// </summary>
        public static string WebrequestAccept = "*/*";
        /// <summary>
        /// Get/set the HTTP method (get or post) to use for connections
        /// </summary>
        public static string WebrequestMethod = "GET";
        /// <summary>
        /// Get/set the referer to use for connections
        /// </summary>
        public static string WebrequestReferer = ""; //"http://www.microsoft.com";
        /// <summary>
        /// Get/set the webproxy to use for connections
        /// </summary>
        public static WebProxy WebrequestProxy = null;

        /// <summary>
        /// Downloads the text file using the HTTP protocol
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="localFile">The local file to download to</param>
        public static void DownloadTextFile(string url, string localFile)
        {
            string content = DownloadTextFile(url);
            StreamWriter writer = File.CreateText(localFile);
            writer.Write(content);
            writer.Close();
        }

        /// <summary>
        /// Downloads the text file using the HTTP protocol and returns the content
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>File content</returns>
        public static string DownloadTextFile(string url, Encoding encoding)
        {
            byte[] file = DownloadBinaryFile(url);
            return encoding.GetString(file);
        }
        /// <summary>
        /// Downloads the text file using the HTTP protocol and returns the content
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>File content</returns>
        public static string DownloadTextFile(string url)
        {
            return DownloadTextFile(url, Encoding.Default);
        }

        /// <summary>
        /// Downloads the binary file using the HTTP protocol
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="localFile">The local file to download to</param>
        public static void DownloadBinaryFile(string url, string localFile)
        {
            EnsureDirectoryExist(localFile);
            byte[] content = DownloadBinaryFile(url);
            FileStream file = File.OpenWrite(localFile);
            file.Write(content, 0, content.Length);
            file.Close();
        }

        private static void EnsureDirectoryExist(string localFile)
        {
            Logger.DebugFormat("Test folder for file {0}", localFile);
            var file = new FileInfo(localFile);
            var dir = file.Directory;
            if (dir != null && !dir.Exists)
            {
                Logger.InfoFormat("Folder {0} doesn't exist, create it", dir.FullName);
                dir.Create();
            }
        }

        /// <summary>
        /// Downloads the binary file using the HTTP protocol and returns the content
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>Binary content</returns>
        public static byte[] DownloadBinaryFile(string url)
        {
            byte[] result = null;
            byte[] buffer = new byte[DownloadBlockSize];

            // Creates an HttpWebRequest with the specified URL. 
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.UserAgent = WebrequestUserAgent;
            req.Accept = WebrequestAccept;
            req.Method = WebrequestMethod;
            req.Referer = WebrequestReferer;
            if (WebrequestProxy != null)
                req.Proxy = WebrequestProxy;

            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            {
                using (Stream stream = resp.GetResponseStream())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = stream.Read(buffer, 0, buffer.Length);
                            memoryStream.Write(buffer, 0, count);

                        } while (count != 0);

                        result = memoryStream.ToArray();
                    }
                }
            }
            return result;
        }
    }
}

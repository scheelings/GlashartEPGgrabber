using System;
using System.Net;
using log4net;

namespace GlashartLibrary.IO
{
    public class HttpWebDownloader : IWebDownloader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CachedWebDownloader));

        public byte[] DownloadBinary(string url)
        {
            try
            {
                var webClient = new WebClient();
                return webClient.DownloadData(url);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to download the data from {0}", url);
                return null;
            }
        }

        public string DownloadString(string url)
        {
            try
            {
                var webClient = new WebClient();
                return webClient.DownloadString(url);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to download the string from {0}", url);
                return null;
            }
        }
    }
}
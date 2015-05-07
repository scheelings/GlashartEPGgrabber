using System.IO;
using log4net;

namespace GlashartLibrary.IO
{
    public class FileDownloader : IFileDownloader
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(FileDownloader));
        private readonly IWebDownloader _webDownloader;

        public FileDownloader(IWebDownloader webDownloader)
        {
            _webDownloader = webDownloader;
        }

        /// <summary>
        /// Downloads the binary file using the HTTP downloader
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="localFile">The local file to download to</param>
        public void DownloadBinaryFile(string url, string localFile)
        {
            EnsureDirectoryExist(localFile);
            var content = _webDownloader.DownloadBinary(url);
            var file = File.OpenWrite(localFile);
            file.Write(content, 0, content.Length);
            file.Close();
        }

        private void EnsureDirectoryExist(string localFile)
        {
            Logger.DebugFormat("Test folder for file {0}", localFile);
            var file = new FileInfo(localFile);
            var dir = file.Directory;
            if (dir == null || dir.Exists) return;
            Logger.InfoFormat("Folder {0} doesn't exist, create it", dir.FullName);
            dir.Create();
        }
    }
}
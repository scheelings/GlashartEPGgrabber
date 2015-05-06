namespace GlashartLibrary.IO
{
    public interface IHttpDownloader
    {
        byte[] DownloadBinary(string url);
        string DownloadString(string url);
    }
}
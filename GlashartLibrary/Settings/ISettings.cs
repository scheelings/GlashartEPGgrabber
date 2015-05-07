using System.Collections.Generic;

namespace GlashartLibrary.Settings
{
    public interface ISettings
    {
        string TvMenuURL { get; }
        string EpgURL { get; }
        string TvMenuFolder { get; }
        bool IgmpToUdp { get; }
        List<string> M3U_ChannelLocationImportance { get;}
        int EpgNumberOfDays { get; }
        string EpgFolder { get; }
        int EpgArchiving { get; }
        string ChannelsListFile { get; }
        string XmlTvFile { get; }
        string M3UFile { get; }
        string DownloadedM3UFile { get; }
        string IconFolder { get; }
        string DataFolder { get; }
    }
}
using System.Collections.Generic;

namespace GlashartLibrary.Settings
{
    public interface ISettings
    {
        string TvMenuURL { get; }
        string EpgURL { get; }
        string TvMenuFolder { get; }
        string M3UfileName { get; }
        bool IgmpToUdp { get; }
        List<string> M3U_ChannelLocationImportance { get;}
        string ChannelsListFile { get; }
        int EpgNumberOfDays { get; }
        string EpgFolder { get; }
        int EpgArchiving { get; }
        string XmlTvFileName { get; }
        string DownloadedM3UFileName { get; }
        string IconFolder { get; }
    }
}
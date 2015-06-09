using System.Collections.Generic;

namespace GlashartLibrary.Settings
{
    public interface ISettings
    {
        string TvMenuURL { get; }
        string EpgURL { get; }
        string ImagesURL { get; }
        bool IgmpToUdp { get; }
        List<string> M3U_ChannelLocationImportance { get;}
        int EpgNumberOfDays { get; }
        int EpgArchiving { get; }
        bool UseDisplayNameForIcon { get; }

        string TvMenuFolder { get; }
        string EpgFolder { get; }
        string IconFolder { get; }
        string DataFolder { get; }
        string TvheadendFolder { get; }

        string ChannelsListFile { get; }
        string M3UFile { get; }
        string XmlTvFile { get; }
        string DownloadedM3UFile { get; }
        string TvhGenreTranslationsFile { get; }
        string TvheadendNetworkInterface { get; }
        string TvheadendNetworkName { get; }
    }
}
using System.Collections.Generic;
using System.Linq;
using GlashartLibrary.Properties;

namespace GlashartLibrary
{
    public class ConfigSettings : ISettings
    {
        public string TvMenuURL { get { return Settings.Default.TvMenuURL; } }
        public string EpgURL { get { return Settings.Default.EpgURL; } }
        public string TvMenuFolder { get { return Settings.Default.TvMenuFolder; } }
        public string M3UfileName { get { return Settings.Default.M3UfileName; } }
        public bool IgmpToUdp { get { return Settings.Default.IgmpToUdp; } }
        public List<string> M3U_ChannelLocationImportance { get { return Settings.Default.M3U_ChannelLocationImportance.Cast<string>().ToList(); } }
        public string ChannelsListFile { get { return Settings.Default.ChannelsListFile; } }
        public int EpgNumberOfDays { get { return Settings.Default.EpgNumberOfDays; } }
        public string EpgFolder { get { return Settings.Default.EpgFolder; } }
        public int EpgArchiving { get { return Settings.Default.EpgArchiving; } }
        public string XmlTvFileName { get { return Settings.Default.XmlTvFileName; } }
        public string DownloadedM3UFileName { get { return Settings.Default.DownloadedM3UFileName; } }
    }
}
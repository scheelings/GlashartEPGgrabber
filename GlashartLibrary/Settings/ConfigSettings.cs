using System.Collections.Generic;
using System.Linq;

namespace GlashartLibrary.Settings
{
    public class ConfigSettings : ISettings
    {
        public string TvMenuURL { get { return Properties.Settings.Default.TvMenuURL; } }
        public string EpgURL { get { return Properties.Settings.Default.EpgURL; } }
        public string TvMenuFolder { get { return Properties.Settings.Default.TvMenuFolder; } }
        public string M3UfileName { get { return Properties.Settings.Default.M3UfileName; } }
        public bool IgmpToUdp { get { return Properties.Settings.Default.IgmpToUdp; } }
        public List<string> M3U_ChannelLocationImportance { get { return Properties.Settings.Default.M3U_ChannelLocationImportance.Cast<string>().ToList(); } }
        public string ChannelsListFile { get { return Properties.Settings.Default.ChannelsListFile; } }
        public int EpgNumberOfDays { get { return Properties.Settings.Default.EpgNumberOfDays; } }
        public string EpgFolder { get { return Properties.Settings.Default.EpgFolder; } }
        public int EpgArchiving { get { return Properties.Settings.Default.EpgArchiving; } }
        public string XmlTvFileName { get { return Properties.Settings.Default.XmlTvFileName; } }
        public string DownloadedM3UFileName { get { return Properties.Settings.Default.DownloadedM3UFileName; } }
        public string IconFolder { get { return Properties.Settings.Default.IconFolder; } }
    }
}
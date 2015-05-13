using System.Collections.Generic;
using System.Linq;

namespace GlashartLibrary.Settings
{
    public class ConfigSettings : ISettings
    {
        public string TvMenuURL { get { return Properties.Settings.Default.TvMenuURL; } }
        public string EpgURL { get { return Properties.Settings.Default.EpgURL; } }
        public string ImagesURL { get { return Properties.Settings.Default.ImagesURL; } }
        public bool UseDisplayNameForIcon { get { return Properties.Settings.Default.UseDisplayNameForIcon; } }
        public string TvMenuFolder { get { return Properties.Settings.Default.TvMenuFolder; } }
        public bool IgmpToUdp { get { return Properties.Settings.Default.IgmpToUdp; } }
        public List<string> M3U_ChannelLocationImportance { get { return Properties.Settings.Default.M3U_ChannelLocationImportance.Cast<string>().ToList(); } }
        public string ChannelsListFile { get { return Properties.Settings.Default.ChannelsListFileName; } }
        public string XmlTvFile { get { return Properties.Settings.Default.XmlTvFileName; } }
        public string M3UFile { get { return Properties.Settings.Default.M3UfileName; } }
        public string DownloadedM3UFile { get { return Properties.Settings.Default.DownloadedM3UFileName; } }
        public string TvhGenreTranslationsFile { get { return Properties.Settings.Default.TvhGenreTranslationsFile; } }
        public int EpgNumberOfDays { get { return Properties.Settings.Default.EpgNumberOfDays; } }
        public string EpgFolder { get { return Properties.Settings.Default.EpgFolder; } }
        public int EpgArchiving { get { return Properties.Settings.Default.EpgArchiving; } }
        public string IconFolder { get { return Properties.Settings.Default.IconFolder; } }
        public string DataFolder { get { return Properties.Settings.Default.DataFolder; } }
    }
}
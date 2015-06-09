using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using log4net;

namespace GlashartLibrary.Settings
{
    public class IniSettings : ISettings
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IniSettings));

        public string TvMenuURL { get; private set; }
        public string EpgURL { get; private set; }
        public string ImagesURL { get; private set; }
        
        public bool IgmpToUdp { get; private set; }
        public List<string> M3U_ChannelLocationImportance { get; private set; }

        public int EpgNumberOfDays { get; private set; }

        public string DataFolder { get; private set; }
        public bool UseDisplayNameForIcon { get; private set; }
        public string TvMenuFolder { get { return Path.Combine(DataFolder, "TvMenu"); } }
        public string EpgFolder { get { return Path.Combine(DataFolder, "Epg"); } }
        public string IconFolder { get { return Path.Combine(DataFolder, "Icons"); } }
        public string TvheadendFolder { get { return Path.Combine(DataFolder, "TVheadend"); } }

        public string XmlTvFile { get { return Path.Combine(DataFolder, XmlTvFileName); } }
        private string XmlTvFileName { get; set; }

        public string DownloadedM3UFile { get { return Path.Combine(TvMenuFolder, DownloadedM3UFileName); } }
        private string DownloadedM3UFileName { get; set; }

        public string TvhGenreTranslationsFile { get { return Path.Combine(DataFolder, TvhGenreTranslationsFileName); } }
        private string TvhGenreTranslationsFileName { get; set; }

        public string TvheadendNetworkInterface { get; private set; }
        public string TvheadendNetworkName { get; private set; }

        public string ChannelsListFile { get { return Path.Combine(TvMenuFolder, ChannelsListFileName); } }
        private string ChannelsListFileName { get; set; }

        public string M3UFile { get { return Path.Combine(TvMenuFolder, M3UFileName); } }
        private string M3UFileName { get; set; }

        public int EpgArchiving { get; private set; }
        public string LogLevel { get; private set; }

        public void Load()
        {
            Logger.Info("Read GlashartEPGgrabber.ini");
            M3U_ChannelLocationImportance = new List<string>();
            try
            {
                using (var reader = new StreamReader("GlashartEPGgrabber.ini"))
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        Logger.DebugFormat("Process line {0}", line);
                        ReadConfigItem(line);
                    }

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to load the configuration", ex);
            }
        }

        private void ReadConfigItem(string line)
        {
            var keyvalue = line.Split('=');
            if (keyvalue.Length < 2)
            {
                Logger.WarnFormat("Failed to read configuration line: {0}", line);
                return;
            }
            if (keyvalue[0].Equals("M3U_ChannelLocationImportance"))
            {
                M3U_ChannelLocationImportance.Add(keyvalue[1]);
            }
            else SetValue(keyvalue[0], keyvalue[1]);
        }

        private void SetValue(string key, string value)
        {
            var propertyInfo = GetType().GetProperty(key, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            if (propertyInfo == null)
            {
                Logger.WarnFormat("Unknown configuration key: {0}", key);
                return;
            }
            try
            {
                Logger.DebugFormat("Read configuration item {0} with value {1}", key, value);
                propertyInfo.SetValue(this, Convert.ChangeType(value, propertyInfo.PropertyType), null);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to read {0} into {1} as {2}", key, value, propertyInfo.PropertyType);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;

namespace GlashartLibrary.Settings
{
    public class IniSettings : ISettings
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IniSettings));

        public string TvMenuURL { get; private set; }
        public string EpgURL { get; private set; }
        
        public bool IgmpToUdp { get; private set; }
        public List<string> M3U_ChannelLocationImportance { get; private set; }

        public int EpgNumberOfDays { get; private set; }

        public string DataFolder { get; private set; }
        public string TvMenuFolder { get { return Path.Combine(DataFolder, "TvMenu"); } }
        public string EpgFolder { get { return Path.Combine(DataFolder, "Epg"); } }
        public string IconFolder { get { return Path.Combine(DataFolder, "Icons"); } }

        public string XmlTvFile { get { return Path.Combine(DataFolder, XmlTvFileName); } }
        private string XmlTvFileName { get; set; }

        public string DownloadedM3UFile { get { return Path.Combine(TvMenuFolder, DownloadedM3UFileName); } }
        private string DownloadedM3UFileName { get; set; }

        public string TvhGenreTranslationsFile { get { return Path.Combine(DataFolder, TvhGenreTranslationsFileName); } }
        private string TvhGenreTranslationsFileName { get; set; }

        public string ChannelsListFile { get { return Path.Combine(TvMenuFolder, ChannelsListFileName); } }
        private string ChannelsListFileName { get; set; }

        public string M3UFile { get { return Path.Combine(TvMenuFolder, M3UFileName); } }
        private string M3UFileName { get; set; }

        public int EpgArchiving { get; private set; }
        public string LogLevel { get; private set; }

        public void Load()
        {
            Logger.Info("Read GlashartEPGgrabber.ini");
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
            SetValue(keyvalue[0], keyvalue[1]);
        }

        private void SetValue(string key, string value)
        {
            switch (key)
            {
                case "TvMenuURL":
                    TvMenuURL = value;
                    break;
                case "EpgURL":
                    EpgURL = value;
                    break;
                case "M3UFileName":
                    M3UFileName = value;
                    break;
                case "IgmpToUdp":
                    IgmpToUdp = bool.Parse(value);
                    break;
                case "M3U_ChannelLocationImportance":
                    M3U_ChannelLocationImportance = value.Split(';').ToList();
                    break;
                case "ChannelsListFileName":
                    ChannelsListFileName = value;
                    break;
                case "EpgNumberOfDays":
                    EpgNumberOfDays = int.Parse(value);
                    break;
                case "DataFolder":
                    DataFolder = value;
                    break;
                case "EpgArchiving":
                    EpgArchiving = int.Parse(value);
                    break;
                case "XmlTvFileName":
                    XmlTvFileName = value;
                    break;
                case "DownloadedM3UFileName":
                    DownloadedM3UFileName = value;
                    break;
                case "LogLevel":
                    LogLevel = value;
                    break;
                case "TvhGenreTranslationsFileName":
                    TvhGenreTranslationsFileName = value;
                    break;
                default:
                    Logger.WarnFormat("Unknown configuration key: {0}", key);
                    return;
            }
            Logger.DebugFormat("Read configuration item {0} with value {1}", key, value);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;

namespace GlashartLibrary
{
    public class IniSettings : ISettings
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IniSettings));

        public string TvMenuURL { get; private set; }
        public string EpgURL { get; private set; }
        public string TvMenuFolder { get; set; }
        public string M3UfileName { get; set; }
        public bool IgmpToUdp { get; set; }
        public List<string> M3U_ChannelLocationImportance { get; set; }
        public string ChannelsListFile { get; set; }
        public int EpgNumberOfDays { get; set; }
        public string EpgFolder { get; set; }
        public int EpgArchiving { get; set; }
        public string XmlTvFileName { get; set; }
        public string DownloadedM3UFileName { get; set; }
        public string LogLevel { get; set; }

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
                case "TvMenuFolder":
                    TvMenuFolder = value;
                    break;
                case "M3UfileName":
                    M3UfileName = value;
                    break;
                case "IgmpToUdp":
                    IgmpToUdp = bool.Parse(value);
                    break;
                case "M3U_ChannelLocationImportance":
                    M3U_ChannelLocationImportance = value.Split(';').ToList();
                    break;
                case "ChannelsListFile":
                    ChannelsListFile = value;
                    break;
                case "EpgNumberOfDays":
                    EpgNumberOfDays = int.Parse(value);
                    break;
                case "EpgFolder":
                    EpgFolder = value;
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
                
                default:
                    Logger.WarnFormat("Unknown configuration key: {0}", key);
                    return;
            }
            Logger.DebugFormat("Read configuration item {0} with value {1}", key, value);
        }
    }
}
using GlashartLibrary.Helpers;
using GlashartLibrary.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;

namespace GlashartLibrary
{
    public sealed class Main
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Main));

        public static ISettings Settings;
        private const string TvMenuFileName = "index.xhtml.gz";
        private const string TvMenuFileNameDecompressed = "index.html";
        private const string TvMenuScriptFile = "code.js.gz";
        private const string TvMenuScriptFileDecompressed = "code.js";
        private const string ChannelsXmlFile = "Channels.xml";
        private const string EPGXmlFile = "EPG.xml";

        /// <summary>
        /// Downloads the tv menu file
        /// </summary>
        /// <returns></returns>
        public static bool DownloadTvMenu()
        {
            try
            {
                string url = string.Concat(Settings.TvMenuURL, TvMenuFileName);
                string localFile = Path.Combine(Settings.TvMenuFolder, TvMenuFileName);

                Logger.InfoFormat("Downloading TV menu to {0}", localFile);
                HttpDownloader.DownloadBinaryFile(url, localFile);
                Logger.Info("TV menu file downloaded");

                return true;
            }
            catch (Exception err)
            {
                Logger.Error(err);
                return false;
            }
        }

        /// <summary>
        /// Decompresses the tv menu file
        /// </summary>
        /// <returns></returns>
        public static bool DecompressTvMenu()
        {
            try
            {
                string localFile = Path.Combine(Settings.TvMenuFolder, TvMenuFileName);
                string localHtmlFile = Path.Combine(Settings.TvMenuFolder, TvMenuFileNameDecompressed);

                Logger.InfoFormat("Uncompressing TV menu to {0}", localHtmlFile);
                CompressionHelper.Decompress(localFile, localHtmlFile);
                Logger.Info("TV menu file uncompressed");

                return true;
            }
            catch (Exception err)
            {
                Logger.Error(err);
                return false;
            }
        }

        /// <summary>
        /// Downloads the tv menu (java)script file
        /// </summary>
        /// <returns></returns>
        public static bool DownloadTvMenuScript()
        {
            try
            {
                //Parse the HTML file and determine tje path to the javascript file
                string localHtmlFile = Path.Combine(Settings.TvMenuFolder, TvMenuFileNameDecompressed);
                string javascriptFile = HtmlHelper.GetScriptTagSrc(localHtmlFile);
                if (string.IsNullOrWhiteSpace(javascriptFile))
                {
                    Logger.Error("TV menu script file not found in TV menu HTML file");
                    return false;
                }

                //Determine the url to download
                string url = string.Concat(Settings.TvMenuURL, javascriptFile);
                string localFile = Path.Combine(Settings.TvMenuFolder, TvMenuScriptFile);

                Logger.InfoFormat("Downloading TV menu script to {0}", localFile);
                HttpDownloader.DownloadBinaryFile(url, localFile);
                Logger.Info("TV menu script file downloaded");

                return true;
            }
            catch (Exception err)
            {
                Logger.Error(err);
                return false;
            }
        }

        /// <summary>
        /// Decompresses the tv menu script file
        /// </summary>
        /// <returns></returns>
        public static bool DecompressTvMenuScript()
        {
            try
            {
                string localFile = Path.Combine(Settings.TvMenuFolder, TvMenuScriptFile);
                string localHtmlFile = Path.Combine(Settings.TvMenuFolder, TvMenuScriptFileDecompressed);

                Logger.InfoFormat("Uncompressing TV menu script file to {0}", localHtmlFile);
                CompressionHelper.Decompress(localFile, localHtmlFile);
                Logger.Info("TV menu script file uncompressed");

                return true;
            }
            catch (Exception err)
            {
                Logger.Error(err);
                return false;
            }
        }

        /// <summary>
        /// Generates the channel XML file.
        /// </summary>
        public static List<Channel> GenerateChannelXmlFile()
        {
            try
            {
                string localFile = Path.Combine(Settings.TvMenuFolder, TvMenuScriptFileDecompressed);

                Logger.Info("Parsing channels from the TV menu script file");
                var channels = JavascriptHelper.ParseChannnels(localFile);
                Logger.InfoFormat("{0} channels found in TV menu script file", channels.Count);

                string channelFile = Path.Combine(Settings.TvMenuFolder, ChannelsXmlFile);
                Logger.InfoFormat("Logging channel list to file {0}", channelFile);
                XmlHelper.Serialize(channels, channelFile);
                Logger.Info("Channel XML file generated");

                return channels;
            }
            catch (Exception err)
            {
                Logger.Error(err);
                return null;
            }
        }

        /// <summary>
        /// Generates the M3U file.
        /// </summary>
        /// <param name="channels">The channels.</param>
        /// <returns>List of generated channels</returns>
        public static List<Channel> GenerateM3Ufile(List<Channel> channels = null)
        {
            try
            {
                List<Channel> result = null;

                string localFile = Path.Combine(Settings.TvMenuFolder, ChannelsXmlFile);
                string m3uFile = Settings.M3UfileName;

                //Read the channels for the xml file
                if (channels == null)
                {
                    Logger.InfoFormat("Reading channels from XML file {0}", localFile);
                    channels = XmlHelper.Deserialize<List<Channel>>(localFile);
                    Logger.InfoFormat("{0} channels found in channels xml file", channels.Count);
                }

                //Determine if a channel list is present
                var channelList = ReadChannelList(channels);

                Logger.InfoFormat("Generating M3U file {0} based on channels", m3uFile);
                result = M3UHelper.GenerateM3U(channels, channelList, m3uFile, Settings.M3U_ChannelLocationImportance.OfType<string>().ToArray());
                Logger.Info("M3U file generated");

                return result;
            }
            catch (Exception err)
            {
                Logger.Error(err);
                return null;
            }
        }
        /// <summary>
        /// Converts the M3U file.
        /// </summary>
        public static bool ConvertM3Ufile()
        {
            try
            {
                string localFile = Path.Combine(Settings.TvMenuFolder, ChannelsXmlFile);
                string m3uFile = Settings.M3UfileName;
                string downloadedM3uFile = Settings.DownloadedM3UFileName;

                Logger.InfoFormat("Reading downloaded M3U file {0}", downloadedM3uFile);
                var channels = M3UHelper.ParseM3U(downloadedM3uFile);
                Logger.Info("Downloaded M3U file parsed");

                //Determine if a channel list is present
                var channelList = ReadChannelList(null);

                Logger.InfoFormat("Generating M3U file {0} based on parsed M3U file", m3uFile);
                M3UHelper.GenerateM3U(channels, channelList, m3uFile);
                Logger.Info("M3U file generated");

                return true;
            }
            catch (Exception err)
            {
                Logger.Error(err);
                return false;
            }
        }

        /// <summary>
        /// Reads the channel list from the channel file
        /// </summary>
        /// <param name="availableChannels">The available channels.</param>
        /// <returns></returns>
        private static List<ChannelListItem> ReadChannelList(List<Channel> availableChannels)
        {
            var result = new List<ChannelListItem>();

            //Read the channel list file (which has channels per line in the format {number},{originalname},{newname}. {newname} is optional)
            string fileName = Settings.ChannelsListFile;
            Logger.DebugFormat("Reading channel list from {0}", fileName);

            if (File.Exists(fileName))
            {
                foreach (string line in File.ReadLines(fileName))
                {
                    var channel = ChannelListItem.Parse(line);
                    if (channel != null)
                        result.Add(channel);
                }
            }

            //When the list is empty, create a list on all available channels
            if (result.Count == 0 && availableChannels != null)
            {
                Logger.Debug("No channels found in channel list file. Using all available channels");
                for (int i = 0; i < availableChannels.Count; i++)
                    result.Add(new ChannelListItem { Number = (i + 1), OriginalName = availableChannels[i].Name });
            }

            return result;
        }

        /// <summary>
        /// Downloads the EPG files
        /// </summary>
        public static bool DownloadEPGfiles()
        {
            try
            {
                try
                {
                    Logger.DebugFormat("Cleanup EPG files from folder {0} (older than {1} days)", Settings.EpgFolder, Settings.EpgArchiving);
                    EPGhelper.CleanUpEPG(Settings.EpgFolder, Settings.EpgArchiving);
                    Logger.Debug("EPG files cleaned up");
                }
                catch (Exception err)
                {
                    Logger.Error(err);
                }

                Logger.InfoFormat("Downloading EPG files to folder {0}", Settings.EpgFolder);
                EPGhelper.DownloadEPGfiles(Settings.EpgURL, Settings.EpgFolder, Settings.EpgNumberOfDays);
                Logger.Info("EPG files downloaded");

                return true;
            }
            catch (Exception err)
            {
                Logger.Error(err);
                return false;
            }
        }

        /// <summary>
        /// Decompresses the EPG files
        /// </summary>
        public static bool DecompressEPGfiles()
        {
            try
            {
                Logger.InfoFormat("Decompressing EPG files in folder {0}", Settings.EpgFolder);
                EPGhelper.DecompressEPGfiles(Settings.EpgFolder, Settings.EpgNumberOfDays);
                Logger.Info("EPG files decompressed");

                return true;
            }
            catch (Exception err)
            {
                Logger.Error(err);
                return false;
            }
        }

        public static List<EpgChannel> ReadEpgFromFiles()
        {
            Logger.InfoFormat("Reading EPG files from folder {0}", Settings.EpgFolder);
            var epg = EPGhelper.ReadEPGfiles(Settings.EpgFolder, Settings.EpgNumberOfDays);
            Logger.Info("EPG files read");
            return epg;
        }

        public static List<Channel> ReadChannelList()
        {
            //Read the channels for the xml file
            string localFile = Path.Combine(Settings.TvMenuFolder, ChannelsXmlFile);
            Logger.DebugFormat("Reading channels from XML file {0}", localFile);
            var channels = XmlHelper.Deserialize<List<Channel>>(localFile);
            Logger.DebugFormat("{0} channels found in channels xml file", channels.Count);


            //Determine if a channel list is present
            var channelList = ReadChannelList(channels);
            //Filter the channels based on the channel list, because this is the list we want to use
            List<Channel> channelsToUse = new List<Channel>();
            foreach (var channelListItem in channelList)
            {
                var channelToUse = channels.FirstOrDefault(c => c.Name.Equals(channelListItem.OriginalName, StringComparison.InvariantCultureIgnoreCase));
                if (channelToUse != null)
                {
                    channelToUse.Name = channelListItem.GetName();
                    channelsToUse.Add(channelToUse);
                }
            }

            return channelsToUse;
        }

        /// <summary>
        /// Generates the XMLTV file
        /// </summary>
        public static bool GenerateXmlTv(List<EpgChannel> epg)
        {
            try
            {
                var channelsToUse = ReadChannelList();
                //Generate XMLTV file
                string xmltvFile = Settings.XmlTvFileName;
                Logger.InfoFormat("Generating XMLTV file {0}", xmltvFile);
                XmlTvHelper.GenerateXmlTv(epg, channelsToUse, xmltvFile);
                Logger.Info("XMLTV file generated");

                return true;
            }
            catch (Exception err)
            {
                Logger.Error(err);
                return false;
            }
        }

        public static List<EpgChannel> DownloadDetails(List<EpgChannel> epg)
        {
            int succes = 0, failed = 0, percent = 0;
            float total = epg.SelectMany(channel => channel.Programs).Count();
            foreach(var program in epg.SelectMany(channel => channel.Programs))
            {
                //Log percentage of completion
                var current = (int)(((succes + failed)/total) * 100);
                if (current != percent)
                {
                    percent = current;
                    Logger.InfoFormat("Tried downloading {0}% EPG program details", percent);
                }
                //Download string
                Logger.DebugFormat("Try to download details for: {0}", program.Id);
                var details = EPGhelper.DownloadDetails(program.Id);
                if (string.IsNullOrWhiteSpace(details))
                {
                    failed++;
                    continue;
                }
                //Parse and update
                var parsed = JsonConvert.DeserializeObject<EpgDetails>(details);
                program.Description = parsed.Description;
                program.Genres = parsed.Genres;
                Logger.DebugFormat("Updated program {0}", program.Id);
                succes++;
            }

            Logger.InfoFormat("Succesfully loaded details for {0} programs", succes);
            Logger.InfoFormat("Failed to load details for {0} programs", failed);
            return epg;
        }
    }
}

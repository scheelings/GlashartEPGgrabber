using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlashartLibrary.IO;
using GlashartLibrary.Settings;
using log4net;

namespace GlashartLibrary.Helpers
{
    public sealed class TvHeadendHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TvHeadendHelper));
        private readonly ISettings _settings;

        private const string NetworkName = "IPTV";

        /// <summary>
        /// Initializes a new instance of the <see cref="TvHeadendHelper"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public TvHeadendHelper(ISettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Generates the TVheadend configuration files
        /// </summary>
        /// <param name="channels">The channel list.</param>
        /// <param name="channelList">Filtered channel list.</param>
        /// <param name="folder">The folder to save the TVH files.</param>
        /// <param name="locationImportanceList">The location importance list (so the 1st string in this array is the name of the channel location which is most important; when this</param>
        public void GenerateTVH(List<Channel> channels, List<ChannelListItem> channelList, string folder, params string[] locationImportanceList)
        {
            if (channels == null || channels.Count == 0)
                return;

            var networkId = Guid.NewGuid().ToString("N");
            GenerateFolders(folder, networkId);
            var tags = new Dictionary<string, string>();

            //Loop throug the channel list
            foreach (var channelListItem in channelList)
            {
                //Find channel
                var channel = channels.FirstOrDefault(c => c.Name.Equals(channelListItem.OriginalName, StringComparison.InvariantCultureIgnoreCase));
                if (channel == null)
                {
                    Logger.DebugFormat("Channel '{0}' not found in available channels. Ignoring...", channelListItem.OriginalName);
                    continue;
                }
                if (channel.Locations == null || channel.Locations.Count == 0)
                {
                    Logger.DebugFormat("TVheadend generator ignores {0}, because no locations found", channel.Name);
                    continue;
                }

                //Get the (most important) location
                string locationUrl = null;
                var locationFound = false;
                foreach (var mostImportantLocation in locationImportanceList)
                {
                    var location = channel.Locations.FirstOrDefault(l => mostImportantLocation.Equals(l.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (location != null)
                    {
                        locationUrl = GetLocationUrl(location.Url);
                        locationFound = true;
                        break;
                    }
                }
                //When no location is found, we will get the 1st one in the list
                if (!locationFound)
                {
                    var location = channel.Locations.First();
                    locationUrl = GetLocationUrl(location.Url);
                    Logger.DebugFormat("TVheadend generator selects first location {0} for channel {1}, because no important location is found", location.Name, channel.Name);
                }

                //No Tag property in our Channels yet
                var tag = string.Empty; //channel.Tag
                var tagIndexes = new List<string>();
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    //    tag = channel.Tag;
                    var channelTags = tag.Split(',');
                    foreach (var channelTag in channelTags)
                    {
                        if (!tags.ContainsKey(channelTag))
                        {
                            tags.Add(channelTag, Guid.NewGuid().ToString("N"));
                            File.WriteAllText(Path.Combine(folder, "channel", "tag", tags[channelTag]),
                                string.Concat("{\n\t\"enabled\": true,\n\t\"internal\": false,\n\t\"titledIcon\": false,\n\t\"name\": \"", channelTag, "\",\n\t\"comment\": \"\",\n\t\"icon\": \"\"\n}"));
                        }
                        tagIndexes.Add(string.Concat("\"", tags[channelTag], "\""));
                    }
                }

                //Identifier
                var id = channel.Name.ToCharArray();
                id = Array.FindAll(id, char.IsLetterOrDigit);
                var identifier = new string(id);
                //Image icon
                var img = channel.Icons.FirstOrDefault(ico => File.Exists(Path.Combine(_settings.IconFolder, ico)));
                //Id's
                var muxId = Guid.NewGuid().ToString("N");
                var svcId = Guid.NewGuid().ToString("N");
                var channelId = Guid.NewGuid().ToString("N");

                //Generate folder and files
                Directory.CreateDirectory(Path.Combine(folder, "input", "iptv", "networks", networkId, "muxes", muxId, "services"));
                File.WriteAllText(Path.Combine(folder, "input", "iptv", "networks", networkId, "muxes", muxId, "config"), 
                    string.Concat("{\n\t\"iptv_url\": \"", locationUrl, "\",\n\t\"iptv_interface\": \"", _settings.TVheadendNetworkInterface, "\",\n\t\"iptv_atsc\": false,\n\t\"iptv_muxname\": \"", identifier, "\",\n\t\"iptv_sname\": \"", identifier, "\",\n\t\"iptv_respawn\": false,\n\t\"enabled\": true,\n\t\"scan_result\": 1\n}"));
                File.WriteAllText(Path.Combine(folder, "input", "iptv", "networks", networkId, "muxes", muxId, "services", svcId), 
                    string.Concat("{\n\t\"sid\": 1,\n\t\"svcname\": \"", identifier, "\",\n\t\"dvb_servicetype\": ", (tag.Contains("Radio") || tag.Contains("radio") ? 2 : 1), ",\n\t\"enabled\": true\n}"));
                File.WriteAllText(Path.Combine(folder, "channel", "config", channelId), 
                    string.Concat("{\n\t\"enabled\": true,\n\t\"name\": \"", channel.Name, "\",\n\t\"number\": \"", channelListItem.Number, "\",\n\t\"icon\": \"", img, "\",\n\t\"dvr_pre_time\": 0,\n\t\"dvr_pst_time\": 0,\n\t\"services\": [\n\t\t\"", svcId, "\"\n\t],\n\t\"tags\": [\n\t\t", string.Join(",\n\t\t", tagIndexes), "\n\t],\n\t\"bouquet\": \"\"\n}"));

                //No EPG property in our channel yet
                //if (!string.IsNullOrWhiteSpace(channel.EPG))
                //    File.WriteAllText(folder + "\\epggrab\\xmltv\\channels\\" + channel_id, "{\n\t\"name\": \"" + identifier + "\",\n\t\"channels\": [\n\t\t\"" + channel_id + "\"\n\t]\n}");
            }
        }

        /// <summary>
        /// Generates the TVheadend configuration files
        /// </summary>
        /// <param name="channels">The channel list.</param>
        /// <param name="channelList">The filtered channel list.</param>
        /// <param name="folder">The folder to save the TVH files.</param>
        public void GenerateTVH(List<M3UChannel> channels, List<ChannelListItem> channelList, string folder)
        {
            if (channels == null || channels.Count == 0)
                return;

            var networkId = Guid.NewGuid().ToString("N");
            GenerateFolders(folder, networkId);
            var tags = new Dictionary<string, string>();

            //Loop throug the channel list
            foreach (var channelListItem in channelList)
            {
                //Find channel
                var channel = channels.FirstOrDefault(c => c.Name.Equals(channelListItem.OriginalName, StringComparison.InvariantCultureIgnoreCase));
                if (channel == null)
                {
                    Logger.DebugFormat("Channel '{0}' not found in available channels. Ignoring...", channelListItem.OriginalName);
                    continue;
                }
                if (string.IsNullOrWhiteSpace(channel.Url))
                {
                    Logger.DebugFormat("TVheadend generator ignores {0}, because no URL present", channel.Name);
                    continue;
                }


                //No Tag property in our Channels yet
                var tag = string.Empty; //channel.Tag
                var tagIndexes = new List<string>();
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    //    tag = channel.Tag;
                    var channelTags = tag.Split(',');
                    foreach (var channelTag in channelTags)
                    {
                        if (!tags.ContainsKey(channelTag))
                        {
                            tags.Add(channelTag, Guid.NewGuid().ToString("N"));
                            File.WriteAllText(Path.Combine(folder, "channel", "tag", tags[channelTag]),
                                string.Concat("{\n\t\"enabled\": true,\n\t\"internal\": false,\n\t\"titledIcon\": false,\n\t\"name\": \"", channelTag, "\",\n\t\"comment\": \"\",\n\t\"icon\": \"\"\n}"));
                        }
                        tagIndexes.Add(string.Concat("\"", tags[channelTag], "\""));
                    }
                }

                //Identifier
                var id = channel.Name.ToCharArray();
                id = Array.FindAll(id, char.IsLetterOrDigit);
                var identifier = new string(id);
                //No Image property in our Channels yet
                var img = string.Empty;// channel.Icons.FirstOrDefault(ico => File.Exists(Path.Combine(_settings.IconFolder, ico)));
                //Id's
                var muxId = Guid.NewGuid().ToString("N");
                var svcId = Guid.NewGuid().ToString("N");
                var channelId = Guid.NewGuid().ToString("N");

                //Generate folder and files
                Directory.CreateDirectory(Path.Combine(folder, "input", "iptv", "networks", networkId, "muxes", muxId, "services"));
                File.WriteAllText(Path.Combine(folder, "input", "iptv", "networks", networkId, "muxes", muxId, "config"),
                    string.Concat("{\n\t\"iptv_url\": \"", channel.Url, "\",\n\t\"iptv_interface\": \"", _settings.TVheadendNetworkInterface, "\",\n\t\"iptv_atsc\": false,\n\t\"iptv_muxname\": \"", identifier, "\",\n\t\"iptv_sname\": \"", identifier, "\",\n\t\"iptv_respawn\": false,\n\t\"enabled\": true,\n\t\"scan_result\": 1\n}"));
                File.WriteAllText(Path.Combine(folder, "input", "iptv", "networks", networkId, "muxes", muxId, "services", svcId),
                    string.Concat("{\n\t\"sid\": 1,\n\t\"svcname\": \"", identifier, "\",\n\t\"dvb_servicetype\": ", (tag.Contains("Radio") || tag.Contains("radio") ? 2 : 1), ",\n\t\"enabled\": true\n}"));
                File.WriteAllText(Path.Combine(folder, "channel", "config", channelId),
                    string.Concat("{\n\t\"enabled\": true,\n\t\"name\": \"", channel.Name, "\",\n\t\"number\": \"", channelListItem.Number, "\",\n\t\"icon\": \"", img, "\",\n\t\"dvr_pre_time\": 0,\n\t\"dvr_pst_time\": 0,\n\t\"services\": [\n\t\t\"", svcId, "\"\n\t],\n\t\"tags\": [\n\t\t", string.Join(",\n\t\t", tagIndexes), "\n\t],\n\t\"bouquet\": \"\"\n}"));

                //No EPG property in our channel yet
                //if (!string.IsNullOrWhiteSpace(channel.EPG))
                //    File.WriteAllText(folder + "\\epggrab\\xmltv\\channels\\" + channel_id, "{\n\t\"name\": \"" + identifier + "\",\n\t\"channels\": [\n\t\t\"" + channel_id + "\"\n\t]\n}");
            }
        }

        /// <summary>
        /// Generates the TVheadend configuration folders in the specified folder
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="networkId">The network identifier.</param>
        private void GenerateFolders(string folder, string networkId)
        {
            //Cleanup folder first
            FileHelper.DeleteFolderContent(folder, true);

            //Generate the necessary folders
            Directory.CreateDirectory(Path.Combine(folder, "epggrab", "xmltv", "channels"));
            Directory.CreateDirectory(Path.Combine(folder, "channel", "tag"));
            Directory.CreateDirectory(Path.Combine(folder, "channel", "config"));
            Directory.CreateDirectory(Path.Combine(folder, "input", "iptv", "networks", networkId, "muxes"));

            File.WriteAllText(Path.Combine(folder, "input", "iptv", "config"),
                string.Concat("{\n\t\"uuid\": \"", Guid.NewGuid().ToString("N"), "\",\n\t\"skipinitscan\": true,\n\t\"autodiscovery\": false\n}"));
            File.WriteAllText(Path.Combine(folder, "input", "iptv", "networks", networkId, "config"),
                string.Concat("{\n\t\"nid\": 0,\n\t\"networkname\": \"", NetworkName, "\",\n\t\"skipinitscan\": true,\n\t\"autodiscovery\": false,\n\t\"idlescan\": false,\n\t\"max_streams\": 2,\n\t\"max_bandwidth\": 0,\n\t\"max_timeout\": 10\n}"));
        }

        /// <summary>
        /// Gets the location URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private string GetLocationUrl(string url)
        {
            if (_settings.IgmpToUdp)
                url = url.Replace("igmp://", "udp://@");
            return url;
        }
    }
}

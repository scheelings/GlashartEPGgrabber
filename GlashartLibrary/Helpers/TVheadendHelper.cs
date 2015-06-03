using GlashartLibrary.IO;
using GlashartLibrary.Settings;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlashartLibrary.Helpers
{
    public sealed class TVheadendHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TVheadendHelper));
        private readonly ISettings _settings;

        private const string NetworkName = "IPTV";

        /// <summary>
        /// Initializes a new instance of the <see cref="TVheadendHelper"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public TVheadendHelper(ISettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Generates the TVheadend configuration files
        /// </summary>
        /// <param name="channels">The channel list.</param>
        /// <param name="folder">The folder to save the TVH files.</param>
        /// <param name="locationImportanceList">The location importance list (so the 1st string in this array is the name of the channel location which is most important; when this
        public void GenerateTVH(List<Channel> channels, List<ChannelListItem> channelList, string folder, params string[] locationImportanceList)
        {
            if (channels == null || channels.Count == 0)
                return;

            string networkId = System.Guid.NewGuid().ToString("N");
            GenerateFolders(folder, networkId);
            Dictionary<string, string> tags = new Dictionary<string, string>();

            //Loop throug the channel list
            foreach (ChannelListItem channelListItem in channelList)
            {
                //Find channel
                Channel channel = channels.FirstOrDefault(c => c.Name.Equals(channelListItem.OriginalName, StringComparison.InvariantCultureIgnoreCase));
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
                bool locationFound = false;
                foreach (string mostImportantLocation in locationImportanceList)
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
                string tag = string.Empty; //channel.Tag
                List<string> tagIndexes = new List<string>();
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    //    tag = channel.Tag;
                    string[] channelTags = tag.Split(',');
                    foreach (string channelTag in channelTags)
                    {
                        if (!tags.ContainsKey(channelTag))
                        {
                            tags.Add(channelTag, System.Guid.NewGuid().ToString("N"));
                            File.WriteAllText(Path.Combine(folder, "channel", "tag", tags[channelTag]),
                                string.Concat("{\n\t\"enabled\": true,\n\t\"internal\": false,\n\t\"titledIcon\": false,\n\t\"name\": \"", channelTag, "\",\n\t\"comment\": \"\",\n\t\"icon\": \"\"\n}"));
                        }
                        tagIndexes.Add(string.Concat("\"", tags[channelTag], "\""));
                    }
                }

                //Identifier
                char[] id = channel.Name.ToCharArray();
                id = Array.FindAll<char>(id, (c => char.IsLetterOrDigit(c)));
                string identifier = new string(id);
                //Image icon
                string img = channel.Icons.FirstOrDefault(ico => File.Exists(Path.Combine(_settings.IconFolder, ico)));
                //Id's
                string mux_id = System.Guid.NewGuid().ToString("N");
                string svc_id = System.Guid.NewGuid().ToString("N");
                string channel_id = System.Guid.NewGuid().ToString("N");

                //Generate folder and files
                Directory.CreateDirectory(Path.Combine(folder, "input", "iptv", "networks", networkId, "muxes", mux_id, "services"));
                File.WriteAllText(Path.Combine(folder, "input", "iptv", "networks", networkId, "muxes", mux_id, "config"), 
                    string.Concat("{\n\t\"iptv_url\": \"", locationUrl, "\",\n\t\"iptv_interface\": \"", _settings.TVheadendNetworkInterface, "\",\n\t\"iptv_atsc\": false,\n\t\"iptv_muxname\": \"", identifier, "\",\n\t\"iptv_sname\": \"", identifier, "\",\n\t\"iptv_respawn\": false,\n\t\"enabled\": true,\n\t\"scan_result\": 1\n}"));
                File.WriteAllText(Path.Combine(folder, "input", "iptv", "networks", networkId, "muxes", mux_id, "services", svc_id), 
                    string.Concat("{\n\t\"sid\": 1,\n\t\"svcname\": \"", identifier, "\",\n\t\"dvb_servicetype\": ", (tag.Contains("Radio") || tag.Contains("radio") ? 2 : 1), ",\n\t\"enabled\": true\n}"));
                File.WriteAllText(Path.Combine(folder, "channel", "config", channel_id), 
                    string.Concat("{\n\t\"enabled\": true,\n\t\"name\": \"", channel.Name, "\",\n\t\"number\": \"", channelListItem.Number, "\",\n\t\"icon\": \"", img, "\",\n\t\"dvr_pre_time\": 0,\n\t\"dvr_pst_time\": 0,\n\t\"services\": [\n\t\t\"", svc_id, "\"\n\t],\n\t\"tags\": [\n\t\t", string.Join(",\n\t\t", tagIndexes), "\n\t],\n\t\"bouquet\": \"\"\n}"));

                //No EPG property in our channel yet
                //if (!string.IsNullOrWhiteSpace(channel.EPG))
                //    File.WriteAllText(folder + "\\epggrab\\xmltv\\channels\\" + channel_id, "{\n\t\"name\": \"" + identifier + "\",\n\t\"channels\": [\n\t\t\"" + channel_id + "\"\n\t]\n}");
            }
        }

        /// <summary>
        /// Generates the TVheadend configuration files
        /// </summary>
        /// <param name="channels">The channel list.</param>
        /// <param name="folder">The folder to save the TVH files.</param>
        public void GenerateTVH(List<M3UChannel> channels, List<ChannelListItem> channelList, string folder)
        {
            if (channels == null || channels.Count == 0)
                return;

            string networkId = System.Guid.NewGuid().ToString("N");
            GenerateFolders(folder, networkId);
            Dictionary<string, string> tags = new Dictionary<string, string>();

            //Loop throug the channel list
            foreach (ChannelListItem channelListItem in channelList)
            {
                //Find channel
                M3UChannel channel = channels.FirstOrDefault(c => c.Name.Equals(channelListItem.OriginalName, StringComparison.InvariantCultureIgnoreCase));
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
                string tag = string.Empty; //channel.Tag
                List<string> tagIndexes = new List<string>();
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    //    tag = channel.Tag;
                    string[] channelTags = tag.Split(',');
                    foreach (string channelTag in channelTags)
                    {
                        if (!tags.ContainsKey(channelTag))
                        {
                            tags.Add(channelTag, System.Guid.NewGuid().ToString("N"));
                            File.WriteAllText(Path.Combine(folder, "channel", "tag", tags[channelTag]),
                                string.Concat("{\n\t\"enabled\": true,\n\t\"internal\": false,\n\t\"titledIcon\": false,\n\t\"name\": \"", channelTag, "\",\n\t\"comment\": \"\",\n\t\"icon\": \"\"\n}"));
                        }
                        tagIndexes.Add(string.Concat("\"", tags[channelTag], "\""));
                    }
                }

                //Identifier
                char[] id = channel.Name.ToCharArray();
                id = Array.FindAll<char>(id, (c => char.IsLetterOrDigit(c)));
                string identifier = new string(id);
                //No Image property in our Channels yet
                string img = string.Empty;// channel.Icons.FirstOrDefault(ico => File.Exists(Path.Combine(_settings.IconFolder, ico)));
                //Id's
                string mux_id = System.Guid.NewGuid().ToString("N");
                string svc_id = System.Guid.NewGuid().ToString("N");
                string channel_id = System.Guid.NewGuid().ToString("N");

                //Generate folder and files
                Directory.CreateDirectory(Path.Combine(folder, "input", "iptv", "networks", networkId, "muxes", mux_id, "services"));
                File.WriteAllText(Path.Combine(folder, "input", "iptv", "networks", networkId, "muxes", mux_id, "config"),
                    string.Concat("{\n\t\"iptv_url\": \"", channel.Url, "\",\n\t\"iptv_interface\": \"", _settings.TVheadendNetworkInterface, "\",\n\t\"iptv_atsc\": false,\n\t\"iptv_muxname\": \"", identifier, "\",\n\t\"iptv_sname\": \"", identifier, "\",\n\t\"iptv_respawn\": false,\n\t\"enabled\": true,\n\t\"scan_result\": 1\n}"));
                File.WriteAllText(Path.Combine(folder, "input", "iptv", "networks", networkId, "muxes", mux_id, "services", svc_id),
                    string.Concat("{\n\t\"sid\": 1,\n\t\"svcname\": \"", identifier, "\",\n\t\"dvb_servicetype\": ", (tag.Contains("Radio") || tag.Contains("radio") ? 2 : 1), ",\n\t\"enabled\": true\n}"));
                File.WriteAllText(Path.Combine(folder, "channel", "config", channel_id),
                    string.Concat("{\n\t\"enabled\": true,\n\t\"name\": \"", channel.Name, "\",\n\t\"number\": \"", channelListItem.Number, "\",\n\t\"icon\": \"", img, "\",\n\t\"dvr_pre_time\": 0,\n\t\"dvr_pst_time\": 0,\n\t\"services\": [\n\t\t\"", svc_id, "\"\n\t],\n\t\"tags\": [\n\t\t", string.Join(",\n\t\t", tagIndexes), "\n\t],\n\t\"bouquet\": \"\"\n}"));

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
            FileHelper.DeleteFolderContent(folder, true, false);

            //Generate the necessary folders
            Directory.CreateDirectory(Path.Combine(folder, "epggrab", "xmltv", "channels"));
            Directory.CreateDirectory(Path.Combine(folder, "channel", "tag"));
            Directory.CreateDirectory(Path.Combine(folder, "channel", "config"));
            Directory.CreateDirectory(Path.Combine(folder, "input", "iptv", "networks", networkId, "muxes"));

            File.WriteAllText(Path.Combine(folder, "input", "iptv", "config"),
                string.Concat("{\n\t\"uuid\": \"", System.Guid.NewGuid().ToString("N"), "\",\n\t\"skipinitscan\": true,\n\t\"autodiscovery\": false\n}"));
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

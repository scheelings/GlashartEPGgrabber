using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GlashartLibrary.Settings;
using log4net;

namespace GlashartLibrary.Helpers
{
    public sealed class M3UHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(M3UHelper));

        private const string ChannelLineStart = "#EXTINF:";
        private static readonly string[] ChannelUrlStarts = new string[] { "udp://", "rtp://", "rtsp://", "igmp://" };

        private readonly ISettings _settings;

        public M3UHelper(ISettings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Generates the M3U file
        /// </summary>
        /// <example>
        /// #EXTM3U
        /// #EXTINF:1,Nederland 1
        /// udp://@224.1.3.1:12110
        /// </example>
        /// <param name="channels">The channel list.</param>
        /// <param name="fileName">M3U file name.</param>
        /// <param name="locationImportanceList">The location importance list (so the 1st string in this array is the name of the channel location which is most important; when this
        /// location name does not exist; the 2nd string in the array will be used; and so on...</param>
        /// <returns>List of generated channels</returns>
        public List<Channel> GenerateM3U(List<Channel> channels, List<ChannelListItem> channelList, string fileName, params string[] locationImportanceList)
        {
            if (channels == null || channels.Count == 0)
                return null;

            List<Channel> result = new List<Channel>();
            List<string> lines = new List<string>();
            lines.Add("#EXTM3U");

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
                    Logger.DebugFormat("M3U generator ignores {0}, because no locations found", channel.Name);
                    continue;
                }

                //Add line for channel
                result.Add(channel);
                lines.Add(string.Format("#EXTINF:{0},{1}", channelListItem.Number, channelListItem.GetName()));
                
                //Get the (most important) location
                bool locationFound = false;
                foreach (string mostImportantLocation in locationImportanceList)
                {
                    var location = channel.Locations.FirstOrDefault(l => mostImportantLocation.Equals(l.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (location != null)
                    {
                        lines.Add(GetLocationUrl(location.Url));
                        locationFound = true;
                        break;
                    }
                }
                //When no location is found, we will get the 1st one in the list
                if (!locationFound)
                {
                    var location = channel.Locations.First();
                    lines.Add(GetLocationUrl(location.Url));
                    Logger.DebugFormat("M3U generator selects first location {0} for channel {1}, because no important location is found", location.Name, channel.Name);
                }
            }

            //Write lines to file
            File.WriteAllLines(fileName, lines, Encoding.UTF8);

            return result;
        }

        /// <summary>
        /// Generates the M3U file
        /// </summary>
        /// <example>
        /// #EXTM3U
        /// #EXTINF:1,Nederland 1
        /// udp://@224.1.3.1:12110
        /// </example>
        /// <param name="channels">The channel list.</param>
        /// <param name="fileName">M3U file name.</param>
        /// <returns>List of generated channels</returns>
        public List<M3UChannel> GenerateM3U(List<M3UChannel> channels, List<ChannelListItem> channelList, string fileName)
        {
            if (channels == null || channels.Count == 0)
                return null;

            List<M3UChannel> result = new List<M3UChannel>();
            List<string> lines = new List<string>();
            lines.Add("#EXTM3U");

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
                    Logger.DebugFormat("M3U generator ignores {0}, because no URL present", channel.Name);
                    continue;
                }

                //Add line for channel
                result.Add(channel);
                lines.Add(string.Format("#EXTINF:{0},{1}", channelListItem.Number, channelListItem.GetName()));
                lines.Add(channel.Url);
            }

            //Write lines to file
            File.WriteAllLines(fileName, lines, Encoding.UTF8);

            return result;
        }

        /// <summary>
        /// Parses the m3u file
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public List<M3UChannel> ParseM3U(string fileName)
        {
            List<M3UChannel> result = new List<M3UChannel>();

            foreach (string line in File.ReadAllLines(fileName, Encoding.UTF8))
            {
                //Check the current line
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith(ChannelLineStart, StringComparison.InvariantCultureIgnoreCase))
                {
                    //Channel no and name
                    var channel = new M3UChannel();
                    result.Add(channel);

                    string[] parts = line.Substring(ChannelLineStart.Length).Split(',');
                    if (parts.Length == 2)
                    {
                        channel.Number = parts[0].Trim();
                        channel.Name = parts[1].Trim();
                    }
                    else if (parts.Length == 1)
                        channel.Name = parts[0].Trim();
                }
                else if (ChannelUrlStarts.Any(e => line.StartsWith(e, StringComparison.InvariantCultureIgnoreCase)))
                {
                    //Channel URL
                    result.Last().Url = line.Trim();
                }
            }

            return result;
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

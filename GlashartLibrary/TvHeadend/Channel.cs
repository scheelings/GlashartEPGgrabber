using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlashartLibrary.TvHeadend
{
    public class Channel
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Channel));

        [JsonIgnore]
        public string Id { get; set; }
        [JsonIgnore]
        public State State { get; private set; }

        /*TvHeadend properties*/
        public bool enabled { get; set; }
        public string name { get; set; }
        public int number { get; set; }
        public string icon { get; set; }
        public bool epgauto { get; set; }
        public int dvr_pre_time { get; set; }
        public int dvr_pst_time { get; set; }
        public List<string> services { get; set; }
        public List<string> tags { get; set; }
        public string bouquet { get; set; }

        /*Tvheadend extra properties*/
        [JsonExtensionData]
        public IDictionary<string, JToken> _additionalData;

        public Channel()
        {
            Id = Guid.NewGuid().ToString();
            name = string.Empty;
            number = -1;
            icon = string.Empty;
            enabled = true;
            epgauto = true;
            dvr_pre_time = 0;
            dvr_pst_time = 0;
            services = new List<string>();
            tags = new List<string>();
            bouquet = string.Empty;
            State = State.New;
        }

        public static List<Channel> ReadFromDisk(string tvhFolder)
        {
            var channels = new List<Channel>();
            var channelsFolder = GetFolder(tvhFolder);
            if (!Directory.Exists(channelsFolder))
            {
                Logger.WarnFormat("Directory {0} doesn't exist", channelsFolder);
                return channels;
            }

            channels.AddRange(
                Directory.EnumerateFiles(channelsFolder)
                         .Select(ReadChannelFromFile)
                         .Where(channel => channel != null)
            );

            return channels;
        }

        public void SaveToDisk(string tvhFolder)
        {
            var folder = GetFolder(tvhFolder);
            if (!Directory.Exists(folder))
            {
                Logger.DebugFormat("Folder doesn't exist, create {0}", folder);
                Directory.CreateDirectory(folder);
            }
            var file = Path.Combine(folder, Id);
            var json = TvhJsonConvert.Serialize(this);
            Logger.DebugFormat("Generated json: {0} for {1}", json, file);

            State = File.Exists(file) ? State.Updated : State.Created;
            File.WriteAllText(file, json);
            Logger.DebugFormat("Written json to file {0} ({1})", file, State);
        }

        private static Channel ReadChannelFromFile(string file)
        {
            try
            {
                var json = File.ReadAllText(file);
                var channel = JsonConvert.DeserializeObject<Channel>(json);
                channel.Id = file.Split(Path.DirectorySeparatorChar).Last();
                channel.State = State.Loaded;
                return channel;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to read and parse channel data from {0}", file);
                return null;
            }
        }

        private static string GetFolder(string tvhFolder)
        {
            return Path.Combine(tvhFolder, "channel", "config");
        }

        public void AddTag(Tag tvhTag)
        {
            if (!tags.Contains(tvhTag.Id))
            {
                tags.Add(tvhTag.Id);
            }
        }

        public void AddService(Service service)
        {
            if (!services.Contains(service.Id))
            {
                services.Add(service.Id);
            }
        }
    }
}

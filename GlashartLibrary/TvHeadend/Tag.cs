using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlashartLibrary.TvHeadend
{
    public class Tag
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Tag));

        [JsonIgnore]
        public string Id { get; set; }
        [JsonIgnore]
        public State State { get; private set; }

        /*TvHeadend properties*/
        public bool enabled { get; set; }
        public int index { get; set; }
        public string name { get; set; }
        public bool @internal { get; set; }
        public bool @private { get; set; }
        public string icon { get; set; }
        public bool titled_icon { get; set; }
        public string comment { get; set; }

        /*Tvheadend extra properties*/
        [JsonExtensionData]
        public IDictionary<string, JToken> _additionalData;

        public Tag()
        {
            Id = Guid.NewGuid().ToString();
            enabled = true;
            index = -1;
            name = string.Empty;
            @internal = false;
            @private = false;
            icon = string.Empty;
            titled_icon = false;
            comment = string.Empty;
            State = State.New;
        }

        public static List<Tag> ReadFromDisk(string tvhFolder)
        {
            var channels = new List<Tag>();
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

        private static Tag ReadChannelFromFile(string file)
        {
            try
            {
                var json = File.ReadAllText(file);
                var channel = JsonConvert.DeserializeObject<Tag>(json);
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
            return Path.Combine(tvhFolder, "channel", "tag");
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlashartLibrary.TvHeadend
{
    public class Mux
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Mux));

        [JsonIgnore]
        public string Id { get; set; }
        [JsonIgnore]
        public List<Service> Services { get; set; }
        [JsonIgnore]
        public State State { get; private set; }

        /*TvHeadend properties*/
        public string iptv_url { get; set; }
        public string iptv_interface { get; set; }
        public bool? iptv_atsc { get; set; }
        public bool? iptv_respawn { get; set; }
        public bool? enabled { get; set; }
        public int? epg { get; set; }
        public int? scan_result { get; set; }

        /*Tvheadend extra properties*/
        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        public Mux()
        {
            Id = Guid.NewGuid().ToString();
            Services = new List<Service>();

            iptv_atsc = false;
            iptv_respawn = false;
            enabled = true;
            scan_result = 1;
            epg = 1;
            State = State.New;
        }

        public static Mux ReadFromDisk(string folder)
        {
            Logger.DebugFormat("Read mux from {0}", folder);
            var config = GetFileName(folder);
            if (!File.Exists(config))
            {
                Logger.WarnFormat("Mux config file ({0}) doesn't exist", config);
                return null;
            }

            var configJson = File.ReadAllText(config);
            Logger.DebugFormat("Parse mux config json: {0}", configJson);
            try
            {
                var mux = JsonConvert.DeserializeObject<Mux>(configJson);
                mux.Id = folder.Split(Path.DirectorySeparatorChar).Last();
                mux.State = State.Loaded;
                ReadServices(mux, GetServicesFolder(folder));
                return mux;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to Deserialize mux from json: {0}", configJson);
                return null;
            }
        }

        public void SaveToDisk(string networkFolder)
        {
            var folder = Path.Combine(networkFolder, Id);
            if (!Directory.Exists(folder))
            {
                Logger.DebugFormat("Folder doesn't exist, create {0}", folder);
                Directory.CreateDirectory(folder);
            }
            var file = GetFileName(folder);
            var json = TvhJsonConvert.Serialize(this);
            Logger.DebugFormat("Generated json: {0} for {1}", json, file);

            State = File.Exists(file) ? State.Updated : State.Created;
            File.WriteAllText(file, json);
            Logger.DebugFormat("Written json to file {0} ({1})", file, State);

            Services.ForEach(s => s.SaveToDisk(GetServicesFolder(folder)));
        }

        private static void ReadServices(Mux mux, string folder)
        {
            Logger.DebugFormat("Read services for mux {0} ({1}) from disk", mux.Id, mux.iptv_url);
            mux.Services.AddRange(
                Directory.EnumerateFiles(folder)
                         .Select(Service.ReadFromDisk)
                         .Where(service => service != null)
            );
        }

        private static string GetFileName(string folder)
        {
            return Path.Combine(folder, "config");
        }

        private static string GetServicesFolder(string folder)
        {
            return Path.Combine(folder, "services");
        }

        public Service ResolveService(string name)
        {
            return Services.First(s => s.svcname == name);
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlashartLibrary.TvHeadend
{
    public class Network
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Network));

        [JsonIgnore]
        public string Id { get; set; }
        [JsonIgnore]
        public List<Mux> Muxes { get; set; }
        [JsonIgnore]
        public State State { get; private set; }

        /*TvHeadend properties*/
        public int? priority { get; set; }
        public int? spriority { get; set; }
        public int? max_streams { get; set; }
        public int? max_bandwidth { get; set; }
        public int? max_timeout { get; set; }
        public string networkname { get; set; }
        public int? nid { get; set; }
        public bool? autodiscovery { get; set; }
        public bool? skipinitscan { get; set; }
        public bool? idlescan { get; set; }
        public bool? sid_chnum { get; set; }
        public bool? ignore_chnum { get; set; }
        public int? satip_source { get; set; }
        public bool? localtime { get; set; }

        /*Tvheadend extra properties*/
        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData;

        public Network()
        {
            Id = Guid.NewGuid().ToString();
            Muxes = new List<Mux>();

            priority = 1;
            spriority = 1;
            max_streams = 0;
            max_bandwidth = 0;
            max_timeout = 15;
            nid = 0;
            autodiscovery = true;
            skipinitscan = true;
            idlescan = false;
            sid_chnum = false;
            ignore_chnum = false;
            satip_source = 0;
            localtime = false;
            State = State.New;
        }

        public void SaveToDisk(string tvhFolder)
        {
            var folder = Path.Combine(GetFolder(tvhFolder), Id);
            if (!Directory.Exists(folder))
            {
                Logger.DebugFormat("Folder doesn't exist, create {0}", folder);
                Directory.CreateDirectory(folder);
            }
            var file = GetFileName(folder);
            var json = TvhJsonConvert.Serialize(this);
            Logger.DebugFormat("Generated json: {0} for {1}", json, file);

            State = File.Exists(file) ? State.Updated : State.Created;
            File.WriteAllText(file,json);
            Logger.DebugFormat("Written json to file {0} ({1})", file, State);

            Muxes.ForEach(m => m.SaveToDisk(GetMuxFolder(folder)));
        }

        public static List<Network> ReadFromDisk(string tvhFolder)
        {
            var networks = new List<Network>();
            var networksFolder = GetFolder(tvhFolder);
            if (!Directory.Exists(networksFolder))
            {
                Logger.WarnFormat("Directory {0} doesn't exist", networksFolder);
                return networks;
            }

            networks.AddRange(
                Directory.EnumerateDirectories(networksFolder)
                         .Select(ReadNetworkFromFolder)
                         .Where(network => network != null)
            );

            return networks;
        }

        private static Network ReadNetworkFromFolder(string folder)
        {
            Logger.InfoFormat("Read network from {0}", folder);
            var config = GetFileName(folder);
            if (!File.Exists(config))
            {
                Logger.WarnFormat("Network config file ({0}) doesn't exist", config);
                return null;
            }

            var configJson = File.ReadAllText(config);
            Logger.DebugFormat("Parse network config json: {0}", configJson);
            try
            {
                var network = JsonConvert.DeserializeObject<Network>(configJson);
                network.Id = folder.Split(Path.DirectorySeparatorChar).Last();
                network.Muxes = new List<Mux>();
                network.State = State.Loaded;
                ReadMuxes(network, GetMuxFolder(folder));
                return network;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to Deserialize network from json: {0}", configJson);
                return null;
            }
        }

        private static void ReadMuxes(Network network, string networkFolder)
        {
            Logger.DebugFormat("Read muxes for network {0} ({1}) from disk", network.Id, network.networkname);
            network.Muxes.AddRange(
                Directory.EnumerateDirectories(networkFolder)
                         .Select(Mux.ReadFromDisk)
                         .Where(mux => mux != null)
            );
        }

        private static string GetFolder(string tvhFolder)
        {
            return Path.Combine(tvhFolder, "input", "iptv", "networks");
        }

        private static string GetFileName(string networkFolder)
        {
            return Path.Combine(networkFolder, "config");
        }

        private static string GetMuxFolder(string folder)
        {
            return Path.Combine(folder, "muxes");
        }
    }
}

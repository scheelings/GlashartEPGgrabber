using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace GlashartLibrary.TvHeadend
{
    public enum State { New, Loaded, Created, Updated }

    public class TvhConfiguration
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TvhConfiguration));

        private List<Network> _networks = new List<Network>();
        private List<Channel> _channels = new List<Channel>();
        private List<Tag> _tags = new List<Tag>();
        private string _tvhFolder = string.Empty;
        private string _defaultNetworkName = string.Empty;

        private Network DefaultNetwork
        {
            get
            {
                var network = _networks.FirstOrDefault(n => n.networkname.Equals(_defaultNetworkName, StringComparison.OrdinalIgnoreCase));
                return network ?? CreateNetwork(_defaultNetworkName);
            }
        }

        public static TvhConfiguration ReadFromDisk(string tvhFolder, string defaultNetworkName)
        {
            var config = new TvhConfiguration
            {
                _tvhFolder = tvhFolder,
                _defaultNetworkName = defaultNetworkName,
                _networks = Network.ReadFromDisk(tvhFolder),
                _channels = Channel.ReadFromDisk(tvhFolder),
                _tags = Tag.ReadFromDisk(tvhFolder)
            };
            return config;
        }

        public void SaveToDisk()
        {
            _networks.ForEach(n => n.SaveToDisk(_tvhFolder));
            Logger.InfoFormat("Saved {0} networks to disk ({1} Created; {2} Updated)", _networks.Count, _networks.Count(n => n.State == State.Created), _networks.Count(n => n.State == State.Updated));
            var muxes = _networks.SelectMany(n => n.Muxes).ToList();
            Logger.InfoFormat("Saved {0} muxes to disk ({1} Created; {2} Updated)", muxes.Count, muxes.Count(n => n.State == State.Created), muxes.Count(n => n.State == State.Updated));
            var services = muxes.SelectMany(m => m.Services).ToList();
            Logger.InfoFormat("Saved {0} services to disk ({1} Created; {2} Updated)", services.Count, services.Count(n => n.State == State.Created), services.Count(n => n.State == State.Updated));
            _channels.ForEach(n => n.SaveToDisk(_tvhFolder));
            Logger.InfoFormat("Saved {0} channels to disk ({1} Created; {2} Updated)", _channels.Count, _channels.Count(n => n.State == State.Created), _channels.Count(n => n.State == State.Updated));
            _tags.ForEach(n => n.SaveToDisk(_tvhFolder));
            Logger.InfoFormat("Saved {0} tags to disk ({1} Created; {2} Updated)", _tags.Count, _tags.Count(n => n.State == State.Created), _tags.Count(n => n.State == State.Updated));
        }
        
        public Mux ResolveMux(string name)
        {
            var mux = _networks.SelectMany(n => n.Muxes).FirstOrDefault(m => m.Services.Any(s => s.svcname == name));
            return mux ?? CreateMux(name);
        }

        public Channel ResolveChannel(string name)
        {
            var channel = _channels.FirstOrDefault(c => c.name == name);
            return channel ?? CreateChannel(name);
        }

        public Tag ResolveTag(string name)
        {
            var tag = _tags.FirstOrDefault(c => c.name == name);
            return tag ?? CreateTag(name);
        }

        private Tag CreateTag(string name)
        {
            Logger.InfoFormat("Create new TVH tag for {0}", name);
            var tag = new Tag { name = name };
            _tags.Add(tag);
            return tag;
        }

        private Channel CreateChannel(string name)
        {
            Logger.InfoFormat("Create new TVH channel for {0}", name);
            var channel = new Channel { name = name };
            _channels.Add(channel);
            return channel;
        }

        private Mux CreateMux(string name)
        {
            Logger.InfoFormat("Create new TVH mux with service for {0}", name);
            var mux = new Mux();
            mux.Services.Add(new Service { svcname = name });
            DefaultNetwork.Muxes.Add(mux);
            return mux;
        }

        private Network CreateNetwork(string name)
        {
            var network = new Network { networkname = name };
            _networks.Add(network);
            return network;
        }
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GlashartLibrary.TvHeadend
{
    public class Epg
    {
        [JsonIgnore]
        public string Id { get; set; }
        [JsonIgnore]
        public State State { get; private set; }

        /*TvHeadend properties*/
        public string name { get; set; }
        public string icon { get; set; }
        public List<string> channels { get; set; }

        /*Tvheadend extra properties*/
        [JsonExtensionData]
        public IDictionary<string, JToken> _additionalData;

        public Epg()
        {
            Id = Guid.NewGuid().ToString();
            State = State.New;
        }
    }
}
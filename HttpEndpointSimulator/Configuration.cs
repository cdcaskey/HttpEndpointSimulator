using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HttpEndpointSimulator
{
    public class Configuration
    {
        public string ListenUrl { get; private set; }

        public string BaseLocation { get; private set; }

        public Configuration(string configLocation = "simulatorsettings.json")
        {
            var configFile = File.ReadAllText(configLocation);
            dynamic config = JObject.Parse(configFile);

            ListenUrl = config.listenUrl;
            BaseLocation = config.baseLocation;
        }
    }
}

using Jeno.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Jeno.Services
{
    internal class ConfigurationSerializer : IConfigurationSerializer
    {
        private const string _jenoSection = "Jeno";

        private readonly string _configuratrionPath;
        private Dictionary<string, JenoConfiguration> _configuration;

        public ConfigurationSerializer()
        {
            _configuratrionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            _configuration = JsonConvert.DeserializeObject<Dictionary<string, JenoConfiguration>>(File.ReadAllText(_configuratrionPath));
        }

        public JenoConfiguration ReadConfiguration()
        {
            return _configuration[_jenoSection];
        }

        public bool SaveConfiguration(JenoConfiguration configuration)
        {
            _configuration[_jenoSection] = configuration;
            File.WriteAllText(_configuratrionPath, JsonConvert.SerializeObject(_configuration));
            return true;
        }
    }
}
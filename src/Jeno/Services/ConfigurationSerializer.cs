using Jeno.Core;
using Jeno.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
        }

        public async Task<JenoConfiguration> ReadConfiguration()
        {
            var json = await File.ReadAllTextAsync(_configuratrionPath);
            _configuration = JsonConvert.DeserializeObject<Dictionary<string, JenoConfiguration>>(json);
            return _configuration[_jenoSection];
        }

        public async Task SaveConfiguration(JenoConfiguration configuration)
        {
            _configuration[_jenoSection] = configuration;
            await File.WriteAllTextAsync(_configuratrionPath, JsonConvert.SerializeObject(_configuration));
        }
    }
}
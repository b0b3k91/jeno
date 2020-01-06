using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Jeno.Core;
using Jeno.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Jeno.Services
{
    internal class ConfigurationSerializer : IConfigurationSerializer
    {
        private const string _jenoSection = "jeno";

        private readonly string _configurationPath;
        private Dictionary<string, JenoConfiguration> _configuration;

        public ConfigurationSerializer()
        {
            _configurationPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        }

        public async Task<JenoConfiguration> ReadConfiguration()
        {
            var json = await File.ReadAllTextAsync(_configurationPath);
            _configuration = JsonConvert.DeserializeObject<Dictionary<string, JenoConfiguration>>(json);
            return _configuration[_jenoSection];
        }

        public async Task SaveConfiguration(JenoConfiguration configuration)
        {
            _configuration[_jenoSection] = configuration;

            var serializedConfiguration = JsonConvert.SerializeObject(_configuration, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            });

            await File.WriteAllTextAsync(_configurationPath, serializedConfiguration);
        }
    }
}
using System.Threading.Tasks;

namespace Jeno.Core
{
    internal interface IConfigurationSerializer
    {
        Task<JenoConfiguration> ReadConfiguration();

        Task SaveConfiguration(JenoConfiguration configuration);
    }
}
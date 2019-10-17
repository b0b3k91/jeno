using System.Threading.Tasks;

namespace Jeno.Core
{
    internal interface IConfigurationSerializer
    {
        Task<JenoConfiguration> ReadConfiguration();

        Task<int> SaveConfiguration(JenoConfiguration configuration);
    }
}
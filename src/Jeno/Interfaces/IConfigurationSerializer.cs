using Jeno.Core;
using System.Threading.Tasks;

namespace Jeno.Interfaces
{
    internal interface IConfigurationSerializer
    {
        Task<JenoConfiguration> ReadConfiguration();

        Task SaveConfiguration(JenoConfiguration configuration);
    }
}
using System.Threading.Tasks;
using Jeno.Core;

namespace Jeno.Interfaces
{
    public interface IConfigurationSerializer
    {
        Task<JenoConfiguration> ReadConfiguration();

        Task SaveConfiguration(JenoConfiguration configuration);
    }
}
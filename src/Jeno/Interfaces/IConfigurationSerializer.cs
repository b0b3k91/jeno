using Jeno.Core;
using System.Threading.Tasks;

namespace Jeno.Interfaces
{
    public interface IConfigurationSerializer
    {
        Task<JenoConfiguration> ReadConfiguration();

        Task SaveConfiguration(JenoConfiguration configuration);
    }
}
namespace Jeno.Core
{
    internal interface IConfigurationSerializer
    {
        JenoConfiguration ReadConfiguration();

        bool SaveConfiguration(JenoConfiguration configuration);
    }
}
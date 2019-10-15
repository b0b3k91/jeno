using System;
using System.Collections.Generic;
using System.Text;

namespace Jeno.Core
{
    interface IConfigurationSerializer
    {
        JenoConfiguration ReadConfiguration();

        bool SaveConfiguration(JenoConfiguration configuration);
    }
}

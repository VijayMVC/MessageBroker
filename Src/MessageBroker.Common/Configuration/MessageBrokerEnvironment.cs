using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public enum MessageBrokerEnvironment
    {
        Test,           // Use for unit test testing
        Local,          // Local Service Fabric
        Dark,           // Use for QA testing
        Production,     // Use for production
    }
}

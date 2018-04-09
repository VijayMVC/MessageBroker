using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public enum SettleType
    {
        Processed,          // Message was processed successfully, move message to history
        Rejected,           // Message will never be able to process, move message to history
        Abandon,            // Remove agent lock so other agents can process
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public class InternalQueueStatusV1
    {
        public string QueueName { get; set; }

        public int QueueCount { get; set; }

        public int CurrentSizeLimit { get; set; }

        public int QueueSize { get; set; }
    }
}

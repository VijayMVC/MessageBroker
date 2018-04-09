using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public class InternalQueueConfigurationV1
    {
        public string QueueName { get; set; }

        public int? QueueSize { get; set; }

        public DateTime? LastProcessedDate { get; set; }
    }
}

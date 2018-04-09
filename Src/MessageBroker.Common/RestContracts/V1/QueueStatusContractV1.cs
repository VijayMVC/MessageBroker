using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    [JsonObject]
    public class QueueStatusContractV1
    {
        [JsonProperty("queueName")]
        public string QueueName { get; set; }

        [JsonProperty("queueCount")]
        public int QueueCount { get; set; }

        [JsonProperty("currentSizeLimit")]
        public int CurrentSizeLimit { get; set; }

        [JsonProperty("queueSize")]
        public int QueueSize { get; set; }
    }
}

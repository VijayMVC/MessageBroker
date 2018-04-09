using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    [JsonObject]
    public class SettleMessageContractV1
    {
        [JsonProperty("queueName")]
        public string QueueName { get; set; }

        [JsonProperty("messageId")]
        public long MessageId { get; set; }

        [JsonProperty("agentId")]
        public int AgentId { get; set; }

        [JsonProperty("settleType")]
        public SettleType SettleType { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}

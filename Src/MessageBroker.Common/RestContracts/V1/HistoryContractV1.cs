using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    [JsonObject]
    public class HistoryContractV1
    {
        [JsonProperty("historyId")]
        public long HistoryId { get; set; }

        [JsonProperty("messageId")]
        public long MessageId { get; set; }

        [JsonProperty("activityType")]
        public string ActivityType { get; set; }

        [JsonProperty("queueName")]
        public string QueueName { get; set; }

        [JsonProperty("cv")]
        public string Cv { get; set; }

        [JsonProperty("clientMessageId")]
        public string ClientMessageId { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("settleByAgent")]
        public string SettleByAgent { get; set; }

        [JsonProperty("errorMesage")]
        public string ErrorMesage { get; set; }

        [JsonProperty("retryCount")]
        public int RetryCount { get; set; }

        [JsonProperty("_createdDate")]
        public DateTime _createdDate { get; set; }
    }
}

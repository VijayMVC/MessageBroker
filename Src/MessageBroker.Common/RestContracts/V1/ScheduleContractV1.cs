using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    [JsonObject]
    public class ScheduleContractV1
    {
        [JsonProperty("scheduleId")]
        public long ScheduleId { get; set; }

        [JsonProperty("queueId")]
        public int QueueId { get; set; }

        [JsonProperty("clientMessageId")]
        public string ClientMessageId { get; set; }

        [JsonProperty("cv")]
        public string Cv { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("scheduleDate")]
        public DateTime ScheduleDate { get; set; }

        [JsonProperty("createdByAgentId")]
        public int CreatedByAgentId { get; set; }

        [JsonProperty("_createdDate")]
        public DateTime _createdDate { get; set; }
    }
}

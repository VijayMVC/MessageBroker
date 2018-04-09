using Khooversoft.Toolbox;
using Newtonsoft.Json;
using System;

namespace MessageBroker.Common
{
    [JsonObject]
    public class EnqueueMessageContractV1
    {
        [JsonProperty("queueName")]
        public string QueueName { get; set; }

        [JsonProperty("agentId")]
        public int AgentId { get; set; }

        [JsonProperty("clientMessageId")]
        public string ClientMessageId { get; set; }

        [JsonProperty("cv")]
        public string Cv { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("scheduleDate")]
        public DateTime? ScheduleDate { get; set; }

        public void Serialize<T>(T value) where T : class
        {
            Verify.IsNotNull(nameof(value), value);
            Payload = JsonConvert.SerializeObject(value);
        }

        public T Deserialize<T>() where T : class
        {
            if (Payload.IsEmpty())
            {
                return null;
            }

            return JsonConvert.DeserializeObject<T>(Payload);
        }
    }
}

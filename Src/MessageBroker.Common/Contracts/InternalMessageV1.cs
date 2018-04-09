using Khooversoft.Toolbox;
using Newtonsoft.Json;
using System;

namespace MessageBroker.Common
{
    public class InternalMessageV1
    {
        public long MessageId { get; set; }

        public int QueueId { get; set; }

        public string ClientMessageId { get; set; }

        public string Cv { get; set; }

        public string Payload { get; set; }

        public DateTime? LockedDate { get; set; }

        public int? LockedByAgentId { get; set; }

        public int RetryCount { get; set; }

        public int CreatedByAgentId { get; set; }

        public DateTime _createdDate { get; set; }

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

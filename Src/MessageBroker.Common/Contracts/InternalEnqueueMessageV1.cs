using Khooversoft.Toolbox;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MessageBroker.Common
{
    public class InternalEnqueueMessageV1
    {
        public string QueueName { get; set; }

        public int AgentId { get; set; }

        public string ClientMessageId { get; set; }

        public string Cv { get; set; }

        public string Payload { get; set; }

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

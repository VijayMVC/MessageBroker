using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    [JsonObject]
    public class AgentContractV1
    {
        [JsonProperty("agentId")]
        public int AgentId { get; set; }

        [JsonProperty("agentName")]
        public string AgentName { get; set; }

        [JsonProperty("_createdDate")]
        public DateTime? _createdDate { get; set; }
    }
}

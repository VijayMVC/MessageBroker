using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBrokerTestClient
{
    [JsonObject]
    internal class WorkRequest
    {
        [JsonProperty("processName")]
        public string ProcessName { get; set; }

        [JsonProperty("parameters")]
        public IList<string> Parameters { get; set; }

        [JsonProperty("messageCount")]
        public long MessageCount { get; set; }
    }
}

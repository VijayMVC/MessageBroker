using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    [JsonObject]
    public class HealthCheckContractV1
    {
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}

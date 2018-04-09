using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    [JsonObject]
    public class HistoryDetailContractV1
    {
        [JsonProperty("historySize")]
        public int HistorySize { get; set; }
    }
}

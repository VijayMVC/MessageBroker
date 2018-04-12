// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    [JsonObject]
    public class QueueDetailContractV1
    {
        [JsonProperty("queueId")]
        public int QueueId { get; set; }

        [JsonProperty("queueName")]
        public string QueueName { get; set; }

        [JsonProperty("currentSizeLimit")]
        public int CurrentSizeLimit { get; set; }

        [JsonProperty("currentRetryLimit")]
        public int CurrentRetryLimit { get; set; }

        [JsonProperty("lockValidForSec")]
        public int LockValidForSec { get; set; }

        [JsonProperty("disable")]
        public bool Disable { get; set; }

        [JsonProperty("queueLength")]
        public int? QueueLength { get; set; }

        [JsonProperty("scheduleQueueLength")]
        public int? ScheduleQueueLength { get; set; }
    }
}

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
    public class SetQueueContractV1
    {
        [JsonProperty("queueName")]
        public string QueueName { get; set; }

        [JsonProperty("currentSizeLimit")]
        public int? CurrentSizeLimit { get; set; }

        [JsonProperty("currentRetryLimit")]
        public int? CurrentRetryLimit { get; set; }

        [JsonProperty("lockValidForSec")]
        public int? LockValidForSec { get; set; }

        public void SetDefaults()
        {
            CurrentSizeLimit = CurrentSizeLimit ?? 5000;
            CurrentRetryLimit = CurrentRetryLimit ?? 3;
            LockValidForSec = LockValidForSec ?? 5 * 60;
        }
    }
}

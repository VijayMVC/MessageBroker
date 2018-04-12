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
    public class HealthCheckContractV1
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("repositoryHealthCheck")]
        public string RepositoryHealthCheck { get; set; }
    }
}

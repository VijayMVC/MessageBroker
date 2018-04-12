// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Toolbox;
using Newtonsoft.Json;
using System;

namespace MessageBroker.Common
{
    [JsonObject]
    public class MessageContractV1
    {
        [JsonProperty("messageId")]
        public long MessageId { get; set; }

        [JsonProperty("queueId")]
        public int QueueId { get; set; }

        [JsonProperty("clientMessageId")]
        public string ClientMessageId { get; set; }

        [JsonProperty("cv")]
        public string Cv { get; set; }

        [JsonProperty("payload")]
        public string Payload { get; set; }

        [JsonProperty("lockedDate")]
        public DateTime? LockedDate { get; set; }

        [JsonProperty("lockedByAgentId")]
        public int? LockedByAgentId { get; set; }

        [JsonProperty("retryCount")]
        public int RetryCount { get; set; }

        [JsonProperty("createdByAgentId")]
        public int CreatedByAgentId { get; set; }

        [JsonProperty("_createdDate")]
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

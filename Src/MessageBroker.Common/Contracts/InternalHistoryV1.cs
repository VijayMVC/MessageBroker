// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public class InternalHistoryV1
    {
        public long HistoryId { get; set; }

        public long MessageId { get; set; }

        public string ActivityType { get; set; }

        public string QueueName { get; set; }

        public string Cv { get; set; }

        public string ClientMessageId { get; set; }

        public string Payload { get; set; }

        public string SettleByAgent { get; set; }

        public string ErrorMesage { get; set; }

        public int RetryCount { get; set; }

        public DateTime _createdDate { get; set; }
    }
}

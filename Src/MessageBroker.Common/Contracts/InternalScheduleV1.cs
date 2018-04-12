// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public class InternalScheduleV1
    {
        public long ScheduleId { get; set; }

        public int QueueId { get; set; }

        public string ClientMessageId { get; set; }

        public string Cv { get; set; }

        public string Payload { get; set; }

        public DateTime ScheduleDate { get; set; }

        public int CreatedByAgentId { get; set; }

        public DateTime _createdDate { get; set; }
    }
}

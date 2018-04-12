// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public class InternalQueueManagementV1
    {
        public int QueueId { get; set; }

        public string QueueName { get; set; }

        public int CurrentSizeLimit { get; set; }

        public int CurrentRetryLimit { get; set; }

        public int LockValidForSec { get; set; }

        public bool Disable { get; set; }

        public int? QueueLength { get; set; }

        public int? ScheduleQueueLength { get; set; }
    }
}

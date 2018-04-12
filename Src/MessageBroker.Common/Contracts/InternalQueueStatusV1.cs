// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public class InternalQueueStatusV1
    {
        public string QueueName { get; set; }

        public int QueueCount { get; set; }

        public int CurrentSizeLimit { get; set; }

        public int QueueSize { get; set; }
    }
}

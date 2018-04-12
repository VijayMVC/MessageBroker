// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public class InternalAgentV1
    {
        public int AgentId { get; set; }

        public string AgentName { get; set; }

        public DateTime _createdDate { get; set; }
    }
}

// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    public class MessageEvent
    {
        public MessageEvent(MessageContractV1 contract, CancellationToken token, AgentContractV1 agent)
        {
            Contract = contract;
            Token = token;
            Agent = agent;
        }

        public MessageContractV1 Contract { get; }

        public CancellationToken Token { get; }

        public AgentContractV1 Agent { get; }
    }
}

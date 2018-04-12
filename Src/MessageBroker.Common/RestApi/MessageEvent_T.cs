// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    public class MessageEvent<T> where T : class
    {
        public MessageEvent(T message, MessageContractV1 contract, AgentContractV1 agent, bool useSettle)
        {
            Message = message;
            Contract = contract;
            Agent = agent;
            UseSettle = useSettle;
        }

        public T Message { get; }

        public MessageContractV1 Contract { get; }

        public AgentContractV1 Agent { get; }

        public bool UseSettle { get; }
    }
}

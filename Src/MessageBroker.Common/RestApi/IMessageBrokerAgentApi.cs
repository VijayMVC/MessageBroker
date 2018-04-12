// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Net;
using Khooversoft.Toolbox;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    public interface IMessageBrokerAgentApi
    {
        Task<RestResponse<RestPageResultV1<AgentContractV1>>> GetAgents(IWorkContext context);
    }
}

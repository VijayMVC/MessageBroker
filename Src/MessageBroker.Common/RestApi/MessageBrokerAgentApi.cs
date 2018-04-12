// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Net;
using Khooversoft.Toolbox;
using System;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    /// <summary>
    /// Agent management API
    /// </summary>
    public class MessageBrokerAgentApi : ClientBase, IMessageBrokerAgentApi
    {
        private static readonly Tag _tag = new Tag(nameof(MessageBrokerAgentApi));

        public MessageBrokerAgentApi(Uri baseUri, IRestClientConfiguration restClientConfiguration)
            : base(baseUri, restClientConfiguration)
        {
        }

        /// <summary>
        /// Get a list of agents, no paging is currently supported so single list, maybe truncated
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>response</returns>
        public async Task<RestResponse<RestPageResultV1<AgentContractV1>>> GetAgents(IWorkContext context)
        {
            Verify.IsNotNull(nameof(context), context);
            context = context.WithTag(_tag);

            return await CreateClient()
                .GetAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context)
                .GetContentAsync<RestPageResultV1<AgentContractV1>>(context);
        }

        protected override RestClient CreateClient()
        {
            return base.CreateClient()
                .AddPath("v1/Agent");
        }
    }
}

// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Net;
using Khooversoft.Toolbox;
using System;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    /// <summary>
    /// REST Api for message broker history
    /// </summary>
    public class MessageBrokerHistoryApi : ClientBase, IMessageBrokerHistoryApi
    {
        private static readonly Tag _tag = new Tag(nameof(MessageBrokerAgentApi));

        public MessageBrokerHistoryApi(Uri baseUri, IRestClientConfiguration restClientConfiguration)
            : base(baseUri, restClientConfiguration)
        {
        }

        /// <summary>
        /// Get history for message ID
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="messageId">message id</param>
        /// <returns>response</returns>
        public async Task<RestResponse<HistoryContractV1>> GetHistory(IWorkContext context, long messageId)
        {
            Verify.IsNotNull(nameof(context), context);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath(messageId.ToString())
                .GetAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context)
                .GetContentAsync<HistoryContractV1>(context);
        }

        protected override RestClient CreateClient()
        {
            return base.CreateClient()
                .AddPath("v1/History");
        }
    }
}

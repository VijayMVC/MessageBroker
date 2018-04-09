using Khooversoft.Net;
using Khooversoft.Toolbox;
using System;
using System.Net;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    /// <summary>
    /// Rest API for message broker for queue management functions.
    /// </summary>
    public class MessageBrokerManagementApi : ClientBase, IMessageBrokerManagementApi
    {
        private static readonly Tag _tag = new Tag(nameof(MessageBrokerManagementApi));

        public MessageBrokerManagementApi(Uri baseUri, IRestClientConfiguration restClientConfiguration)
            : base(baseUri, restClientConfiguration)
        {
        }

        /// <summary>
        /// Create or update queue
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="contract">request details</param>
        /// <returns>status</returns>
        public async Task<RestResponse> SetQueue(IWorkContext context, SetQueueContractV1 contract)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotNull(nameof(contract), contract);
            Verify.IsNotEmpty(nameof(contract.QueueName), contract.QueueName);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath(contract.QueueName)
                .SetContent(contract)
                .PutAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context);
        }

        /// <summary>
        /// Clear queue, mark all active messages deleted
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="queueName">name of queue</param>
        /// <param name="copyToHistory">copy active records to history (default = false)</param>
        /// <returns>status</returns>
        public async Task<RestResponse> ClearQueue(IWorkContext context, string queueName, bool copyToHistory = false)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotEmpty(nameof(queueName), queueName);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath(queueName)
                .AddPath("clear-queue")
                .AddQuery(nameof(copyToHistory), copyToHistory.ToString())
                .PostAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context);
        }

        /// <summary>
        /// Delete a queue, all active messages are marked deleted and queue is marked deleted
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="queueName">queue name</param>
        /// <returns>status</returns>
        public async Task<RestResponse> DeleteQueue(IWorkContext context, string queueName)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotEmpty(nameof(queueName), queueName);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath(queueName)
                .DeleteAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context);
        }

        /// <summary>
        /// Disable queue, queue cannot accept new messages, or retrieve messages
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="queueName">queue name</param>
        /// <returns>response</returns>
        public async Task<RestResponse> DisableQueue(IWorkContext context, string queueName)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotEmpty(nameof(queueName), queueName);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath(queueName)
                .AddPath("disable-queue")
                .PostAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context);
        }

        /// <summary>
        /// Enable a queue, queue can accept new messages and dequeue operations
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="queueName">queue name</param>
        /// <returns>response</returns>
        public async Task<RestResponse> EnableQueue(IWorkContext context, string queueName)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotEmpty(nameof(queueName), queueName);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath(queueName)
                .AddPath("enable-queue")
                .PostAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context);
        }

        /// <summary>
        /// Get queue details
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="queueName">queue name</param>
        /// <returns>response</returns>
        public async Task<RestResponse<QueueDetailContractV1>> GetQueue(IWorkContext context, string queueName, HttpStatusCode[] acceptedCodes = null)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotEmpty(nameof(queueName), queueName);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath(queueName)
                .GetAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context, acceptedCodes: acceptedCodes)
                .GetContentAsync<QueueDetailContractV1>(context);
        }

        /// <summary>
        /// Get list of queues, active or disabled
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="disable">true to return disable queues</param>
        /// <returns>response</returns>
        public async Task<RestResponse<RestPageResultV1<QueueDetailContractV1>>> GetQueueList(IWorkContext context, bool disable = false)
        {
            Verify.IsNotNull(nameof(context), context);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddQuery(nameof(disable), disable.ToString())
                .GetAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context)
                .GetContentAsync<RestPageResultV1<QueueDetailContractV1>>(context);
        }

        /// <summary>
        /// Get queue status - return queue count and queue size for all queues
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<RestResponse<RestPageResultV1<QueueStatusContractV1>>> GetQueueStatus(IWorkContext context)
        {
            Verify.IsNotNull(nameof(context), context);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath("status")
                .GetAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context)
                .GetContentAsync<RestPageResultV1<QueueStatusContractV1>>(context);
        }

        protected override RestClient CreateClient()
        {
            return base.CreateClient()
                .AddPath("v1/management");
        }
    }
}

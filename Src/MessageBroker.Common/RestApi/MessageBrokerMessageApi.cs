using Khooversoft.Net;
using Khooversoft.Toolbox;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    /// <summary>
    /// REST API for managing and posting messages
    /// </summary>
    public class MessageBrokerMessageApi : ClientBase, IMessageBrokerMessageApi
    {
        private static readonly Tag _tag = new Tag(nameof(MessageBrokerMessageApi));

        public MessageBrokerMessageApi(Uri baseUri, IRestClientConfiguration restClientConfiguration)
            : base(baseUri, restClientConfiguration)
        {
        }

        /// <summary>
        /// Enqueue a message
        /// 
        /// If the "ScheduleDate" of the contract is specified, this will schedule a message
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="queueName">queue name</param>
        /// <param name="contract">contract for the message</param>
        /// <returns>200 will return message id (long), all other codes are errors</returns>
        public async Task<RestResponse<EnqueuedContractV1>> EnqueueMessage(IWorkContext context, EnqueueMessageContractV1 contract)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotNull(nameof(contract), contract);
            Verify.IsNotEmpty(nameof(contract.QueueName), contract.QueueName);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath(contract.QueueName)
                .AddPath("message/head")
                .SetContent(contract)
                .PostAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context)
                .GetContentAsync<EnqueuedContractV1>(context);
        }

        /// <summary>
        /// Dequeue a message if one is available and mark it processed
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="queueName">queue name</param>
        /// <param name="waitFor">wait for message (should be greater than 30 seconds)</param>
        /// <param name="token">cancellation token (optional)</param>
        /// <returns>200 for message retrieved, 204 if no messages are available</returns>
        public async Task<RestResponse<MessageContractV1>> DequeueMessageAndDelete(IWorkContext context, string queueName, TimeSpan? waitFor = null, CancellationToken? token = null)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotEmpty(nameof(queueName), queueName);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath(queueName)
                .AddPath("message/head")
                .AddQuery("waitMs", waitFor?.TotalMilliseconds.ToString())
                .DeleteAsync(context, token)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context)
                .GetContentAsync<MessageContractV1>(context);
        }

        /// <summary>
        /// Dequeue a message if one is available and lock it for the agent
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="queueName">queue name</param>
        /// <param name="waitFor">wait for message (should be greater than 30 seconds)</param>
        /// <param name="token">cancellation token (optional)</param>
        /// <returns>200 for message retrieved, 204 if no messages are available</returns>
        public async Task<RestResponse<MessageContractV1>> DequeueMessageWithLock(IWorkContext context, string queueName, AgentContractV1 agentContract, TimeSpan? waitFor = null, CancellationToken? token = null)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotEmpty(nameof(queueName), queueName);
            Verify.IsNotNull(nameof(agentContract), agentContract);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath(queueName)
                .AddPath("message/head")
                .AddQuery("agentId", agentContract.AgentId.ToString())
                .AddQuery("waitMs", waitFor?.TotalMilliseconds.ToString())
                .GetAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context)
                .GetContentAsync<MessageContractV1>(context);
        }

        /// <summary>
        /// Settle a message that is locked by an agent
        /// </summary>
        /// <param name="context"></param>
        /// <param name="contract"></param>
        /// <returns>response</returns>
        public async Task<RestResponse> SettleMessage(IWorkContext context, SettleMessageContractV1 contract)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotNull(nameof(contract), contract);
            Verify.IsNotEmpty(nameof(contract.QueueName), contract.QueueName);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath(contract.QueueName)
                .AddPath("message")
                .AddPath(contract.MessageId.ToString())
                .SetContent(contract)
                .PostAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context);
        }

        /// <summary>
        /// Get messages that have been schedule for a queue (no paging is current implemented, single response with limit)
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="queueName">queue name</param>
        /// <returns>response</returns>
        public async Task<RestResponse<RestPageResultV1<MessageContractV1>>> GetActiveMessages(IWorkContext context, string queueName)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotEmpty(nameof(queueName), queueName);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath(queueName)
                .AddPath("message")
                .GetAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context)
                .GetContentAsync<RestPageResultV1<MessageContractV1>>(context);
        }

        /// <summary>
        /// Get messages that have been schedule for a queue (no paging is current implemented, single response with limit)
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="queueName">queue name</param>
        /// <returns>response</returns>
        public async Task<RestResponse<RestPageResultV1<ScheduleContractV1>>> GetMessageSchedules(IWorkContext context, string queueName)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotEmpty(nameof(queueName), queueName);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath(queueName)
                .AddPath("schedule")
                .GetAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context)
                .GetContentAsync<RestPageResultV1<ScheduleContractV1>>(context);
        }

        /// <summary>
        /// Delete message schedule
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="queueName">queue name</param>
        /// <param name="scheduleId">schedule id</param>
        /// <returns>response</returns>
        public Task<RestResponse> DeleteMessageSchedule(IWorkContext context, string queueName, long scheduleId)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotEmpty(nameof(queueName), queueName);
            context = context.WithTag(_tag);

            return CreateClient()
                .AddPath(queueName)
                .AddPath("schedule")
                .AddPath(scheduleId.ToString())
                .DeleteAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context);
        }

        /// <summary>
        /// Get agent ID (agent registration is created if it does not exist)
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="queueName">queue name</param>
        /// <param name="agentName">agent name</param>
        /// <returns>agent details</returns>
        public async Task<RestResponse<AgentContractV1>> GetAgentId(IWorkContext context, string queueName, string agentName)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotEmpty(nameof(queueName), queueName);
            Verify.IsNotEmpty(nameof(agentName), agentName);

            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath(queueName)
                .AddPath("agent")
                .AddPath(agentName)
                .GetAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context)
                .GetContentAsync<AgentContractV1>(context);
        }

        protected override RestClient CreateClient()
        {
            return base.CreateClient()
                .AddPath("v1/message");
        }
    }
}

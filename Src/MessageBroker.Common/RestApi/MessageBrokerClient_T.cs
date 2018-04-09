using Khooversoft.Net;
using Khooversoft.Toolbox;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    /// <summary>
    /// Message client for a specific payload for basic functionality, Enqueue and dequeue processing
    /// 
    /// Message must support JSON serialization
    /// </summary>
    /// <typeparam name="T">Payload type</typeparam>
    public class MessageBrokerClient<T> : IMessageBrokerClient<T>, IMessageBrokerClientConfiguration where T : class
    {
        private AgentContractV1 _agent;
        private readonly Tag _tag = new Tag(nameof(MessageBrokerClient<T>));
        private bool _enabledQueue = false;

        /// <summary>
        /// Creates a message broker client
        /// </summary>
        /// <param name="baseUri">URI for the message broker service</param>
        /// <param name="agentName">agent's name</param>
        /// <param name="queueName">queue to use</param>
        public MessageBrokerClient(Uri baseUri, string agentName, string queueName)
        {
            Verify.IsNotNull(nameof(baseUri), baseUri);
            Verify.IsNotEmpty(nameof(agentName), agentName);
            Verify.IsNotEmpty(nameof(queueName), queueName);

            BaseUri = baseUri;
            AgentName = agentName;
            QueueName = queueName;

            IRestClientConfiguration restClient = RestClientConfiguration.Default;
            Client = new MessageBrokerClient(this, restClient);
        }

        /// <summary>
        /// Creates a message broker client
        /// </summary>
        /// <param name="baseUri">URI for the message broker service</param>
        /// <param name="agentName">agent's name</param>
        /// <param name="queueName">queue to use</param>
        public MessageBrokerClient(Uri baseUri, string agentName, string queueName, IRestClientConfiguration restClient)
        {
            Verify.IsNotNull(nameof(baseUri), baseUri);
            Verify.IsNotEmpty(nameof(agentName), agentName);
            Verify.IsNotEmpty(nameof(queueName), queueName);
            Verify.IsNotNull(nameof(restClient), restClient);

            BaseUri = baseUri;
            AgentName = agentName;
            QueueName = queueName;

            Client = new MessageBrokerClient(this, restClient);
        }

        /// <summary>
        /// Base URI for the Message Broker service
        /// </summary>
        public Uri BaseUri { get; }

        /// <summary>
        /// Agent's name
        /// </summary>
        public string AgentName { get; }

        /// <summary>
        /// Queue name to use
        /// </summary>
        public string QueueName { get; }

        /// <summary>
        /// Full client
        /// </summary>
        public IMessageBrokerClient Client { get; }

        /// <summary>
        /// Create new broker client with new agent name
        /// </summary>
        /// <param name="agentName">new agent name</param>
        /// <returns>new message broker client</returns>
        public IMessageBrokerClient<T> WithAgent(string agentName)
        {
            Verify.IsNotEmpty(nameof(agentName), agentName);

            return new MessageBrokerClient<T>(BaseUri, agentName, QueueName);
        }

        /// <summary>
        /// Enqueue a message
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="message">message to send</param>
        /// <returns>task</returns>
        public async Task Enqueue(IWorkContext context, T message)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotNull(nameof(message), message);
            context = context.WithTag(_tag);

            await EnableQueue(context);

            EnqueueMessageContractV1 request = await CreateMessage(context, message);
            RestResponse<EnqueuedContractV1> response = await Client.Message.EnqueueMessage(context, request);
            Verify.IsNotNull(nameof(response), response);
        }

        /// <summary>
        /// Dequeue message, no wait
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="useSettle">use settle operations, true messages are locked and must be settled, false = read and delete (implicit)</param>
        /// <returns>message event or null for no message available</returns>
        public async Task<MessageEvent<T>> Dequeue(IWorkContext context, bool useSettle = true)
        {
            AgentContractV1 agent = await GetAgent(context);
            RestResponse<MessageContractV1> readMessage = null;

            await EnableQueue(context);

            if (!useSettle)
            {
                readMessage = await Client.Message.DequeueMessageAndDelete(context, QueueName);
            }
            else
            {
                readMessage = await Client.Message.DequeueMessageWithLock(context, QueueName, agent);
            }

            if (readMessage == null || readMessage.StatusCode == HttpStatusCode.NoContent)
            {
                return null;
            }

            return new MessageEvent<T>(readMessage.Value.Deserialize<T>(), readMessage.Value, agent, useSettle);
        }

        /// <summary>
        /// Settle a message
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="messageId">message id</param>
        /// <param name="settleType">settle type</param>
        /// <param name="errorMessage">error message</param>
        /// <returns>task</returns>
        public async Task Settle(IWorkContext context, long messageId, SettleType settleType, string errorMessage = null)
        {
            await SettleMessage(context, messageId, settleType, errorMessage);
        }

        /// <summary>
        /// Settle a message based on message event
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="messageEvent">message event</param>
        /// <param name="settleType">settle type</param>
        /// <param name="errorMessage">error message (optional)</param>
        /// <returns></returns>
        public async Task Settle(IWorkContext context, MessageEvent<T> messageEvent, SettleType settleType, string errorMessage = null)
        {
            Verify.IsNotNull(nameof(messageEvent), messageEvent);
            Verify.Assert(messageEvent.UseSettle, "Message did not require settlement");

            await SettleMessage(context, messageEvent.Contract.MessageId, settleType, errorMessage);
        }

        /// <summary>
        /// Process dequeue operations, monitors for new messages and calls process message lambda.
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="processMessage">lambda to call when a message is received</param>
        /// <param name="token">cancellation token, this cancel's the processing loop</param>
        /// <param name="useSettle">use settle operations, true messages are locked and must be settled, false = read and delete (implicit)</param>
        /// <param name="wait">true for long polling</param>
        /// <returns>task</returns>
        public async Task ProcessDequeue(IWorkContext context, Func<IWorkContext, T, MessageEvent, Task<bool>> processMessage, CancellationToken token, bool useSettle = true, bool wait = true)
        {
            Verify.IsNotNull(nameof(processMessage), processMessage);
            context = context.WithTag(_tag);

            await EnableQueue(context);

            RestResponse<MessageContractV1> readMessage = null;
            AgentContractV1 agent = await GetAgent(context);
            TimeSpan? waitFor = wait ? TimeSpan.FromSeconds(15) : (TimeSpan?)null;

            while (!token.IsCancellationRequested)
            {
                if (!useSettle)
                {
                    readMessage = await Client.Message.DequeueMessageAndDelete(context, QueueName, waitFor, token);
                }
                else
                {
                    readMessage = await Client.Message.DequeueMessageWithLock(context, QueueName, agent, waitFor, token);
                }

                if (readMessage == null || readMessage.StatusCode == HttpStatusCode.NoContent)
                {
                    readMessage = null;
                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(500), token);
                    }
                    catch
                    {
                        return;
                    }

                    continue;
                }

                try
                {
                    var messageEvent = new MessageEvent(readMessage.Value, token, agent);
                    bool success = await processMessage(context, readMessage.Value.Deserialize<T>(), messageEvent);

                    if (useSettle)
                    {
                        await SettleMessage(context, readMessage.Value.MessageId, success ? SettleType.Processed : SettleType.Rejected);
                    }
                }
                catch (Exception ex)
                {
                    ToolboxEventSource.Log.Error(context, "Exception raised", ex);

                    if (useSettle && readMessage != null && readMessage.StatusCode == HttpStatusCode.OK)
                    {
                        await SettleMessage(context, readMessage.Value.MessageId, SettleType.Rejected, ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Get cached agent or ask the service
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>agent contract</returns>
        private async Task<AgentContractV1> GetAgent(IWorkContext context)
        {
            if (_agent != null)
            {
                return _agent;
            }

            return _agent = (await Client.Message.GetAgentId(context, QueueName, AgentName)).Value;
        }

        /// <summary>
        /// Enable queue, insure the queue exist
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>task</returns>
        private async Task EnableQueue(IWorkContext context)
        {
            if (_enabledQueue)
            {
                return;
            }

            RestResponse<QueueDetailContractV1> result = await Client.Management.GetQueue(context, QueueName, new HttpStatusCode[] { HttpStatusCode.NotFound });
            if (result.StatusCode == HttpStatusCode.OK)
            {
                return;
            }

            var contract = new SetQueueContractV1
            {
                QueueName = QueueName,
            };

            await Client.Management.SetQueue(context, contract);
            _enabledQueue = true;
        }

        /// <summary>
        /// Create message with GUID as client message id
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="message">message to serialize</param>
        /// <returns>message contract</returns>
        private async Task<EnqueueMessageContractV1> CreateMessage(IWorkContext context, T message)
        {
            Verify.IsNotNull(nameof(message), message);

            AgentContractV1 agent = await GetAgent(context);

            return new EnqueueMessageContractV1
            {
                QueueName = QueueName,
                AgentId = agent.AgentId,
                ClientMessageId = Guid.NewGuid().ToString(),
                Cv = context.Cv,
                Payload = JsonConvert.SerializeObject(message),
            };
        }

        /// <summary>
        /// Settle message for Dequeue and lock operations
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="messageId">message id</param>
        /// <param name="settleType">settle type</param>
        /// <param name="errorMessage">error message (optional)</param>
        /// <returns>task</returns>
        private async Task SettleMessage(IWorkContext context, long messageId, SettleType settleType, string errorMessage = null)
        {
            AgentContractV1 agent = await GetAgent(context);

            var settleMessage = new SettleMessageContractV1
            {
                AgentId = agent.AgentId,
                QueueName = QueueName,
                SettleType = settleType,
                MessageId = messageId,
                ErrorMessage = errorMessage

            };

            await Client.Message.SettleMessage(context, settleMessage);
        }
    }
}

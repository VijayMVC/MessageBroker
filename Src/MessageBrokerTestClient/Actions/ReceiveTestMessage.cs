// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Net;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using MessageBroker.Common.RestApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBrokerTestClient
{
    internal class ReceiveTestMessage
    {
        private readonly IOptions _options;
        private readonly IMessageBrokerClient _client;
        private long _highMessageCount;
        private readonly object _lock = new object();

        public ReceiveTestMessage(IOptions options, IMessageBrokerClient client)
        {
            Verify.IsNotNull(nameof(options), options);
            Verify.IsNotNull(nameof(client), client);

            _options = options;
            _client = client;
        }

        public async Task Run(IWorkContext context, CancellationToken token)
        {
            Console.WriteLine($"Running {_options.ClientCount} clients");

            await VerifyQueue(context, token);
            if (token.IsCancellationRequested)
            {
                return;
            }

            using (var report = new MonitorReport("Monitor", _options))
            {
                var tasks = new List<Task>();

                foreach (var clientNumber in Enumerable.Range(0, _options.ClientCount))
                {
                    tasks.Add(Task.Run(() => RandomClient(context, report, clientNumber, token)));
                }

                Task.WaitAll(tasks.ToArray());
            }
        }

        /// <summary>
        /// Verify that queue is provisioned, the senders will create the queue
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="token">token</param>
        /// <returns>task</returns>
        private async Task VerifyQueue(IWorkContext context, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                RestResponse<QueueDetailContractV1> result = await _client.Management.GetQueue(context, _options.QueueName, new HttpStatusCode[] { HttpStatusCode.NotFound });
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return;
                }

                // Wait and try again
                Console.WriteLine("Waiting for Queue to be constructed");
                await Utility.Delay(TimeSpan.FromSeconds(1), token);
            }
        }

        private async Task RandomClient(IWorkContext context, MonitorReport report, int clientNumber, CancellationToken token)
        {
            string agentName = $"{_options.AgentName}_Receive_{clientNumber}";
            AgentContractV1 agent = (await _client.Message.GetAgentId(context, _options.QueueName, agentName)).Value;

            TimeSpan? waitFor = _options.NoWait ? (TimeSpan?)null : TimeSpan.FromSeconds(30);

            using (var monitor = new MonitorRate(report, agentName))
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        RestResponse<MessageContractV1> readMessage;

                        if (_options.NoLock)
                        {
                            readMessage = await _client.Message.DequeueMessageAndDelete(context, _options.QueueName, waitFor, token);
                        }
                        else
                        {
                            readMessage = await _client.Message.DequeueMessageWithLock(context, _options.QueueName, agent, waitFor, token);
                        }

                        if (readMessage.StatusCode == HttpStatusCode.NoContent)
                        {
                            monitor.AddRetry();
                            await Utility.Delay(TimeSpan.FromMilliseconds(500), token);
                            continue;
                        }

                        WorkRequest workRequest = readMessage.Value.Deserialize<WorkRequest>();

                        lock (_lock)
                        {
                            if (workRequest.MessageCount > _highMessageCount)
                            {
                                _highMessageCount = workRequest.MessageCount;
                            }
                        }

                        await Utility.Delay(_options.Delay, token);

                        if (!_options.NoLock)
                        {
                            var settleMessage = new SettleMessageContractV1
                            {
                                AgentId = agent.AgentId,
                                QueueName = _options.QueueName,
                                SettleType = SettleType.Processed,
                                MessageId = readMessage.Value.MessageId,
                            };

                            await _client.Message.SettleMessage(context, settleMessage);
                        }

                        monitor.IncrementRead();
                    }
                    catch (Exception ex)
                    {
                        monitor.IncrementError(ex.ToString());
                    }
                }
            }
        }
    }
}

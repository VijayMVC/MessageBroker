// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Net;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using MessageBroker.Common.RestApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBrokerTestClient
{
    internal class SendTestMessages
    {
        private readonly IOptions _options;
        private readonly IMessageBrokerClient _client;
        private readonly Random _random = new Random();
        private long _messageCount = 0;

        public SendTestMessages(IOptions options, IMessageBrokerClient client)
        {
            Verify.IsNotNull(nameof(options), options);
            Verify.IsNotNull(nameof(client), client);

            _options = options;
            _client = client;
        }

        public async Task Run(IWorkContext context, CancellationToken token)
        {
            Console.WriteLine($"Running {_options.ClientCount} clients");

            await SetupQueue(context);

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

        private async Task SetupQueue(IWorkContext context)
        {
            var contract = new SetQueueContractV1
            {
                QueueName = _options.QueueName,
            };

            await _client.Management.SetQueue(context, contract);
        }

        private async Task RandomClient(IWorkContext context, MonitorReport report, int clientNumber, CancellationToken token)
        {
            string agentName = $"{_options.AgentName}_Send_{clientNumber}";
            AgentContractV1 agent = (await _client.Message.GetAgentId(context, _options.QueueName, agentName)).Value;

            using (var monitor = new MonitorRate(report, agentName))
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        EnqueueMessageContractV1 request = CreateMessage(context, agent.AgentId, _options.QueueName);
                        RestResponse<EnqueuedContractV1> response = await _client.Message.EnqueueMessage(context, request);

                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            monitor.IncrementError();
                            continue;
                        }

                        monitor.IncrementNew();

                        await Utility.Delay(_options.Delay, token);
                    }
                    catch
                    {
                        monitor.IncrementError();
                    }
                }
            }
        }

        private EnqueueMessageContractV1 CreateMessage(IWorkContext context, int agentId, string queueName)
        {
            var workRequest = new WorkRequest
            {
                ProcessName = "this process",
                Parameters = new List<string>(new string[] { "P1='1'", "p2=3", "p4=3" }),
                MessageCount = Interlocked.Increment(ref _messageCount),
            };

            return new EnqueueMessageContractV1
            {
                QueueName = queueName,
                AgentId = agentId,
                ClientMessageId = Guid.NewGuid().ToString(),
                Cv = context.Cv,
                Payload = JsonConvert.SerializeObject(workRequest),
            };
        }
    }
}

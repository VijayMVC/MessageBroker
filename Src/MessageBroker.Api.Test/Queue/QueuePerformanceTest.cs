// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Toolbox;
using MessageBroker.Api.Test.Application;
using MessageBroker.Common;
using MessageBroker.Common.RestApi;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MessageBroker.Api.Test.Queue
{
    [Collection("General")]
    public class QueuePerformanceTest
    {
        private const string _agentName = "agent1";
        private static readonly string _queueName = nameof(QueuePerformanceTest);
        private readonly Tag _tag = new Tag(nameof(QueueTests));
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private long _writeCounter = 0;
        private long _readCount = 0;
        private MessageBrokerClient _client;
        private Utility _utility;

        public QueuePerformanceTest()
        {
            _client = new MessageBrokerClient(TestAssembly.ClientConfiguration, TestAssembly.GetRestClientConfiguration());
            _utility = new Utility(_client);

            ResetDatabase()
                .GetAwaiter()
                .GetResult();
        }

        private async Task ResetDatabase()
        {
            var context = _workContext.WithTag(_tag);

            await _client.Administration.ResetDatabase(context);
        }

        [Fact]
        public async Task QueueReadWriteTestApi()
        {
            var context = _workContext.WithTag(_tag);

            AgentContractV1 agent = await _utility.SetupAgentAndQueue(_queueName, _agentName);

            var tokenSource = new CancellationTokenSource();
            var taskList = new List<Task>();
            taskList.Add(Task.Run(() => ReadQueue(context, tokenSource.Token)));
            taskList.Add(Task.Run(() => WriteQueue(context, agent.AgentId, tokenSource.Token)));

            await Task.Delay(TimeSpan.FromSeconds(20));

            tokenSource.Cancel();
            Task.WaitAll(taskList.ToArray());
        }

        private async Task WriteQueue(IWorkContext context, int agentId, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10), token);
                }
                catch
                {
                    continue;
                }

                EnqueueMessageContractV1 message = _utility.CreateMessage(context, agentId, _queueName);
                await _client.Message.EnqueueMessage(context, message);

                Interlocked.Increment(ref _writeCounter);
            }
        }

        private async Task ReadQueue(IWorkContext context, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                MessageContractV1 readMessage = (await _client.Message.DequeueMessageAndDelete(context, _queueName, null)).Value;
                if (readMessage != null)
                {
                    Interlocked.Increment(ref _readCount);
                }
            }
        }
    }
}

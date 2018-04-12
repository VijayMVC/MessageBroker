// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentAssertions;
using Khooversoft.Toolbox;
using MessageBroker.Api.Test.Application;
using MessageBroker.Common;
using MessageBroker.Common.RestApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MessageBroker.Api.Test.Queue
{
    [Collection("General")]
    public class MessageBrokerClientTests
    {
        private const string _agentName = "agent2";
        private static readonly string _queueName = nameof(MessageBrokerClientTests);
        private readonly Tag _tag = new Tag(nameof(MessageBrokerClientTests));
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private MessageBrokerClient<Payload> _client;
        private Utility _utility;

        public MessageBrokerClientTests()
        {
            _client = new MessageBrokerClient<Payload>(
                TestAssembly.ClientConfiguration.BaseUri,
                _agentName,
                _queueName,
                TestAssembly.GetRestClientConfiguration());

            _utility = new Utility(_client.Client);

            ResetDatabase()
                .GetAwaiter()
                .GetResult();
        }

        private async Task ResetDatabase()
        {
            var context = _workContext.WithTag(_tag);

            await _client.Client.Administration.ResetDatabase(context);
        }

        [Fact]
        public async Task ClientImplicitEnqueueDequeueTest()
        {
            var context = _workContext.WithTag(_tag);

            var payload = new Payload
            {
                ProcessName = Guid.NewGuid().ToString(),
                IntValue = 5,
            };

            await _client.Enqueue(context, payload);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            MessageEvent<Payload> readPayload = await _client.Dequeue(context, useSettle: false);
            readPayload.Should().NotBeNull();
            VerifyMessage(payload, readPayload.Message);
            readPayload.UseSettle.Should().BeFalse();

            await _utility.VerifyQueue(context, _queueName, 0, 0);

            HistoryContractV1 history = (await _client.Client.History.GetHistory(context, readPayload.Contract.MessageId)).Value;
            _utility.VerifyHistoryMessage(readPayload.Contract, history, _queueName, _agentName);
        }

        [Fact]
        public async Task ClientExplicitEnqueueDequeueTest()
        {
            var context = _workContext.WithTag(_tag);

            var variations = new[]
            {
                new {
                    SettleType = SettleType.Processed
                },
                new {
                    SettleType = SettleType.Rejected
                },
            };

            foreach (var test in variations)
            {
                var payload = new Payload
                {
                    ProcessName = Guid.NewGuid().ToString(),
                    IntValue = 5,
                };

                await _client.Enqueue(context, payload);
                await _utility.VerifyQueue(context, _queueName, 1, 0);

                MessageEvent<Payload> readPayload = await _client.Dequeue(context, useSettle: true);
                VerifyMessage(payload, readPayload.Message);
                readPayload.UseSettle.Should().BeTrue();

                await _client.Settle(context, readPayload, test.SettleType);
                await _utility.VerifyQueue(context, _queueName, 0, 0);

                HistoryContractV1 history = (await _client.Client.History.GetHistory(context, readPayload.Contract.MessageId)).Value;
                _utility.VerifyHistoryMessage(readPayload.Contract, history, _queueName, _agentName, settleType: test.SettleType);
            }
        }

        [Fact]
        public async Task ClientExplicitEnqueueDequeueProcessTest()
        {
            var context = _workContext.WithTag(_tag);

            var payload = new Payload
            {
                ProcessName = Guid.NewGuid().ToString(),
                IntValue = 5,
            };

            await _client.Enqueue(context, payload);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            Payload readPayload = null;
            MessageEvent messageEvent = null;
            int count = 0;

            Task t = _client.ProcessDequeue(
                context,
                (c, m, e) =>
                {
                    readPayload = m;
                    messageEvent = e;
                    Interlocked.Increment(ref count);
                    return Task.FromResult<bool>(true);
                },
                tokenSource.Token,
                useSettle: true,
                wait: false);

            await Task.Delay(TimeSpan.FromSeconds(5));
            tokenSource.Cancel();

            t.Wait();
            Verify.IsNotNull(nameof(messageEvent), messageEvent);
            count.Should().Be(1);
            VerifyMessage(payload, readPayload);

            await _client.Settle(context, messageEvent.Contract.MessageId, SettleType.Processed);
            await _utility.VerifyQueue(context, _queueName, 0, 0);

            HistoryContractV1 history = (await _client.Client.History.GetHistory(context, messageEvent.Contract.MessageId)).Value;
            _utility.VerifyHistoryMessage(messageEvent.Contract, history, _queueName, _agentName);
        }

        [Fact]
        public async Task ClientMultipleExplicitEnqueueDequeueProcessTest()
        {
            var context = _workContext.WithTag(_tag);
            var receiveList = new List<Tuple<Payload, MessageEvent>>();
            var sendList = new List<Payload>();
            const int messageCount = 10;

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            int count = 0;

            Task t = Task.Run(() => _client.ProcessDequeue(
                context,
                (c, m, e) =>
                {
                    receiveList.Add(new Tuple<Payload, MessageEvent>(m, e));
                    Interlocked.Increment(ref count);
                    return Task.FromResult(true);
                },
                tokenSource.Token,
                useSettle: true,
                wait: false)
                );

            foreach (var index in Enumerable.Range(0, messageCount))
            {
                var payload = new Payload
                {
                    ProcessName = Guid.NewGuid().ToString(),
                    IntValue = index,
                };

                await _client.Enqueue(context, payload);
                sendList.Add(payload);
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
            tokenSource.Cancel();

            t.Wait();
            count.Should().Be(messageCount);

            foreach (var index in Enumerable.Range(0, messageCount))
            {
                VerifyMessage(sendList[index], receiveList[index].Item1);

                MessageContractV1 contract = receiveList[index].Item2.Contract;
                HistoryContractV1 history = (await _client.Client.History.GetHistory(context, contract.MessageId)).Value;
                _utility.VerifyHistoryMessage(contract, history, _queueName, _agentName);
            }
        }

        private void VerifyMessage(Payload payload, Payload message)
        {
            Verify.IsNotNull(nameof(payload), payload);
            Verify.IsNotNull(nameof(message), message);

            payload.ProcessName.Should().Be(message.ProcessName);
            payload.IntValue.Should().Be(message.IntValue);
        }

        [JsonObject]
        private class Payload
        {
            [JsonProperty("processName")]
            public string ProcessName { get; set; }

            [JsonProperty("intValue")]
            public int IntValue { get; set; }
        }
    }
}

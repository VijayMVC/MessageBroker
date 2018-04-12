// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentAssertions;
using Khooversoft.Toolbox;
using MessageBroker.Api.Test.Application;
using MessageBroker.Common;
using MessageBroker.Common.RestApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBroker.Api.Test.Queue
{
    [Collection("General")]
    public class ScheduleTests
    {
        private const string _agentName = "agent1";
        private static readonly string _queueName = nameof(ScheduleTests);
        private readonly Tag _tag = new Tag(nameof(QueueTests));
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private MessageBrokerClient _client;
        private Utility _utility;

        public ScheduleTests()
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
        public async Task SimpleScheduleSuccessTestApi()
        {
            var context = _workContext.WithTag(_tag);

            AgentContractV1 agent = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            EnqueueMessageContractV1 message = _utility.CreateMessage(context, agent.AgentId, _queueName);
            message.ScheduleDate = DateTime.UtcNow.AddSeconds(2);

            await _client.Message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 0, 1);

            MessageContractV1 readMessage = (await _client.Message.DequeueMessageAndDelete(context, _queueName, null)).Value;
            readMessage.Should().BeNull();

            await Task.Delay(TimeSpan.FromSeconds(10));

            readMessage = (await _client.Message.DequeueMessageAndDelete(context, _queueName, null)).Value;
            _utility.VerifyMessage(message, readMessage, agent.AgentId, false);

            HistoryContractV1 history = (await _client.History.GetHistory(context, readMessage.MessageId)).Value;
            _utility.VerifyHistoryMessage(readMessage, history, _queueName, _agentName);
        }

        [Fact]
        public async Task MultipleScheduleSuccessTestApi()
        {
            const int scheduleSize = 10;
            var context = _workContext.WithTag(_tag);

            AgentContractV1 agent = await _utility.SetupAgentAndQueue(_queueName, _agentName);

            var list = new List<EnqueueMessageContractV1>();
            foreach (var item in Enumerable.Range(0, scheduleSize))
            {
                EnqueueMessageContractV1 message = _utility.CreateMessage(context, agent.AgentId, _queueName);
                message.ScheduleDate = DateTime.UtcNow.AddSeconds(2);

                await _client.Message.EnqueueMessage(context, message);
                list.Add(message);
            }

            await _utility.VerifyQueue(context, _queueName, 0, scheduleSize);

            MessageContractV1 readMessage = (await _client.Message.DequeueMessageAndDelete(context, _queueName, null)).Value;
            readMessage.Should().BeNull();

            await Task.Delay(TimeSpan.FromSeconds(10));

            var readList = new List<MessageContractV1>();
            foreach (var item in Enumerable.Range(0, scheduleSize))
            {
                readMessage = (await _client.Message.DequeueMessageAndDelete(context, _queueName, null)).Value;
                readMessage.Should().NotBeNull();

                readList.Add(readMessage);
            }

            readList.Count.Should().Be(scheduleSize);

            readMessage = (await _client.Message.DequeueMessageAndDelete(context, _queueName, null)).Value;
            readMessage.Should().BeNull();

            var zip = list
                .OrderBy(x => x.ClientMessageId)
                .Zip(readList.OrderBy(x => x.ClientMessageId), (f, s) => new Tuple<EnqueueMessageContractV1, MessageContractV1>(f, s));

            foreach (var item in zip)
            {
                _utility.VerifyMessage(item.Item1, item.Item2, agent.AgentId, false);
            }

            await _utility.VerifyQueue(context, _queueName, 0, 0);

            foreach (var item in readList)
            {
                HistoryContractV1 history = (await _client.History.GetHistory(context, item.MessageId)).Value;
                _utility.VerifyHistoryMessage(item, history, _queueName, _agentName);
            }
        }

        [Fact]
        public async Task SimpleDeleteScheduleSuccessTestApi()
        {
            var context = _workContext.WithTag(_tag);

            AgentContractV1 agent = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            EnqueueMessageContractV1 message = _utility.CreateMessage(context, agent.AgentId, _queueName);
            message.ScheduleDate = DateTime.UtcNow.AddSeconds(5);

            await _client.Message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 0, 1);

            MessageContractV1 readMessage = (await _client.Message.DequeueMessageAndDelete(context, _queueName, null)).Value;
            readMessage.Should().BeNull();

            await Task.Delay(TimeSpan.FromSeconds(6));

            readMessage = (await _client.Message.DequeueMessageAndDelete(context, _queueName, null)).Value;
            _utility.VerifyMessage(message, readMessage, agent.AgentId, false);

            HistoryContractV1 history = (await _client.History.GetHistory(context, readMessage.MessageId)).Value;
            _utility.VerifyHistoryMessage(readMessage, history, _queueName, _agentName);
        }
    }
}

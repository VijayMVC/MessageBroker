// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentAssertions;
using Khooversoft.Net;
using Khooversoft.Toolbox;
using MessageBroker.Api.Test.Application;
using MessageBroker.Common;
using MessageBroker.Common.RestApi;
using System.Threading.Tasks;
using Xunit;

namespace MessageBroker.Api.Test.Queue
{
    [Collection("General")]
    public class QueueManagementTests
    {
        private const string _agentName = "agent1";
        private static readonly string _queueName = nameof(QueueManagementTests);
        private readonly Tag _tag = new Tag(nameof(QueueTests));
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private MessageBrokerClient _client;
        private Utility _utility;

        public QueueManagementTests()
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
        public async Task SimpleClearQueueNoHistoryTestApi()
        {
            var context = _workContext.WithTag(_tag);

            AgentContractV1 agent = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            EnqueueMessageContractV1 message = _utility.CreateMessage(context, agent.AgentId, _queueName);

            RestResponse<EnqueuedContractV1> response = await _client.Message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            await _client.Management.ClearQueue(context, _queueName, false);

            await _utility.VerifyQueue(context, _queueName, 0, 0);

            HistoryContractV1 history = (await _client.History.GetHistory(context, response.Value.MessageId)).Value;
            history.Should().BeNull();
        }

        [Fact]
        public async Task SimpleClearQueueHistoryTestApi()
        {
            var context = _workContext.WithTag(_tag);

            AgentContractV1 agent = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            EnqueueMessageContractV1 message = _utility.CreateMessage(context, agent.AgentId, _queueName);

            RestResponse<EnqueuedContractV1> response = await _client.Message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            await _client.Management.ClearQueue(context, _queueName, true);

            await _utility.VerifyQueue(context, _queueName, 0, 0);

            HistoryContractV1 history = (await _client.History.GetHistory(context, response.Value.MessageId)).Value;
            history.Should().NotBeNull();
        }
    }
}

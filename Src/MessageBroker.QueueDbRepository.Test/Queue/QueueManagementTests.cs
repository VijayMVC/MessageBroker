// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentAssertions;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using System.Threading.Tasks;
using Xunit;

namespace MessageBroker.QueueDbRepository.Test.Queue
{
    public class QueueManagementTests
    {
        private const string _agentName = "agent1";
        private const string _queueName = "test-queue";
        private readonly Tag _tag = new Tag(nameof(QueueTests));
        private readonly Utility _utility = new Utility();
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private readonly IMessageBrokerAdministration _admin = TestAssembly.Administration;
        private readonly IMessageBrokerManagement _management = TestAssembly.Management;
        private readonly IMessageBroker _message = TestAssembly.Message;

        public QueueManagementTests()
        {
            ResetDatabase()
                .GetAwaiter()
                .GetResult();
        }

        private async Task ResetDatabase()
        {
            var context = _workContext.WithTag(_tag);

            await _admin.ResetDatabase(context);
            await _admin.SetHistorySizeConfiguration(context, 1000);
        }

        [Fact]
        public async Task SimpleClearQueueNoHistoryTest()
        {
            var context = _workContext.WithTag(_tag);

            int agentId = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            InternalEnqueueMessageV1 message = _utility.CreateMessage(context, agentId, _queueName);

            long messageId = await _message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            await _management.ClearQueue(context, _queueName, false);

            await _utility.VerifyQueue(context, _queueName, 0, 0);

            InternalHistoryV1 history = await _management.GetHistory(context, messageId);
            history.Should().BeNull();
        }

        [Fact]
        public async Task SimpleClearQueueHistoryTest()
        {
            var context = _workContext.WithTag(_tag);

            int agentId = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            InternalEnqueueMessageV1 message = _utility.CreateMessage(context, agentId, _queueName);

            long messageId = await _message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            await _management.ClearQueue(context, _queueName, true);

            await _utility.VerifyQueue(context, _queueName, 0, 0);

            InternalHistoryV1 history = await _management.GetHistory(context, messageId);
            history.Should().NotBeNull();
        }
    }
}

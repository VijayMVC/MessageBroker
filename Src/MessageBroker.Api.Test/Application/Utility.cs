// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentAssertions;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using MessageBroker.Common.RestApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageBroker.Api.Test.Application
{
    public class Utility
    {
        private readonly Tag _tag = new Tag(nameof(Utility));
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private readonly IMessageBrokerClient _client;

        public Utility(IMessageBrokerClient client)
        {
            _client = client;
        }

        public async Task<AgentContractV1> SetupAgentAndQueue(
            string queueName,
            string agentName,
            int queueSize = 1000,
            int currentRetryLimit = 3,
            int lockValidForSec = 300)
        {
            var context = _workContext.WithTag(_tag);

            var contract = new SetQueueContractV1
            {
                QueueName = queueName,
                CurrentSizeLimit = queueSize,
                CurrentRetryLimit = currentRetryLimit,
                LockValidForSec = lockValidForSec,
            };

            await _client.Management.SetQueue(context, contract);

            return (await _client.Message.GetAgentId(context, queueName, agentName)).Value;
        }

        public EnqueueMessageContractV1 CreateMessage(IWorkContext context, int agentId, string queueName)
        {
            var workRequest = new WorkRequest
            {
                ProcessName = "this process",
                Parameters = new List<string>(new string[] { "P1='1'", "p2=3", "p4=3" }),
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

        public async Task<QueueDetailContractV1> VerifyQueue(IWorkContext context, string queueName, int queueLength, int scheduleQueueLength)
        {
            IEnumerable<QueueDetailContractV1> list = (await _client.Management.GetQueueList(context)).Value.Items;
            list.Should().NotBeNull();
            list.Count().Should().BeGreaterOrEqualTo(1);

            QueueDetailContractV1 queueItem = list.First(x => x.QueueName == queueName);
            queueItem.QueueName.Should().Be(queueName);
            queueItem.QueueLength.Should().NotHaveValue();

            QueueDetailContractV1 queueDetail = (await _client.Management.GetQueue(context, queueName)).Value;
            queueDetail.Should().NotBeNull();
            queueDetail.QueueName.Should().Be(queueName);
            queueDetail.QueueLength.Should().HaveValue();
            queueDetail.QueueLength.Should().Be(queueLength);
            queueDetail.ScheduleQueueLength.Should().HaveValue();
            queueDetail.ScheduleQueueLength.Should().Be(scheduleQueueLength);

            return queueDetail;
        }

        public async Task<IEnumerable<ScheduleContractV1>> VerifySchedule(IWorkContext context, string queueName, int scheduleQueueLength)
        {
            IEnumerable<ScheduleContractV1> list = (await _client.Message.GetMessageSchedules(context, queueName)).Value.Items;
            list.Should().NotBeNull();
            list.Count().Should().Be(scheduleQueueLength);

            return list;
        }

        public void VerifyMessage(EnqueueMessageContractV1 message, MessageContractV1 readMessage, int agentId, bool testLock)
        {
            readMessage.Should().NotBeNull();
            readMessage.QueueId.Should().BeGreaterOrEqualTo(0);
            readMessage.MessageId.Should().BeGreaterOrEqualTo(0);
            readMessage.CreatedByAgentId.Should().Be(agentId);
            readMessage.ClientMessageId.Should().Be(message.ClientMessageId);
            readMessage.Cv.Should().Be(message.Cv);
            readMessage.Payload.Should().Be(message.Payload);

            if (testLock)
            {
                readMessage.LockedByAgentId.Should().Be(agentId);
            }

            WorkRequest readWorkRequest = readMessage.Deserialize<WorkRequest>();
            readWorkRequest.Should().NotBeNull();
            readWorkRequest.ProcessName.Should().Be("this process");
            readWorkRequest.Parameters.Should().NotBeNull();
            readWorkRequest.Parameters.Count().Should().Be(3);
        }

        public void VerifyHistoryMessage(
            MessageContractV1 readMessage,
            HistoryContractV1 history,
            string queueName,
            string agentName,
            int retryCount = 1,
            SettleType settleType = SettleType.Processed
            )
        {
            history.Should().NotBeNull();
            history.HistoryId.Should().BeGreaterOrEqualTo(0);
            history.MessageId.Should().Be(readMessage.MessageId);
            history.ActivityType.Should().Be(settleType.ToString());
            history.QueueName.Should().Be(queueName);
            history.Cv.Should().Be(readMessage.Cv);
            history.ClientMessageId.Should().Be(readMessage.ClientMessageId);
            history.Payload.Should().Be(readMessage.Payload);
            history.SettleByAgent.Should().Be(agentName);
            history.ErrorMesage.Should().BeNullOrEmpty();
            history.RetryCount.Should().Be(retryCount);
        }

        [JsonObject]
        private class WorkRequest
        {
            [JsonProperty("processName")]
            public string ProcessName { get; set; }

            [JsonProperty("parameters")]
            public IList<string> Parameters { get; set; }
        }
    }
}

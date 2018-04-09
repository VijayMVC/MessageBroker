using FluentAssertions;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBroker.QueueDbRepository.Test.Queue
{
    public class ScheduleTests
    {
        private const string _agentName = "agent1";
        private const string _queueName = "test-queue";
        private readonly Tag _tag = new Tag(nameof(QueueTests));
        private readonly Utility _utility = new Utility();
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private readonly IMessageBrokerAdministration _admin = Repository.Administration;
        private readonly IMessageBrokerManagement _management = Repository.Management;
        private readonly IMessageBroker _message = Repository.Message;

        public ScheduleTests()
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
        public async Task SimpleScheduleSuccessTest()
        {
            var context = _workContext.WithTag(_tag);

            int agentId = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            InternalEnqueueMessageV1 message = _utility.CreateMessage(context, agentId, _queueName);
            message.ScheduleDate = DateTime.UtcNow.AddSeconds(2);

            await _message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 0, 1);

            InternalMessageV1 readMessage = await _message.DequeueMessageAndDelete(context, _queueName);
            readMessage.Should().BeNull();

            await Task.Delay(TimeSpan.FromSeconds(5));

            readMessage = await _message.DequeueMessageAndDelete(context, _queueName);
            _utility.VerifyMessage(message, readMessage, agentId, false);

            InternalHistoryV1 history = await _management.GetHistory(context, readMessage.MessageId);
            _utility.VerifyHistoryMessage(readMessage, history, _queueName, _agentName);
        }

        [Fact]
        public async Task MultipleScheduleSuccessTest()
        {
            const int scheduleSize = 10;
            var context = _workContext.WithTag(_tag);

            int agentId = await _utility.SetupAgentAndQueue(_queueName, _agentName);

            var list = new List<InternalEnqueueMessageV1>();
            foreach (var item in Enumerable.Range(0, scheduleSize))
            {
                InternalEnqueueMessageV1 message = _utility.CreateMessage(context, agentId, _queueName);
                message.ScheduleDate = DateTime.UtcNow.AddSeconds(2);

                await _message.EnqueueMessage(context, message);
                list.Add(message);
            }

            await _utility.VerifyQueue(context, _queueName, 0, scheduleSize);

            InternalMessageV1 readMessage = await _message.DequeueMessageAndDelete(context, _queueName);
            readMessage.Should().BeNull();

            await Task.Delay(TimeSpan.FromSeconds(5));

            var readList = new List<InternalMessageV1>();
            foreach (var item in Enumerable.Range(0, scheduleSize))
            {
                readMessage = await _message.DequeueMessageAndDelete(context, _queueName);
                readMessage.Should().NotBeNull();

                readList.Add(readMessage);
            }

            readList.Count.Should().Be(scheduleSize);

            readMessage = await _message.DequeueMessageAndDelete(context, _queueName);
            readMessage.Should().BeNull();

            var zip = list
                .OrderBy(x => x.ClientMessageId)
                .Zip(readList.OrderBy(x => x.ClientMessageId), (f, s) => new Tuple<InternalEnqueueMessageV1, InternalMessageV1>(f, s));

            foreach (var item in zip)
            {
                _utility.VerifyMessage(item.Item1, item.Item2, agentId, false);
            }

            await _utility.VerifyQueue(context, _queueName, 0, 0);

            foreach (var item in readList)
            {
                InternalHistoryV1 history = await _management.GetHistory(context, item.MessageId);
                _utility.VerifyHistoryMessage(item, history, _queueName, _agentName);
            }
        }

        [Fact]
        public async Task SimpleDeleteScheduleSuccessTest()
        {
            var context = _workContext.WithTag(_tag);

            int agentId = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            InternalEnqueueMessageV1 message = _utility.CreateMessage(context, agentId, _queueName);
            message.ScheduleDate = DateTime.UtcNow.AddSeconds(5);

            await _message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 0, 1);

            InternalMessageV1 readMessage = await _message.DequeueMessageAndDelete(context, _queueName);
            readMessage.Should().BeNull();

            await Task.Delay(TimeSpan.FromSeconds(6));

            readMessage = await _message.DequeueMessageAndDelete(context, _queueName);
            _utility.VerifyMessage(message, readMessage, agentId, false);

            InternalHistoryV1 history = await _management.GetHistory(context, readMessage.MessageId);
            _utility.VerifyHistoryMessage(readMessage, history, _queueName, _agentName);
        }
    }
}

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
    public class QueueTests
    {
        private const string _agentName = "agent1";
        private const string _queueName = "test-queue";
        private readonly Tag _tag = new Tag(nameof(QueueTests));
        private readonly Utility _utility = new Utility();
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private readonly IMessageBrokerAdministration _admin = TestAssembly.Administration;
        private readonly IMessageBrokerManagement _management = TestAssembly.Management;
        private readonly IMessageBroker _message = TestAssembly.Message;

        public QueueTests()
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
        public async Task SimpleEnqueueSuccessTest()
        {
            var context = _workContext.WithTag(_tag);

            int agentId = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            InternalEnqueueMessageV1 message = _utility.CreateMessage(context, agentId, _queueName);

            await _message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            InternalMessageV1 readMessage = await _message.DequeueMessageAndDelete(context, _queueName);
            _utility.VerifyMessage(message, readMessage, agentId, false);

            await _utility.VerifyQueue(context, _queueName, 0, 0);

            InternalHistoryV1 history = await _management.GetHistory(context, readMessage.MessageId);
            _utility.VerifyHistoryMessage(readMessage, history, _queueName, _agentName);
        }

        [Fact]
        public async Task SimpleEnqueueWithLockSuccessTest()
        {
            var context = _workContext.WithTag(_tag);

            int agentId = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            InternalEnqueueMessageV1 message = _utility.CreateMessage(context, agentId, _queueName);

            await _message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            InternalMessageV1 readMessage = await _message.DequeueMessageWithLock(context, _queueName, agentId);
            _utility.VerifyMessage(message, readMessage, agentId, true);

            await _message.SettleMessage(context, readMessage.MessageId, agentId, SettleType.Processed);
            await _utility.VerifyQueue(context, _queueName, 0, 0);

            InternalHistoryV1 history = await _management.GetHistory(context, readMessage.MessageId);
            _utility.VerifyHistoryMessage(readMessage, history, _queueName, _agentName);
        }

        [Fact]
        public async Task SimpleEnqueueWithLockTimeoutSuccessTest()
        {
            var context = _workContext.WithTag(_tag);

            int agentId = await _utility.SetupAgentAndQueue(_queueName, _agentName, lockValidForSec: 5);
            InternalEnqueueMessageV1 message = _utility.CreateMessage(context, agentId, _queueName);

            await _message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            InternalMessageV1 readMessage = await _message.DequeueMessageWithLock(context, _queueName, agentId);
            long saveMessageId = readMessage.MessageId;
            _utility.VerifyMessage(message, readMessage, agentId, true);

            InternalMessageV1 tempMessage = await _message.DequeueMessageWithLock(context, _queueName, agentId);
            tempMessage.Should().BeNull();

            await Task.Delay(TimeSpan.FromSeconds(10));

            readMessage = await _message.DequeueMessageWithLock(context, _queueName, agentId);
            _utility.VerifyMessage(message, readMessage, agentId, true);
            readMessage.MessageId.Should().Be(saveMessageId);

            await _message.SettleMessage(context, readMessage.MessageId, agentId, SettleType.Processed);
            await _utility.VerifyQueue(context, _queueName, 0, 0);

            InternalHistoryV1 history = await _management.GetHistory(context, readMessage.MessageId);
            _utility.VerifyHistoryMessage(readMessage, history, _queueName, _agentName, retryCount: 2);
        }

        [Fact]
        public async Task MultipleEnqueueWithLockSuccessTest()
        {
            const int messageSize = 100;
            var context = _workContext.WithTag(_tag);

            int agentId = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            var messageList = new List<InternalEnqueueMessageV1>();
            var readMessageList = new List<InternalMessageV1>();

            foreach (int index in Enumerable.Range(0, messageSize))
            {
                InternalEnqueueMessageV1 message = _utility.CreateMessage(context, agentId, _queueName);
                messageList.Add(message);

                await _message.EnqueueMessage(context, message);
            }

            await _utility.VerifyQueue(context, _queueName, messageSize, 0);

            InternalMessageV1 readMessage;
            foreach (int index in Enumerable.Range(0, messageSize))
            {
                readMessage = await _message.DequeueMessageWithLock(context, _queueName, agentId);
                readMessage.Should().NotBeNull();
                readMessageList.Add(readMessage);
            }

            readMessage = await _message.DequeueMessageWithLock(context, _queueName, agentId);
            readMessage.Should().BeNull();

            var zip = messageList
                .OrderBy(x => x.ClientMessageId)
                .Zip(readMessageList.OrderBy(x => x.ClientMessageId), (f, s) => new Tuple<InternalEnqueueMessageV1, InternalMessageV1>(f, s));

            foreach (var item in zip)
            {
                _utility.VerifyMessage(item.Item1, item.Item2, agentId, true);
            }

            foreach (var item in readMessageList)
            {
                await _message.SettleMessage(context, item.MessageId, agentId, SettleType.Processed);
            }

            await _utility.VerifyQueue(context, _queueName, 0, 0);

            foreach (var item in readMessageList)
            {
                InternalHistoryV1 history = await _management.GetHistory(context, item.MessageId);
                _utility.VerifyHistoryMessage(item, history, _queueName, _agentName);
            }
        }

        [Fact]
        public async Task QueueSizeLimitTest()
        {
            const int messageSize = 10;
            var context = _workContext.WithTag(_tag);

            int agentId = await _utility.SetupAgentAndQueue(_queueName, _agentName, queueSize: 10);
            var messageList = new List<InternalEnqueueMessageV1>();
            var readMessageList = new List<InternalMessageV1>();

            foreach (int index in Enumerable.Range(0, messageSize))
            {
                InternalEnqueueMessageV1 message = _utility.CreateMessage(context, agentId, _queueName);
                messageList.Add(message);

                await _message.EnqueueMessage(context, message);
            }

            await Verify.AssertExceptionAsync<QueueFullException>(async () =>
            {
                InternalEnqueueMessageV1 message = _utility.CreateMessage(context, agentId, _queueName);
                await _message.EnqueueMessage(context, message);
            });

            await _utility.VerifyQueue(context, _queueName, messageSize, 0);

            InternalMessageV1 readMessage;
            foreach (int index in Enumerable.Range(0, messageSize))
            {
                readMessage = await _message.DequeueMessageWithLock(context, _queueName, agentId);
                readMessage.Should().NotBeNull();
                readMessageList.Add(readMessage);
            }

            readMessage = await _message.DequeueMessageWithLock(context, _queueName, agentId);
            readMessage.Should().BeNull();

            var zip = messageList
                .OrderBy(x => x.ClientMessageId)
                .Zip(readMessageList.OrderBy(x => x.ClientMessageId), (f, s) => new Tuple<InternalEnqueueMessageV1, InternalMessageV1>(f, s));

            foreach (var item in zip)
            {
                _utility.VerifyMessage(item.Item1, item.Item2, agentId, true);
            }

            foreach (var item in readMessageList)
            {
                await _message.SettleMessage(context, item.MessageId, agentId, SettleType.Processed);
            }

            await _utility.VerifyQueue(context, _queueName, 0, 0);

            foreach (var item in readMessageList)
            {
                InternalHistoryV1 history = await _management.GetHistory(context, item.MessageId);
                _utility.VerifyHistoryMessage(item, history, _queueName, _agentName);
            }
        }

        [Fact]
        public async Task QueueRejectTest()
        {
            var context = _workContext.WithTag(_tag);

            int agentId = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            InternalEnqueueMessageV1 message = _utility.CreateMessage(context, agentId, _queueName);

            await _message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            InternalMessageV1 readMessage = await _message.DequeueMessageWithLock(context, _queueName, agentId);
            _utility.VerifyMessage(message, readMessage, agentId, true);

            await _message.SettleMessage(context, readMessage.MessageId, agentId, SettleType.Rejected);
            await _utility.VerifyQueue(context, _queueName, 0, 0);

            InternalHistoryV1 history = await _management.GetHistory(context, readMessage.MessageId);
            _utility.VerifyHistoryMessage(readMessage, history, _queueName, _agentName, settleType: SettleType.Rejected);
        }

        [Fact]
        public async Task QueueAbandonTest()
        {
            var context = _workContext.WithTag(_tag);

            int agentId = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            InternalEnqueueMessageV1 message = _utility.CreateMessage(context, agentId, _queueName);

            await _message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            InternalMessageV1 readMessage = await _message.DequeueMessageWithLock(context, _queueName, agentId);
            _utility.VerifyMessage(message, readMessage, agentId, true);

            await _message.SettleMessage(context, readMessage.MessageId, agentId, SettleType.Abandon);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            readMessage = await _message.DequeueMessageWithLock(context, _queueName, agentId);
            _utility.VerifyMessage(message, readMessage, agentId, true);

            await _message.SettleMessage(context, readMessage.MessageId, agentId, SettleType.Processed);
            await _utility.VerifyQueue(context, _queueName, 0, 0);

            InternalHistoryV1 history = await _management.GetHistory(context, readMessage.MessageId);
            _utility.VerifyHistoryMessage(readMessage, history, _queueName, _agentName, 2);
        }

        [Fact]
        public async Task ListActiveQueueTest()
        {
            const int messageSize = 100;
            var context = _workContext.WithTag(_tag);

            int agentId = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            var messageList = new List<InternalEnqueueMessageV1>();

            foreach (int index in Enumerable.Range(0, messageSize))
            {
                InternalEnqueueMessageV1 message = _utility.CreateMessage(context, agentId, _queueName);
                messageList.Add(message);

                await _message.EnqueueMessage(context, message);
            }

            await _utility.VerifyQueue(context, _queueName, messageSize, 0);

            IEnumerable<InternalMessageV1> activeMessage = await _message.ListActiveMessages(context, _queueName);

            var zip = messageList
                .OrderBy(x => x.ClientMessageId)
                .Zip(activeMessage.OrderBy(x => x.ClientMessageId), (f, s) => new Tuple<InternalEnqueueMessageV1, InternalMessageV1>(f, s));
        }
    }
}

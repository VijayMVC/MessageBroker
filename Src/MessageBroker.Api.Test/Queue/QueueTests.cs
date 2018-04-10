using FluentAssertions;
using Khooversoft.Net;
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
    public class QueueTests
    {
        private const string _agentName = "agent1";
        private static readonly string _queueName = nameof(QueueTests);
        private readonly Tag _tag = new Tag(nameof(QueueTests));
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private MessageBrokerClient _client;
        private Utility _utility;

        public QueueTests()
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
        public async Task SimpleEnqueueSuccessTestApi()
        {
            var context = _workContext.WithTag(_tag);

            AgentContractV1 agent = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            EnqueueMessageContractV1 message = _utility.CreateMessage(context, agent.AgentId, _queueName);

            await _client.Message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            MessageContractV1 readMessage = (await _client.Message.DequeueMessageAndDelete(context, _queueName, null)).Value;
            _utility.VerifyMessage(message, readMessage, agent.AgentId, false);

            await _utility.VerifyQueue(context, _queueName, 0, 0);

            HistoryContractV1 history = (await _client.History.GetHistory(context, readMessage.MessageId)).Value;
            _utility.VerifyHistoryMessage(readMessage, history, _queueName, _agentName);
        }

        [Fact]
        public async Task SimpleEnqueueWithLockSuccessTestApi()
        {
            var context = _workContext.WithTag(_tag);

            AgentContractV1 agent = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            EnqueueMessageContractV1 message = _utility.CreateMessage(context, agent.AgentId, _queueName);

            await _client.Message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            MessageContractV1 readMessage = (await _client.Message.DequeueMessageWithLock(context, _queueName, agent, null)).Value;
            _utility.VerifyMessage(message, readMessage, agent.AgentId, true);

            var settleMessage = new SettleMessageContractV1
            {
                AgentId = agent.AgentId,
                QueueName = _queueName,
                SettleType = SettleType.Processed,
                MessageId = readMessage.MessageId,
            };

            await _client.Message.SettleMessage(context, settleMessage);
            await _utility.VerifyQueue(context, _queueName, 0, 0);

            HistoryContractV1 history = (await _client.History.GetHistory(context, readMessage.MessageId)).Value;
            _utility.VerifyHistoryMessage(readMessage, history, _queueName, _agentName);
        }

        [Fact]
        public async Task SimpleEnqueueWithLockTimeoutSuccessTestApi()
        {
            var context = _workContext.WithTag(_tag);

            AgentContractV1 agent = await _utility.SetupAgentAndQueue(_queueName, _agentName, lockValidForSec: 5);
            EnqueueMessageContractV1 message = _utility.CreateMessage(context, agent.AgentId, _queueName);

            await _client.Message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            MessageContractV1 readMessage = (await _client.Message.DequeueMessageWithLock(context, _queueName, agent, null)).Value;
            long saveMessageId = readMessage.MessageId;
            _utility.VerifyMessage(message, readMessage, agent.AgentId, true);

            MessageContractV1 tempMessage = (await _client.Message.DequeueMessageWithLock(context, _queueName, agent, null)).Value;
            tempMessage.Should().BeNull();

            await Task.Delay(TimeSpan.FromSeconds(10));

            readMessage = (await _client.Message.DequeueMessageWithLock(context, _queueName, agent, null)).Value;
            _utility.VerifyMessage(message, readMessage, agent.AgentId, true);
            readMessage.MessageId.Should().Be(saveMessageId);

            var settleMessage = new SettleMessageContractV1
            {
                AgentId = agent.AgentId,
                QueueName = _queueName,
                SettleType = SettleType.Processed,
                MessageId = readMessage.MessageId,
            };

            await _client.Message.SettleMessage(context, settleMessage);
            await _utility.VerifyQueue(context, _queueName, 0, 0);

            HistoryContractV1 history = (await _client.History.GetHistory(context, readMessage.MessageId)).Value;
            _utility.VerifyHistoryMessage(readMessage, history, _queueName, _agentName, retryCount: 2);
        }

        [Fact]
        public async Task MultipleEnqueueWithLockSuccessTestApi()
        {
            const int messageSize = 100;
            var context = _workContext.WithTag(_tag);

            AgentContractV1 agent = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            var messageList = new List<EnqueueMessageContractV1>();
            var readMessageList = new List<MessageContractV1>();

            foreach (int index in Enumerable.Range(0, messageSize))
            {
                EnqueueMessageContractV1 message = _utility.CreateMessage(context, agent.AgentId, _queueName);
                messageList.Add(message);

                await _client.Message.EnqueueMessage(context, message);
            }

            await _utility.VerifyQueue(context, _queueName, messageSize, 0);

            MessageContractV1 readMessage;
            foreach (int index in Enumerable.Range(0, messageSize))
            {
                readMessage = (await _client.Message.DequeueMessageWithLock(context, _queueName, agent, null)).Value;
                readMessage.Should().NotBeNull();
                readMessageList.Add(readMessage);
            }

            readMessage = (await _client.Message.DequeueMessageWithLock(context, _queueName, agent, null)).Value;
            readMessage.Should().BeNull();

            var zip = messageList
                .OrderBy(x => x.ClientMessageId)
                .Zip(readMessageList.OrderBy(x => x.ClientMessageId), (f, s) => new Tuple<EnqueueMessageContractV1, MessageContractV1>(f, s));

            foreach (var item in zip)
            {
                _utility.VerifyMessage(item.Item1, item.Item2, agent.AgentId, true);
            }

            foreach (var item in readMessageList)
            {
                var settleMessage = new SettleMessageContractV1
                {
                    AgentId = agent.AgentId,
                    QueueName = _queueName,
                    SettleType = SettleType.Processed,
                    MessageId = item.MessageId,
                };

                await _client.Message.SettleMessage(context, settleMessage);
            }

            await _utility.VerifyQueue(context, _queueName, 0, 0);

            foreach (var item in readMessageList)
            {
                HistoryContractV1 history = (await _client.History.GetHistory(context, item.MessageId)).Value;
                _utility.VerifyHistoryMessage(item, history, _queueName, _agentName);
            }
        }

        [Fact]
        public async Task QueueSizeLimitTestApi()
        {
            const int messageSize = 10;
            var context = _workContext.WithTag(_tag);

            AgentContractV1 agent = await _utility.SetupAgentAndQueue(_queueName, _agentName, queueSize: 10);
            var messageList = new List<EnqueueMessageContractV1>();
            var readMessageList = new List<MessageContractV1>();

            foreach (int index in Enumerable.Range(0, messageSize))
            {
                EnqueueMessageContractV1 message = _utility.CreateMessage(context, agent.AgentId, _queueName);
                messageList.Add(message);

                await _client.Message.EnqueueMessage(context, message);
            }

            await Verify.AssertExceptionAsync<RestConflictException>(async () =>
            {
               EnqueueMessageContractV1 message = _utility.CreateMessage(context, agent.AgentId, _queueName);
               await _client.Message.EnqueueMessage(context, message);
            });

            await _utility.VerifyQueue(context, _queueName, messageSize, 0);

            MessageContractV1 readMessage;
            foreach (int index in Enumerable.Range(0, messageSize))
            {
                readMessage = (await _client.Message.DequeueMessageWithLock(context, _queueName, agent, null)).Value;
                readMessage.Should().NotBeNull();
                readMessageList.Add(readMessage);
            }

            readMessage = (await _client.Message.DequeueMessageWithLock(context, _queueName, agent, null)).Value;
            readMessage.Should().BeNull();

            var zip = messageList
                .OrderBy(x => x.ClientMessageId)
                .Zip(readMessageList.OrderBy(x => x.ClientMessageId), (f, s) => new Tuple<EnqueueMessageContractV1, MessageContractV1>(f, s));

            foreach (var item in zip)
            {
                _utility.VerifyMessage(item.Item1, item.Item2, agent.AgentId, true);
            }

            foreach (var item in readMessageList)
            {
                var settleMessage = new SettleMessageContractV1
                {
                    AgentId = agent.AgentId,
                    QueueName = _queueName,
                    SettleType = SettleType.Processed,
                    MessageId = item.MessageId,
                };

                await _client.Message.SettleMessage(context, settleMessage);
            }

            await _utility.VerifyQueue(context, _queueName, 0, 0);

            foreach (var item in readMessageList)
            {
                HistoryContractV1 history = (await _client.History.GetHistory(context, item.MessageId)).Value;
                _utility.VerifyHistoryMessage(item, history, _queueName, _agentName);
            }
        }

        [Fact]
        public async Task QueueRejectTestApi()
        {
            var context = _workContext.WithTag(_tag);

            AgentContractV1 agent = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            EnqueueMessageContractV1 message = _utility.CreateMessage(context, agent.AgentId, _queueName);

            await _client.Message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            MessageContractV1 readMessage = (await _client.Message.DequeueMessageWithLock(context, _queueName, agent, null)).Value;
            _utility.VerifyMessage(message, readMessage, agent.AgentId, true);

            var settleMessage = new SettleMessageContractV1
            {
                AgentId = agent.AgentId,
                QueueName = _queueName,
                SettleType = SettleType.Rejected,
                MessageId = readMessage.MessageId,
            };

            await _client.Message.SettleMessage(context, settleMessage);
            await _utility.VerifyQueue(context, _queueName, 0, 0);

            HistoryContractV1 history = (await _client.History.GetHistory(context, readMessage.MessageId)).Value;
            _utility.VerifyHistoryMessage(readMessage, history, _queueName, _agentName, settleType: SettleType.Rejected);
        }

        [Fact]
        public async Task QueueAbandonTestApi()
        {
            var context = _workContext.WithTag(_tag);

            AgentContractV1 agent = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            EnqueueMessageContractV1 message = _utility.CreateMessage(context, agent.AgentId, _queueName);

            await _client.Message.EnqueueMessage(context, message);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            MessageContractV1 readMessage = (await _client.Message.DequeueMessageWithLock(context, _queueName, agent, null)).Value;
            _utility.VerifyMessage(message, readMessage, agent.AgentId, true);

            var settleMessage = new SettleMessageContractV1
            {
                AgentId = agent.AgentId,
                QueueName = _queueName,
                SettleType = SettleType.Abandon,
                MessageId = readMessage.MessageId,
            };

            await _client.Message.SettleMessage(context, settleMessage);
            await _utility.VerifyQueue(context, _queueName, 1, 0);

            readMessage = (await _client.Message.DequeueMessageWithLock(context, _queueName, agent, null)).Value;
            _utility.VerifyMessage(message, readMessage, agent.AgentId, true);

            settleMessage = new SettleMessageContractV1
            {
                AgentId = agent.AgentId,
                QueueName = _queueName,
                SettleType = SettleType.Processed,
                MessageId = readMessage.MessageId,
            };

            await _client.Message.SettleMessage(context, settleMessage);
            await _utility.VerifyQueue(context, _queueName, 0, 0);

            HistoryContractV1 history = (await _client.History.GetHistory(context, readMessage.MessageId)).Value;
            _utility.VerifyHistoryMessage(readMessage, history, _queueName, _agentName, 2);
        }


        [Fact]
        public async Task ListActiveQueueTestApi()
        {
            const int messageSize = 100;
            var context = _workContext.WithTag(_tag);

            AgentContractV1 agent = await _utility.SetupAgentAndQueue(_queueName, _agentName);
            var messageList = new List<EnqueueMessageContractV1>();

            foreach (int index in Enumerable.Range(0, messageSize))
            {
                EnqueueMessageContractV1 message = _utility.CreateMessage(context, agent.AgentId, _queueName);
                messageList.Add(message);

                await _client.Message.EnqueueMessage(context, message);
            }

            await _utility.VerifyQueue(context, _queueName, messageSize, 0);

            IEnumerable<MessageContractV1> activeMessage = (await _client.Message.GetActiveMessages(context, _queueName)).Value.Items;

            var zip = messageList
                .OrderBy(x => x.ClientMessageId)
                .Zip(activeMessage.OrderBy(x => x.ClientMessageId), (f, s) => new Tuple<EnqueueMessageContractV1, MessageContractV1>(f, s));
        }
    }
}

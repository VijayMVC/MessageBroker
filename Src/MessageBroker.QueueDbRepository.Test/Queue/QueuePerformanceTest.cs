using Khooversoft.Toolbox;
using MessageBroker.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MessageBroker.QueueDbRepository.Test.Queue
{
    public class QueuePerformanceTest
    {
        private const string _agentName = "agent1";
        private const string _queueName = "test-queue";
        private readonly Tag _tag = new Tag(nameof(QueueTests));
        private readonly Utility _utility = new Utility();
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private readonly IMessageBrokerAdministration _admin = TestAssembly.Administration;
        private readonly IMessageBrokerManagement _management = TestAssembly.Management;
        private readonly IMessageBroker _message = TestAssembly.Message;
        private long _writeCounter = 0;
        private long _readCount = 0;

        public QueuePerformanceTest()
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
        public async Task QueueReadWriteTest()
        {
            var context = _workContext.WithTag(_tag);

            int agentId = await _utility.SetupAgentAndQueue(_queueName, _agentName);

            var tokenSource = new CancellationTokenSource();
            var taskList = new List<Task>();
            taskList.Add(Task.Run(() => ReadQueue(context, tokenSource.Token)));
            taskList.Add(Task.Run(() => WriteQueue(context, agentId, tokenSource.Token)));

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

                InternalEnqueueMessageV1 message = _utility.CreateMessage(context, agentId, _queueName);
                await _message.EnqueueMessage(context, message);

                Interlocked.Increment(ref _writeCounter);
            }
        }

        private async Task ReadQueue(IWorkContext context, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                InternalMessageV1 readMessage = await _message.DequeueMessageAndDelete(context, _queueName);
                if (readMessage != null)
                {
                    Interlocked.Increment(ref _readCount);
                }
            }
        }
    }
}

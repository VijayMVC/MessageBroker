using Khooversoft.Toolbox;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    /// <summary>
    /// Message manager
    /// 
    /// (1) Provides long-polling capability using auto reset event
    /// </summary>
    public class MessageManager : IMessageManager
    {
        private static readonly TimeSpan _startSleep = TimeSpan.FromMilliseconds(10);
        private static readonly TimeSpan _maxSleep = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan _incrementSleep = TimeSpan.FromMilliseconds(100);
        private readonly IMessageBroker _message;
        private readonly ConcurrentDictionary<string, AutoResetEvent> _event = new ConcurrentDictionary<string, AutoResetEvent>(StringComparer.OrdinalIgnoreCase);

        public MessageManager(IMessageBroker messageBroker)
        {
            Verify.IsNotNull(nameof(messageBroker), messageBroker);

            _message = messageBroker;
        }

        /// <summary>
        /// Enqueue message and notify any subscriptions for the queue
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="message">enqueue message</param>
        /// <returns>message id</returns>
        public async Task<long> EnqueueMessage(IWorkContext context, InternalEnqueueMessageV1 message)
        {
            long messageId = await _message.EnqueueMessage(context, message);

            // Get auto event for signal to waiting for this message queue
            AutoResetEvent autoResetEvent = _event.GetOrAdd(message.QueueName, x => new AutoResetEvent(false));
            autoResetEvent.Set();

            return messageId;
        }

        /// <summary>
        /// Dequeue message and delete
        /// 
        /// If wait for is specified, register for enqueue notifications and wait.  Wait for must be less then HttpClient's
        /// timeout (default is 100 seconds).
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="queueName">queue name</param>
        /// <param name="waitFor">wait for (optional)</param>
        /// <returns>null or message</returns>
        public async Task<InternalMessageV1> DequeueMessageAndDelete(IWorkContext context, string queueName, TimeSpan? waitFor = null)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            return await TryWithBackoff(queueName, async () => await _message.DequeueMessageAndDelete(context, queueName), waitFor);
        }

        /// <summary>
        /// Dequeue message and lock
        /// 
        /// If wait for is specified, register for enqueue notifications and wait.  Wait for must be less then HttpClient's
        /// timeout (default is 100 seconds).
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="queueName">queue name</param>
        /// <param name="agentId">agent id</param>
        /// <param name="waitFor">wait for (optional)</param>
        /// <returns>null or message</returns>
        public async Task<InternalMessageV1> DequeueMessageWithLock(IWorkContext context, string queueName, int agentId, TimeSpan? waitFor = null)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            return await TryWithBackoff(queueName, async () => await _message.DequeueMessageWithLock(context, queueName, agentId), waitFor);
        }

        public Task SettleMessage(IWorkContext context, long messageId, int agentId, SettleType settleType, string errorMessage = null)
        {
            return _message.SettleMessage(context, messageId, agentId, settleType, errorMessage);
        }

        public Task<IEnumerable<InternalMessageV1>> ListActiveMessage(IWorkContext context, string queueName)
        {
            return _message.ListActiveMessages(context, queueName);
        }

        public Task<IEnumerable<InternalScheduleV1>> GetMessageSchedules(IWorkContext context, string queueName)
        {
            return _message.GetMessageSchedules(context, queueName);
        }

        public Task DeleteMessageSchedule(IWorkContext context, long scheduleId)
        {
            return _message.DeleteMessageSchedule(context, scheduleId);
        }

        public Task<int> GetAgentId(IWorkContext context, string agentName)
        {
            return _message.GetAgentId(context, agentName);
        }

        /// <summary>
        /// Try to get message with back off if waitFor is specified
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <param name="method">get message lambda</param>
        /// <param name="waitFor">wait for (optional)</param>
        /// <returns></returns>
        private async Task<InternalMessageV1> TryWithBackoff(string queueName, Func<Task<InternalMessageV1>> method, TimeSpan? waitFor)
        {
            InternalMessageV1 result = await method();
            if (result != null || waitFor == null)
            {
                return result;
            }

            AutoResetEvent autoResetEvent;
            if (!_event.TryGetValue(queueName, out autoResetEvent))
            {
                autoResetEvent = null;
            }

            DateTime start = DateTime.Now;
            TimeSpan sleep = _startSleep;
            
            while (true)
            {
                if (DateTime.Now - start > waitFor)
                {
                    return null;
                }

                bool signal = false;
                if (autoResetEvent == null)
                {
                    await Task.Delay(sleep);
                }
                else
                {
                    signal = autoResetEvent.WaitOne(sleep);
                }

                result = await method();
                if (result != null)
                {
                    return result;
                }

                if (sleep < waitFor && sleep < _maxSleep)
                {
                    sleep += _incrementSleep;
                }
            }
        }
    }
}

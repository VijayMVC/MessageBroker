using Khooversoft.Toolbox;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    public interface IMessageBrokerClient<T> where T : class
    {
        Uri BaseUri { get; }

        string AgentName { get; }

        string QueueName { get; }

        IMessageBrokerClient Client { get; }

        IMessageBrokerClient<T> WithAgent(string agentName);

        Task Enqueue(IWorkContext context, T message);

        Task<MessageEvent<T>> Dequeue(IWorkContext context, bool useSettle = true);

        Task Settle(IWorkContext context, long messageId, SettleType settleType, string errorMessage = null);

        Task Settle(IWorkContext context, MessageEvent<T> messageEvent, SettleType settleType, string errorMessage = null);

        Task ProcessDequeue(IWorkContext context, Func<IWorkContext, T, MessageEvent, Task<bool>> processMessage, CancellationToken token, bool useSettle = true, bool wait = true);
    }
}

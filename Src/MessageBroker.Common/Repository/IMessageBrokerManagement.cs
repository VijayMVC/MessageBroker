using Khooversoft.Toolbox;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public interface IMessageBrokerManagement
    {
        Task SetQueue(IWorkContext context, string queueName, int currentSizeLimit = 1000, int currentRetryLimit = 3, int lockValidForSec = 5 * 60);

        Task ClearQueue(IWorkContext context, string queueName, bool copyToHistory);

        Task DeleteQueue(IWorkContext context, string queueName);

        Task DisableQueue(IWorkContext context, string queueName);

        Task EnableQueue(IWorkContext context, string queueName);

        Task<InternalQueueManagementV1> GetQueue(IWorkContext context, string queueName);

        Task<IEnumerable<InternalQueueManagementV1>> GetQueueList(IWorkContext context, bool disable = false);

        Task<IEnumerable<InternalAgentV1>> GetAgents(IWorkContext context);

        Task<InternalHistoryV1> GetHistory(IWorkContext context, long messageId);

        Task<IEnumerable<InternalQueueStatusV1>> GetQueueStatus(IWorkContext context);
    }
}

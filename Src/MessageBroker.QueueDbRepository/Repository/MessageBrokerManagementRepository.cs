using Khooversoft.Sql;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageBroker.QueueDbRepository
{
    public class MessageBrokerManagementRepository : IMessageBrokerManagement
    {
        private readonly IMessageBrokerConfiguration _configuration;

        public MessageBrokerManagementRepository(IMessageBrokerConfiguration configuration)
        {
            Verify.IsNotNull(nameof(configuration), configuration);

            _configuration = configuration;
        }

        public Task ClearQueue(IWorkContext context, string queueName, bool copyToHistory)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            return new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[Clear-Queue]")
                .AddParameter(nameof(queueName), queueName)
                .AddParameter(nameof(copyToHistory), copyToHistory)
                .ExecuteNonQuery(context);
        }

        public Task DeleteQueue(IWorkContext context, string queueName)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            return new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[Delete-Queue]")
                .AddParameter(nameof(queueName), queueName)
                .ExecuteNonQuery(context);
        }

        public Task DisableQueue(IWorkContext context, string queueName)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            return new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[Disable-Queue]")
                .AddParameter(nameof(queueName), queueName)
                .AddParameter("disable", true)
                .ExecuteNonQuery(context);
        }

        public Task EnableQueue(IWorkContext context, string queueName)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            return new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[Disable-Queue]")
                .AddParameter(nameof(queueName), queueName)
                .AddParameter("disable", false)
                .ExecuteNonQuery(context);
        }

        public async Task<InternalQueueManagementV1> GetQueue(IWorkContext context, string queueName)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            IEnumerable<QueueManagementRow> rows = await new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[Get-Queue]")
                .AddParameter(nameof(queueName), queueName)
                .Execute(context, QueueManagementRow.Read);

            return rows.Select(x => x.ConvertTo())
                .FirstOrDefault();
        }

        public async Task<IEnumerable<InternalQueueManagementV1>> GetQueueList(IWorkContext context, bool disable = false)
        {
            IEnumerable<QueueManagementRow> rows = await new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[List-Queues]")
                .AddParameter(nameof(disable), disable)
                .Execute(context, QueueManagementRow.Read);

            return rows.Select(x => x.ConvertTo());
        }

        public Task SetQueue(IWorkContext context, string queueName, int currentSizeLimit = 1000, int currentRetryLimit = 3, int lockValidForSec = 5 * 60)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            return new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[Set-Queue]")
                .AddParameter(nameof(queueName), queueName)
                .AddParameter(nameof(currentSizeLimit), currentSizeLimit)
                .AddParameter(nameof(currentRetryLimit), currentRetryLimit)
                .AddParameter(nameof(lockValidForSec), lockValidForSec)
                .ExecuteNonQuery(context);
        }

        public async Task<IEnumerable<InternalAgentV1>> GetAgents(IWorkContext context)
        {
            IEnumerable<AgentRow> rows = await new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[List-Agents]")
                .Execute(context, AgentRow.Read);

            return rows.Select(x => x.ConvertTo());
        }

        public async Task<InternalHistoryV1> GetHistory(IWorkContext context, long messageId)
        {
            IEnumerable<HistoryRow> rows = await new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[Get-History]")
                .AddParameter(nameof(messageId), messageId)
                .Execute(context, HistoryRow.Read);

            return rows.Select(x => x.ConvertTo())
                .FirstOrDefault();
        }

        public async Task<IEnumerable<InternalQueueStatusV1>> GetQueueStatus(IWorkContext context)
        {
            IEnumerable<QueueStatusRow> rows = await new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[Get-QueueStatus]")
                .Execute(context, QueueStatusRow.Read);

            return rows.Select(x => x.ConvertTo());
        }
    }
}

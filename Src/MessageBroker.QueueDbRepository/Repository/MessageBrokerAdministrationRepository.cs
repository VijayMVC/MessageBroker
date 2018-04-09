using Khooversoft.Sql;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageBroker.QueueDbRepository
{
    public class MessageBrokerAdministrationRepository : IMessageBrokerAdministration
    {
        private readonly IMessageBrokerConfiguration _configuration;

        public MessageBrokerAdministrationRepository(IMessageBrokerConfiguration configuration)
        {
            Verify.IsNotNull(nameof(configuration), configuration);

            _configuration = configuration;
        }

        public async Task<int?> GetHistorySizeConfiguration(IWorkContext context)
        {
            IEnumerable<QueueConfigurationRow> rows = await new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[AppAdmin].[Get-HistorySizeConfiguration]")
                .Execute(context, QueueConfigurationRow.Read);

            return rows.Select(x => x.QueueSize).FirstOrDefault();
        }

        public Task ResetDatabase(IWorkContext context)
        {
            return new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[AppAdmin].[Reset-Database]")
                .ExecuteNonQuery(context);
        }

        public Task SetHistorySizeConfiguration(IWorkContext context, int queueSize)
        {
            return new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[AppAdmin].[Set-HistorySizeConfiguration]")
                .AddParameter(nameof(queueSize), queueSize)
                .ExecuteNonQuery(context);
        }
    }
}

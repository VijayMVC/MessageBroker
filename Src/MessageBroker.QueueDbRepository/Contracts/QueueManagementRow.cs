using Khooversoft.Sql;
using Khooversoft.Toolbox;
using System.Data.SqlClient;

namespace MessageBroker.QueueDbRepository
{
    public class QueueManagementRow
    {
        public int QueueId { get; set; }

        public string QueueName { get; set; }

        public int CurrentSizeLimit { get; set; }

        public int CurrentRetryLimit { get; set; }

        public int LockValidForSec { get; set; }

        public bool Disable { get; set; }

        public int? QueueLength { get; set; }

        public int? ScheduleQueueLength { get; set; }

        public static QueueManagementRow Read(IWorkContext context, SqlDataReader reader)
        {
            return new QueueManagementRow
            {
                QueueId = reader.Get<int>(nameof(QueueId)),
                QueueName = reader.Get<string>(nameof(QueueName)),
                CurrentSizeLimit = reader.Get<int>(nameof(CurrentSizeLimit)),
                CurrentRetryLimit = reader.Get<int>(nameof(CurrentRetryLimit)),
                LockValidForSec = reader.Get<int>(nameof(LockValidForSec)),
                Disable = reader.Get<bool>(nameof(Disable)),
                QueueLength = reader.Get<int?>(nameof(QueueLength), optional: true),
                ScheduleQueueLength = reader.Get<int?>(nameof(ScheduleQueueLength), optional: true),
            };
        }
    }
}

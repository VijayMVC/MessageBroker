using Khooversoft.Sql;
using Khooversoft.Toolbox;
using System;
using System.Data.SqlClient;

namespace MessageBroker.QueueDbRepository
{
    public class ScheduleRow
    {
        public long ScheduleId { get; set; }

        public int QueueId { get; set; }

        public string ClientMessageId { get; set; }

        public string Cv { get; set; }

        public string Payload { get; set; }

        public DateTime ScheduleDate { get; set; }

        public int CreatedByAgentId { get; set; }

        public DateTime _createdDate { get; set; }

        public static ScheduleRow Read(IWorkContext context, SqlDataReader reader)
        {
            return new ScheduleRow
            {
                ScheduleId = reader.Get<long>(nameof(ScheduleId)),
                QueueId = reader.Get<int>(nameof(QueueId)),
                ClientMessageId = reader.Get<string>(nameof(ClientMessageId)),
                Cv = reader.Get<string>(nameof(Cv)),
                Payload = reader.Get<string>(nameof(Payload)),
                ScheduleDate = reader.Get<DateTime>(nameof(ScheduleDate)),
                CreatedByAgentId = reader.Get<int>(nameof(CreatedByAgentId)),
                _createdDate = reader.Get<DateTime>(nameof(_createdDate)),
            };
        }
    }
}

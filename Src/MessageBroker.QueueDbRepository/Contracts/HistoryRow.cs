using Khooversoft.Sql;
using Khooversoft.Toolbox;
using System;
using System.Data.SqlClient;

namespace MessageBroker.QueueDbRepository
{
    public class HistoryRow
    {
        public long HistoryId { get; set; }

        public long MessageId { get; set; }

        public string ActivityType { get; set; }

        public string QueueName { get; set; }

        public string Cv { get; set; }

        public string ClientMessageId { get; set; }

        public string Payload { get; set; }

        public string SettleByAgent { get; set; }

        public string ErrorMessage { get; set; }

        public int RetryCount { get; set; }

        public DateTime _createdDate { get; set; }

        public static HistoryRow Read(IWorkContext context, SqlDataReader reader)
        {
            return new HistoryRow
            {
                HistoryId = reader.Get<long>(nameof(HistoryId)),
                MessageId = reader.Get<long>(nameof(MessageId)),
                ActivityType = reader.Get<string>(nameof(ActivityType)),
                QueueName = reader.Get<string>(nameof(QueueName)),
                Cv = reader.Get<string>(nameof(Cv)),
                ClientMessageId = reader.Get<string>(nameof(ClientMessageId)),
                Payload = reader.Get<string>(nameof(Payload)),
                SettleByAgent = reader.Get<string>(nameof(SettleByAgent)),
                ErrorMessage = reader.Get<string>(nameof(ErrorMessage)),
                RetryCount = reader.Get<int>(nameof(RetryCount)),
                _createdDate = reader.Get<DateTime>(nameof(_createdDate)),

            };
        }
    }
}

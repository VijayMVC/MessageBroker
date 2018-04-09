using Khooversoft.Sql;
using Khooversoft.Toolbox;
using System;
using System.Data.SqlClient;

namespace MessageBroker.QueueDbRepository
{
    public class MessageRow
    {
        public long MessageId { get; set; }

        public int QueueId { get; set; }

        public string ClientMessageId { get; set; }

        public string Cv { get; set; }

        public string Payload { get; set; }

        public DateTime? LockedDate { get; set; }

        public int? LockedByAgentId { get; set; }

        public int RetryCount { get; set; }

        public int CreatedByAgentId { get; set; }

        public DateTime _createdDate { get; set; }

        public static MessageRow Read(IWorkContext context, SqlDataReader reader)
        {
            return new MessageRow
            {
                MessageId = reader.Get<int>(nameof(MessageId)),
                QueueId = reader.Get<int>(nameof(QueueId)),
                ClientMessageId = reader.Get<string>(nameof(ClientMessageId)),
                Cv = reader.Get<string>(nameof(Cv)),
                Payload = reader.Get<string>(nameof(Payload)),
                LockedDate = reader.Get<DateTime?>(nameof(LockedDate)),
                LockedByAgentId = reader.Get<int?>(nameof(LockedByAgentId)),
                RetryCount = reader.Get<int>(nameof(RetryCount)),
                CreatedByAgentId = reader.Get<int>(nameof(CreatedByAgentId)),
                _createdDate = reader.Get<DateTime>(nameof(_createdDate)),
            };
        }
    }
}

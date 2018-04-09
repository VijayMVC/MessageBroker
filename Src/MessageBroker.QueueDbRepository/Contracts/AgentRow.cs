using Khooversoft.Sql;
using Khooversoft.Toolbox;
using System;
using System.Data.SqlClient;

namespace MessageBroker.QueueDbRepository
{
    public class AgentRow
    {
        public int AgentId { get; set; }

        public string AgentName { get; set; }

        public DateTime _createdDate { get; set; }

        public static AgentRow Read(IWorkContext context, SqlDataReader reader)
        {
            return new AgentRow
            {
                AgentId = reader.Get<int>(nameof(AgentId)),
                AgentName = reader.Get<string>(nameof(AgentName)),
                _createdDate = reader.Get<DateTime>(nameof(_createdDate))
            };
        }
    }
}

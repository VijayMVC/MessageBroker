using Khooversoft.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public class MessageBrokerConfiguration : IMessageBrokerConfiguration
    {
        public ISqlConfiguration SqlConfiguration { get; set; }

        public TimeSpan? DequeueWaitFor { get; set; }
    }
}

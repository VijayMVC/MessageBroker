using Khooversoft.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public interface IMessageBrokerConfiguration
    {
        ISqlConfiguration SqlConfiguration { get; set; }

        TimeSpan? DequeueWaitFor { get; set; }
    }
}

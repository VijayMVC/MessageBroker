using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    public class MessageBrokerClientConfiguration : IMessageBrokerClientConfiguration
    {
        public Uri BaseUri { get; set; }
    }
}

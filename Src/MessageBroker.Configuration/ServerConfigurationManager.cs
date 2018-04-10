using Khooversoft.Sql;
using MessageBroker.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Configuration
{
    public class ServerConfigurationManager
    {
        private Dictionary<MessageBrokerEnvironment, IMessageBrokerConfiguration> _environments = new Dictionary<MessageBrokerEnvironment, IMessageBrokerConfiguration>()
        {
            [MessageBrokerEnvironment.Test] = new MessageBrokerConfiguration
            {
                SqlConfiguration = new SqlConfiguration("localhost", "QueueDb_Test"),
                DequeueWaitFor = TimeSpan.FromSeconds(30),
            },
            [MessageBrokerEnvironment.Local] = new MessageBrokerConfiguration
            {
                SqlConfiguration = new SqlConfiguration("localhost", "MessageBrokerQueue"),
                DequeueWaitFor = TimeSpan.FromSeconds(30),
            },
            [MessageBrokerEnvironment.PPE] = new MessageBrokerConfiguration
            {
                SqlConfiguration = new SqlConfiguration("<enter server name>", "MessageBrokerQueue"),
                DequeueWaitFor = TimeSpan.FromSeconds(30),
            },
        };

        public IMessageBrokerConfiguration GetConfiguration(MessageBrokerEnvironment environment)
        {
            IMessageBrokerConfiguration configuration;
            if (!_environments.TryGetValue(environment, out configuration))
            {
                throw new ArgumentException($"Unknown environment {environment}");
            }

            return configuration;
        }
    }
}

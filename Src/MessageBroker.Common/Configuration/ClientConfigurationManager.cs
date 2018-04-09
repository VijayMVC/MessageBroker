using MessageBroker.Common.RestApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public class ClientConfigurationManager
    {
        private Dictionary<MessageBrokerEnvironment, IMessageBrokerClientConfiguration> _environments = new Dictionary<MessageBrokerEnvironment, IMessageBrokerClientConfiguration>
        {
            [MessageBrokerEnvironment.Test] = new MessageBrokerClientConfiguration
            {
                BaseUri = new Uri("http://localhost:9985"),
            },
            [MessageBrokerEnvironment.Local] = new MessageBrokerClientConfiguration
            {
                BaseUri = new Uri("http://localhost:8108"),
            },
            [MessageBrokerEnvironment.Dark] = new MessageBrokerClientConfiguration
            {
                BaseUri = new Uri("http://dv-3gdjs22.atgcorporate.com:8108"),
            },
        };

        public IMessageBrokerClientConfiguration Get(MessageBrokerEnvironment environment)
        {
            IMessageBrokerClientConfiguration configuration;
            if (!_environments.TryGetValue(environment, out configuration))
            {
                throw new ArgumentException($"Unknown environment {environment}");
            }

            return configuration;
        }
    }
}

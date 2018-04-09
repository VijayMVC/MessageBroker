using Autofac;
using Khooversoft.Net;
using MessageBroker.Common.RestApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public class MessageBrokerClientAutoFacModule : Module
    {
        private readonly MessageBrokerEnvironment _environment;

        public MessageBrokerClientAutoFacModule(MessageBrokerEnvironment environment)
        {
            _environment = environment;
        }

        protected override void Load(ContainerBuilder builder)
        {
            IMessageBrokerClientConfiguration environmentConfiguration = new ClientConfigurationManager().Get(_environment);

            builder.RegisterInstance(environmentConfiguration).As<IMessageBrokerClientConfiguration>();
            builder.RegisterInstance(new RestClientConfigurationBuilder().Build()).As<IRestClientConfiguration>();
            builder.RegisterType<MessageBrokerClient>().As<IMessageBrokerClient>();
        }
    }
}

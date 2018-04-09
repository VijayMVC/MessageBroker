using Khooversoft.Net;
using Khooversoft.Toolbox;

namespace MessageBroker.Common.RestApi
{
    /// <summary>
    /// Client for message broker API
    /// </summary>
    public class MessageBrokerClient : IMessageBrokerClient
    {
        private readonly IRestClientConfiguration _restClientConfiguration;

        public MessageBrokerClient(IMessageBrokerClientConfiguration clientConfiguration, IRestClientConfiguration restClientConfiguration)
        {
            Verify.IsNotNull(nameof(clientConfiguration), clientConfiguration);
            Verify.IsNotNull(nameof(restClientConfiguration), restClientConfiguration);

            ClientConfiguration = clientConfiguration;
            _restClientConfiguration = restClientConfiguration;

            Administration = new MessageBrokerAdministrationApi(ClientConfiguration.BaseUri, _restClientConfiguration);
            Agent = new MessageBrokerAgentApi(ClientConfiguration.BaseUri, _restClientConfiguration);
            Management = new MessageBrokerManagementApi(ClientConfiguration.BaseUri, _restClientConfiguration);
            Message = new MessageBrokerMessageApi(ClientConfiguration.BaseUri, _restClientConfiguration);
            History = new MessageBrokerHistoryApi(ClientConfiguration.BaseUri, _restClientConfiguration);
        }

        /// <summary>
        /// Client configuration
        /// </summary>
        public IMessageBrokerClientConfiguration ClientConfiguration { get; }

        /// <summary>
        /// Administration API
        /// </summary>
        public IMessageBrokerAdministrationApi Administration { get; }

        /// <summary>
        /// Agent API
        /// </summary>
        public IMessageBrokerAgentApi Agent { get; }

        /// <summary>
        /// Management API
        /// </summary>
        public IMessageBrokerManagementApi Management { get; }

        /// <summary>
        /// Message API
        /// </summary>
        public IMessageBrokerMessageApi Message { get; }

        /// <summary>
        /// History API
        /// </summary>
        public IMessageBrokerHistoryApi History { get; }
    }
}

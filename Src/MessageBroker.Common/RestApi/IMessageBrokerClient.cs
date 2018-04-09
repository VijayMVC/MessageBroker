namespace MessageBroker.Common.RestApi
{
    public interface IMessageBrokerClient
    {
        IMessageBrokerClientConfiguration ClientConfiguration { get; }

        IMessageBrokerAdministrationApi Administration { get; }

        IMessageBrokerAgentApi Agent { get; }

        IMessageBrokerManagementApi Management { get; }

        IMessageBrokerMessageApi Message { get; }

        IMessageBrokerHistoryApi History { get; }
    }
}

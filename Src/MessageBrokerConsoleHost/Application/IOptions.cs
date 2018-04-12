using MessageBroker.Common;

namespace MessageBrokerConsoleHost
{
    public interface IOptions
    {
        MessageBrokerEnvironment Environment { get; }
    }
}

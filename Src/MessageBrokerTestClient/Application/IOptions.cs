using MessageBroker.Common;
using System;

namespace MessageBrokerTestClient
{
    internal interface IOptions
    {
        MessageBrokerEnvironment Environment { get; }

        RunMode? Run { get; }

        string AgentName { get; }

        string QueueName { get; }

        TimeSpan Delay { get; }

        bool ShowDetail { get; }

        int ClientCount { get; }

        TimeSpan SampleRate { get; }

        bool NoLock { get; }

        bool NoWait { get; }
    }
}

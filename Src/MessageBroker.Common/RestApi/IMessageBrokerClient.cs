// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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

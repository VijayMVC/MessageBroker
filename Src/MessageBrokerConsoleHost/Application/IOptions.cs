// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using MessageBroker.Common;

namespace MessageBrokerConsoleHost
{
    public interface IOptions
    {
        MessageBrokerEnvironment Environment { get; }
    }
}

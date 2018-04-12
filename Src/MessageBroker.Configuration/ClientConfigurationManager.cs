// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using MessageBroker.Common;
using MessageBroker.Common.RestApi;
using System;
using System.Collections.Generic;

namespace MessageBroker.Configuration
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
            [MessageBrokerEnvironment.PPE] = new MessageBrokerClientConfiguration
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

// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    public class MessageBrokerClientConfiguration : IMessageBrokerClientConfiguration
    {
        public Uri BaseUri { get; set; }
    }
}

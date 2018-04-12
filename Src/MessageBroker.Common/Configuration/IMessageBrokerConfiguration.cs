// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public interface IMessageBrokerConfiguration
    {
        ISqlConfiguration SqlConfiguration { get; set; }

        TimeSpan? DequeueWaitFor { get; set; }
    }
}

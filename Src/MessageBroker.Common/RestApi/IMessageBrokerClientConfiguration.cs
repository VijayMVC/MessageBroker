﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    public interface IMessageBrokerClientConfiguration
    {
        Uri BaseUri { get; }
    }
}
// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBrokerTestClient
{
    internal static class Utility
    {
        public static async Task Delay(TimeSpan delay, CancellationToken token)
        {
            try
            {
                if (delay != TimeSpan.Zero)
                {
                    await Task.Delay(delay, token);
                }
            }
            catch { }
        }
    }
}

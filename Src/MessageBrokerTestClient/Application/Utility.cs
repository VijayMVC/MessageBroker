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

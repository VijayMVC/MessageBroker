// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MessageBroker.Common;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MessageBrokerConsoleHost
{
    class Program
    {
        private const int ReturnOk = 0;
        private const int ReturnError = 1;

        static int Main(string[] args)
        {
            return new Program().Run(args);
        }

        private int Run(string[] args)
        {
            Console.WriteLine($"Message Broker Console Host - Version {Assembly.GetExecutingAssembly().GetName().Version}");
            Console.WriteLine();

            try
            {
                Options option = Options.Parse(args);
                if (option.Help)
                {
                    Options.DisplayHelp();
                    return ReturnOk;
                }

                Console.WriteLine($"Selected environment: {option.Environment}");

                IWebHost host = WebHost.CreateDefaultBuilder()
                    .UseSetting(nameof(MessageBrokerEnvironment), option.Environment.ToString())
                    .UseUrls(option.HostUri.ToString())
                    .UseStartup<MessageBrokerApi.Startup>()
                    .Build();

                host.Run();
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return ReturnError;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhanded exception: {ex.Message}, {ex}");
                return ReturnError;
            }

            return ReturnOk;
        }
    }
}

// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Autofac;
using Khooversoft.Toolbox;
using MessageBroker.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBrokerTestClient
{
    class Program
    {
        private const int ReturnOk = 0;
        private const int ReturnError = 1;
        private readonly Tag _tag = new Tag(nameof(Program));
        private readonly IWorkContext _workContext = WorkContext.Empty;

        static int Main(string[] args)
        {
            return new Program().Run(args);
        }

        private int Run(string[] args)
        {
            Console.WriteLine($"Message Broker Client - Version {Assembly.GetExecutingAssembly().GetName().Version}");
            Console.WriteLine();
            IWorkContext context = _workContext.WithTag(_tag);

            try
            {
                Options option = Options.Parse(args);
                if (option.Help || option.Run == null)
                {
                    Options.DisplayHelp();
                    return ReturnOk;
                }

                option.DisplaySetting();

                using (var container = BuildContainer(option).BeginLifetimeScope())
                {
                    switch (option.Run)
                    {
                        case RunMode.Send:
                            var sendAction = container.Resolve<SendTestMessages>();
                            Launch(context, sendAction.Run);
                            break;

                        case RunMode.Receive:
                            var receiveAction = container.Resolve<ReceiveTestMessage>();
                            Launch(context, receiveAction.Run);
                            break;

                        default:
                            throw new ArgumentException("Unknown run argument");
                    }
                }
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

        private IContainer BuildContainer(Options options)
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(options).As<IOptions>();
            builder.RegisterModule(new MessageBrokerClientAutoFacModule(options.Environment));
            builder.RegisterType<ReceiveTestMessage>();
            builder.RegisterType<SendTestMessages>();

            return builder.Build();
        }

        private void Launch(IWorkContext context, Func<IWorkContext, CancellationToken, Task> action)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotNull(nameof(action), action);
            context = context.WithTag(_tag);

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            Task runTask = Task.Run(() => action(context, tokenSource.Token));

            Console.WriteLine();
            Console.WriteLine("Starting... Press <return> to quit");

            while (!runTask.IsCompleted)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            }

            tokenSource.Cancel();
            runTask.GetAwaiter().GetResult();
            Console.WriteLine();
        }
    }
}

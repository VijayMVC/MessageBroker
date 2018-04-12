// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace MessageBrokerApi
{
    ///// <summary>
    ///// The FabricRuntime creates an instance of this class for each service type instance. 
    ///// </summary>
    //internal sealed class MessageBrokerApi : StatelessService
    //{
    //    public MessageBrokerApi(StatelessServiceContext context)
    //        : base(context)
    //    { }

    //    /// <summary>
    //    /// Optional override to create listeners (like tcp, http) for this service instance.
    //    /// </summary>
    //    /// <returns>The collection of listeners.</returns>
    //    protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
    //    {
    //        return new ServiceInstanceListener[]
    //        {
    //            new ServiceInstanceListener(serviceContext =>
    //                new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
    //                {
    //                    ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

    //                    return new WebHostBuilder()
    //                                .UseKestrel()
    //                                .ConfigureServices(
    //                                    services => services
    //                                        .AddSingleton<StatelessServiceContext>(serviceContext))
    //                                .UseContentRoot(Directory.GetCurrentDirectory())
    //                                .UseStartup<Startup>()
    //                                .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
    //                                .UseUrls(url)
    //                                .Build();
    //                }))
    //        };
    //    }
    //}

    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class MessageBrokerApi : StatelessService
    {
        public MessageBrokerApi(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            MessageBrokerEnvironment environment = GetEnvironment();

            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        return new WebHostBuilder()
                                    .UseKestrel()
                                    .ConfigureServices(services => services.AddSingleton<StatelessServiceContext>(serviceContext))
                                    .UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseSetting(nameof(MessageBrokerEnvironment), environment.ToString())
                                    .UseStartup<Startup>()
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }

        private MessageBrokerEnvironment GetEnvironment()
        {
            CodePackageActivationContext context = FabricRuntime.GetActivationContext();
            ConfigurationPackage packageContext = context.GetConfigurationPackageObject("Config");

            string environment = packageContext.Settings.Sections["ConfigSection"].Parameters["Environment"].Value;
            Verify.IsNotEmpty(nameof(environment), environment);

            ServiceEventSource.Current.ServiceMessage(Context, $"Environment selected: {environment}");
            return (MessageBrokerEnvironment)Enum.Parse(typeof(MessageBrokerEnvironment), environment, true);
        }
    }
}

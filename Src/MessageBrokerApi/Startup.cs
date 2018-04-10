using Autofac;
using Autofac.Extensions.DependencyInjection;
using Khooversoft.AspMvc;
using Khooversoft.Net;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using MessageBroker.Configuration;
using MessageBroker.QueueDbRepository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using swagger = Swashbuckle.AspNetCore.Swagger;

namespace MessageBrokerApi
{
    /// <summary>
    /// Start up for REST API
    /// </summary>
    public class Startup
    {
        private readonly Tag _tag = new Tag(nameof(Startup));
        private readonly IWorkContext _workContext = WorkContext.Empty;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">current configuration</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            SetEnvironment();
        }

        internal MessageBrokerEnvironment Environment { get; }

        internal IConfiguration Configuration { get; }

        internal IMessageBrokerConfiguration MessageBrokerConfiguration { get; private set; }

        internal MessageBrokerEnvironment MessageBrokerEnvironment { get; private set; }

        internal IContainer ApplicationContainer { get; private set; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">services</param>
        /// <returns>service provider</returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var context = _workContext.WithTag(_tag);

            services.AddMvc()
                .AddJsonOptions(x => x.SerializerSettings.Converters.Add(new StringEnumConverter { CamelCaseText = true }));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new swagger.Info { Title = "Message Broker API", Version = "v1" });

                string basePath = AppContext.BaseDirectory;

                IEnumerable<string> files = Directory.EnumerateFiles(basePath, "MessageBrokerApi.xml", SearchOption.AllDirectories);
                if (files == null || files.Count() == 0)
                {
                    string msg = $"Cannot find XML document file searching directory. basePath={basePath}";
                    MessageBrokerApiEventSource.Log.Error(context, msg);
                    throw new FileNotFoundException(msg, basePath);
                }


                string xmlPath = files.First();
                if (!File.Exists(xmlPath))
                {
                    string msg = $"Cannot find XML document file: {xmlPath}, basePath={basePath}";
                    MessageBrokerApiEventSource.Log.Error(context, msg);
                    throw new FileNotFoundException(msg, xmlPath);
                }

                c.IncludeXmlComments(xmlPath);
            });


            // Build AutoFac container
            var builder = new ContainerBuilder();
            builder.Populate(services);

            builder.Register(cts => MessageBrokerConfiguration).As<IMessageBrokerConfiguration>().SingleInstance();
            builder.RegisterType<MessageBrokerRepository>().As<IMessageBroker>().SingleInstance();
            builder.RegisterType<MessageBrokerManagementRepository>().As<IMessageBrokerManagement>().SingleInstance();
            builder.RegisterType<MessageBrokerAdministrationRepository>().As<IMessageBrokerAdministration>().SingleInstance();
            builder.RegisterType<MessageManager>().As<IMessageManager>().SingleInstance();
            builder.Register(cts => new ServiceConfiguration { Container = ApplicationContainer }).As<IServiceConfiguration>().SingleInstance();
            builder.Register(cts => new HeaderFactory()).As<IHeaderFactory>().SingleInstance();

            ApplicationContainer = builder.Build();
            return new AutofacServiceProvider(ApplicationContainer);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">application</param>
        /// <param name="env">environment</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<SetupMiddleware>();
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Message Broker API V1"));

        }

        private void SetEnvironment()
        {
            IConfigurationRoot root = Configuration as IConfigurationRoot;
            if (root == null)
            {
                return;
            }

            MemoryConfigurationProvider provider = root.Providers.OfType<MemoryConfigurationProvider>().FirstOrDefault();
            if (provider == null)
            {
                return;
            }

            string environment;
            if (!provider.TryGet(nameof(MessageBrokerEnvironment), out environment))
            {
                throw new Exception("Environment not specified");
            }

            environment = environment ?? "Test";
            MessageBrokerEnvironment = environment.Parse<MessageBrokerEnvironment>(ignoreCase: true);
            MessageBrokerConfiguration = new ServerConfigurationManager().GetConfiguration(MessageBrokerEnvironment);
        }
    }
}

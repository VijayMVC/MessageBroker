using Khooversoft.Net;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using MessageBroker.Common.RestApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBroker.Api.Test
{
    public static class TestAssembly
    {
        private static TestServer _testServer;
        private static HttpClient _client;
        private static HttpMessageHandler _httpMessageHandler;
        private static IRestClientConfiguration _restClientConfiguration;
        private static readonly Tag _tag = new Tag(nameof(TestAssembly));

        public static IMessageBrokerClientConfiguration ClientConfiguration = new MessageBrokerClientConfiguration
        {
            BaseUri = new Uri("http://localhost:8080"),
        };

        public static void StartApiHost()
        {
            if (_client != null)
            {
                return;
            }

            _testServer = new TestServer(new WebHostBuilder()
                .UseSetting(nameof(MessageBrokerEnvironment), MessageBrokerEnvironment.Test.ToString())
                .UseStartup<MessageBrokerApi.Startup>());

            _client = _testServer.CreateClient();
            _httpMessageHandler = _testServer.CreateHandler();

            _restClientConfiguration = new RestClientConfigurationBuilder()
                .SetMessageHandler(_testServer.CreateHandler())
                .Build();
        }

        public static IRestClientConfiguration GetRestClientConfiguration()
        {
            StartApiHost();
            return _restClientConfiguration;
        }
    }
}

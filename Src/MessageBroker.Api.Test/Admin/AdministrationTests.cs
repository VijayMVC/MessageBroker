using FluentAssertions;
using Khooversoft.Toolbox;
using MessageBroker.Common.RestApi;
using System.Threading.Tasks;
using Xunit;

namespace MessageBroker.Api.Test.Admin
{
    [Collection("General")]
    public class AdministrationTests
    {
        private readonly Tag _tag = new Tag(nameof(AdministrationTests));
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private MessageBrokerClient _client;

        public AdministrationTests()
        {
            _client = new MessageBrokerClient(TestAssembly.ClientConfiguration, TestAssembly.GetRestClientConfiguration());

            ResetDatabase()
                .GetAwaiter()
                .GetResult();
        }

        private async Task ResetDatabase()
        {
            var context = _workContext.WithTag(_tag);

            await _client.Administration.ResetDatabase(context);
        }

        [Fact]
        public async Task SetHistorySizeConfigurationSuccessTestApi()
        {
            var context = _workContext.WithTag(_tag);

            const int newSize = 9999;
            await _client.Administration.SetHistorySizeConfiguration(context, newSize);

            int? size = (await _client.Administration.GetHistorySizeConfiguration(context))?.Value?.HistorySize;
            size.Should().NotBeNull();
            size.Value.Should().Be(newSize);
        }
    }
}

using FluentAssertions;
using Khooversoft.Net;
using Khooversoft.Toolbox;
using MessageBroker.Api.Test;
using MessageBroker.Common;
using MessageBroker.Common.RestApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBroker.QueueDbRepository.Test.Admin
{
    [Collection("General")]
    public class AgentTests
    {
        private readonly Tag _tag = new Tag(nameof(AgentTests));
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private MessageBrokerClient _client;

        public AgentTests()
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
        public async Task GetAgentSucessTestApi()
        {
            var context = _workContext.WithTag(_tag);

            IEnumerable<string> agentNames = new string[] { "test-agent1", "test-agent2", "test-agent3" };
            var ids = new HashSet<int>();

            foreach (var item in agentNames)
            {
                int agentId = (await _client.Message.GetAgentId(context, "test-queue", item)).Value.AgentId;
                ids.Contains(agentId).Should().BeFalse();
                ids.Add(agentId);
            }

            RestResponse<RestPageResultV1<AgentContractV1>> rows = await _client.Agent.GetAgents(context);
            rows.Should().NotBeNull();
            rows.Value.Items.Count().Should().Be(3);

            rows.Value.Items
                .Select(x => x.AgentName)
                .OrderBy(x => x)
                .Zip(agentNames.OrderBy(x => x), (f, s) => new Tuple<string, string>(f, s))
                .All(x => x.Item1 == x.Item2)
                .Should().BeTrue();
        }
    }
}

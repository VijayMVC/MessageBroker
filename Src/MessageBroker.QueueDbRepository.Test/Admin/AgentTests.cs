// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentAssertions;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MessageBroker.QueueDbRepository.Test.Admin
{
    public class AgentTests
    {
        private readonly Tag _tag = new Tag(nameof(AdministrationTests));
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private readonly IMessageBrokerAdministration _admin = TestAssembly.Administration;
        private readonly IMessageBrokerManagement _management = TestAssembly.Management;
        private readonly IMessageBroker _message = TestAssembly.Message;

        public AgentTests()
        {
            var context = _workContext.WithTag(_tag);

            _admin.ResetDatabase(context)
                .GetAwaiter()
                .GetResult();
        }

        [Fact]
        public async Task GetAgentSucessTest()
        {
            var context = _workContext.WithTag(_tag);

            IEnumerable<string> agentNames = new string[] { "test-agent1", "test-agent2", "test-agent3" };
            var ids = new HashSet<int>();

            foreach (var item in agentNames)
            {
                int agentId = await _message.GetAgentId(context, item);
                ids.Contains(agentId).Should().BeFalse();
                ids.Add(agentId);
            }

            IEnumerable<InternalAgentV1> rows = await _management.GetAgents(context);
            rows.Should().NotBeNull();
            rows.Count().Should().Be(3);

            rows
                .Select(x => x.AgentName)
                .OrderBy(x => x)
                .Zip(agentNames.OrderBy(x => x), (f, s) => new Tuple<string, string>(f, s))
                .All(x => x.Item1 == x.Item2)
                .Should().BeTrue();
        }
    }
}

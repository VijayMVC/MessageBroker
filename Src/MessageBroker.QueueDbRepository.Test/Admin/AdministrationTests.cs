// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentAssertions;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using System.Threading.Tasks;
using Xunit;

namespace MessageBroker.QueueDbRepository.Test.Admin
{
    public class AdministrationTests
    {
        private readonly Tag _tag = new Tag(nameof(AdministrationTests));
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private readonly IMessageBrokerAdministration _admin = TestAssembly.Administration;

        public AdministrationTests()
        {
            var context = _workContext.WithTag(_tag);

            _admin.ResetDatabase(context)
                .GetAwaiter()
                .GetResult();
        }

        [Fact]
        public async Task SetHistorySizeConfigurationSuccessTest()
        {
            var context = _workContext.WithTag(_tag);

            const int newSize = 9999;
            await _admin.SetHistorySizeConfiguration(context, newSize);

            int? size = await _admin.GetHistorySizeConfiguration(context);
            size.Should().NotBeNull();
            size.Value.Should().Be(newSize);
        }
    }
}

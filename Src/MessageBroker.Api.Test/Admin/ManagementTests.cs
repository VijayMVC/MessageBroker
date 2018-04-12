// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using FluentAssertions;
using Khooversoft.Net;
using Khooversoft.Toolbox;
using MessageBroker.Api.Test;
using MessageBroker.Common;
using MessageBroker.Common.RestApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace MessageBroker.QueueDbRepository.Test.Admin
{
    [Collection("General")]
    public class ManagementTests
    {
        private readonly Tag _tag = new Tag(nameof(ManagementTests));
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private MessageBrokerClient _client;

        public ManagementTests()
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
            await _client.Administration.SetHistorySizeConfiguration(context, 1000);
        }

        [Fact]
        public async Task HealthCheckTestApi()
        {
            var context = _workContext.WithTag(_tag);

            RestResponse<HealthCheckContractV1> result = await _client.Management.HealthCheck(context);
            result.Value.Status.Should().Be("ok");
            result.Value.RepositoryHealthCheck.Should().Be("ok");
        }

        [Fact]
        public async Task SetQueueSuccessTestsApi()
        {
            var context = _workContext.WithTag(_tag);

            const string queueName = "test-queue";

            await Verify.AssertExceptionAsync<RestNotFoundException>(async () => await _client.Management.GetQueue(context, queueName));

            RestResponse<QueueDetailContractV1> check = await _client.Management.GetQueue(context, queueName, new HttpStatusCode[] { HttpStatusCode.NotFound });
            check.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var request = new SetQueueContractV1
            {
                QueueName = queueName,
                CurrentSizeLimit = 10,
                CurrentRetryLimit = 11,
                LockValidForSec = 12,
            };

            await _client.Management.SetQueue(context, request);

            QueueDetailContractV1 result = (await _client.Management.GetQueue(context, queueName)).Value;
            result.Should().NotBeNull();
            result.QueueName.Should().Be(queueName);
            result.CurrentSizeLimit.Should().Be(10);
            result.CurrentRetryLimit.Should().Be(11);
            result.LockValidForSec.Should().Be(12);
            result.Disable.Should().BeFalse();
            result.QueueLength.Should().Be(0);

            await _client.Management.DeleteQueue(context, queueName);

            await Verify.AssertExceptionAsync<RestNotFoundException>(async () => await _client.Management.GetQueue(context, queueName));
        }

        [Fact]
        public async Task SetDuplicateQueueFailedTestsApi()
        {
            IEnumerable<QueueDetailContractV1> rows;
            var context = _workContext.WithTag(_tag);

            rows = (await _client.Management.GetQueueList(context)).Value.Items;
            rows.Should().NotBeNull();
            rows.Count().Should().Be(0);

            const string queueName = "test-queue";
            var request = new SetQueueContractV1
            {
                QueueName = queueName,
                CurrentSizeLimit = 10,
                CurrentRetryLimit = 11,
                LockValidForSec = 12,
            };

            await _client.Management.SetQueue(context, request);

            QueueDetailContractV1 result = (await _client.Management.GetQueue(context, queueName)).Value;
            result.Should().NotBeNull();
            result.QueueName.Should().Be(queueName);

            rows = (await _client.Management.GetQueueList(context)).Value.Items;
            rows.Should().NotBeNull();
            rows.Count().Should().Be(1);

            await _client.Management.SetQueue(context, request);

            rows = (await _client.Management.GetQueueList(context)).Value.Items;
            rows.Should().NotBeNull();
            rows.Count().Should().Be(1);

            await _client.Management.DeleteQueue(context, queueName);

            rows = (await _client.Management.GetQueueList(context)).Value.Items;
            rows.Should().NotBeNull();
            rows.Count().Should().Be(0);

            await Verify.AssertExceptionAsync<RestNotFoundException>(async () => await _client.Management.GetQueue(context, queueName));
        }

        [Fact]
        public async Task DisableQueueSuccessTestApi()
        {
            QueueDetailContractV1 result;
            IEnumerable<QueueDetailContractV1> list;
            var context = _workContext.WithTag(_tag);

            list = (await _client.Management.GetQueueList(context, false)).Value.Items;
            list.Should().NotBeNull();
            list.Count().Should().Be(0);

            list = (await _client.Management.GetQueueList(context, true)).Value.Items;
            list.Should().NotBeNull();
            list.Count().Should().Be(0);

            const string queueName = "test-queue";
            var request = new SetQueueContractV1
            {
                QueueName = queueName,
                CurrentSizeLimit = 10,
                CurrentRetryLimit = 11,
                LockValidForSec = 12,
            };

            await _client.Management.SetQueue(context, request);

            list = (await _client.Management.GetQueueList(context, false)).Value.Items;
            list.Should().NotBeNull();
            list.Count().Should().Be(1);

            result = (await _client.Management.GetQueue(context, queueName)).Value;
            result.Should().NotBeNull();
            result.QueueName.Should().Be(queueName);

            await _client.Management.DisableQueue(context, queueName);

            await Verify.AssertExceptionAsync<RestNotFoundException>(async () => await _client.Management.GetQueue(context, queueName));

            list = (await _client.Management.GetQueueList(context, true)).Value.Items;
            list.Should().NotBeNull();
            list.Count().Should().Be(1);
            result = list.First();
            result.QueueName.Should().Be(queueName);
            result.Disable.Should().BeTrue();

            await _client.Management.EnableQueue(context, queueName);

            result = (await _client.Management.GetQueue(context, queueName)).Value;
            result.Should().NotBeNull();
            result.QueueName.Should().Be(queueName);

            list = (await _client.Management.GetQueueList(context, false)).Value.Items;
            list.Should().NotBeNull();
            list.Count().Should().Be(1);
        }

        [Fact]
        public async Task MultipleQueueSuccessTestApi()
        {
            var context = _workContext.WithTag(_tag);

            IEnumerable<string> queueNames = new string[] { "test-queue1", "test-queue2", "test-queue3" };

            foreach (var item in queueNames)
            {
                var request = new SetQueueContractV1
                {
                    QueueName = item,
                    CurrentSizeLimit = 10,
                    CurrentRetryLimit = 11,
                    LockValidForSec = 12,
                };

                await _client.Management.SetQueue(context, request);
            }

            IEnumerable<QueueDetailContractV1> rows = (await _client.Management.GetQueueList(context)).Value.Items;
            rows.Should().NotBeNull();
            rows.Count().Should().Be(queueNames.Count());

            rows
                .OrderBy(x => x.QueueName)
                .Zip(queueNames.OrderBy(x => x), (f, s) => new Tuple<string, string>(f.QueueName, s))
                .All(x => x.Item1 == x.Item2)
                .Should().BeTrue();

            foreach (var item in queueNames)
            {
                await _client.Management.DeleteQueue(context, item);
            }

            rows = (await _client.Management.GetQueueList(context)).Value.Items;
            rows.Should().NotBeNull();
            rows.Count().Should().Be(0);
        }

        [Fact]
        public async Task MultipleDisableQueueSuccessTestApi()
        {
            var context = _workContext.WithTag(_tag);

            IEnumerable<string> queueNames = new string[] { "test-queue1", "test-queue2", "test-queue3" };

            foreach (var item in queueNames)
            {
                var request = new SetQueueContractV1
                {
                    QueueName = item,
                    CurrentSizeLimit = 10,
                    CurrentRetryLimit = 11,
                    LockValidForSec = 12,
                };

                await _client.Management.SetQueue(context, request);
            }

            IEnumerable<QueueDetailContractV1> rows = (await _client.Management.GetQueueList(context)).Value.Items;
            rows.Should().NotBeNull();
            rows.Count().Should().Be(queueNames.Count());

            rows
                .OrderBy(x => x.QueueName)
                .Zip(queueNames.OrderBy(x => x), (f, s) => new Tuple<string, string>(f.QueueName, s))
                .All(x => x.Item1 == x.Item2)
                .Should().BeTrue();

            foreach (var item in queueNames)
            {
                await _client.Management.DisableQueue(context, item);
            }

            rows = (await _client.Management.GetQueueList(context)).Value.Items;
            rows.Should().NotBeNull();
            rows.Count().Should().Be(0);

            rows = (await _client.Management.GetQueueList(context, disable: true)).Value.Items;
            rows.Should().NotBeNull();
            rows.Count().Should().Be(3);

            foreach (var item in queueNames)
            {
                await _client.Management.EnableQueue(context, item);
            }

            rows = (await _client.Management.GetQueueList(context)).Value.Items;
            rows.Should().NotBeNull();
            rows.Count().Should().Be(queueNames.Count());

            rows
                .OrderBy(x => x.QueueName)
                .Zip(queueNames.OrderBy(x => x), (f, s) => new Tuple<string, string>(f.QueueName, s))
                .All(x => x.Item1 == x.Item2)
                .Should().BeTrue();
        }

        [Fact]
        public async Task GetQueueStatusTestApi()
        {
            var context = _workContext.WithTag(_tag);

            const string queueName = "test-queue";
            var request = new SetQueueContractV1
            {
                QueueName = queueName,
                CurrentSizeLimit = 10,
                CurrentRetryLimit = 11,
                LockValidForSec = 12,
            };

            await _client.Management.SetQueue(context, request);

            IEnumerable<QueueStatusContractV1> result = (await _client.Management.GetQueueStatus(context)).Value.Items;
            result.Should().NotBeNull();
            result.Count().Should().Be(1);
            result.First().QueueName.Should().Be(queueName);

            const string queueName2 = "test-queue-2";
            request = new SetQueueContractV1
            {
                QueueName = queueName2,
                CurrentSizeLimit = 10,
                CurrentRetryLimit = 11,
                LockValidForSec = 12,
            };

            await _client.Management.SetQueue(context, request);

            result = (await _client.Management.GetQueueStatus(context)).Value.Items;
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.Count(x => x.QueueName == queueName || x.QueueName == queueName2).Should().Be(2);
        }
    }
}

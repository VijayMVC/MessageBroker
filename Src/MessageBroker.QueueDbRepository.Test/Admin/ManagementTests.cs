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
    public class ManagementTests
    {
        private readonly Tag _tag = new Tag(nameof(AdministrationTests));
        private readonly IWorkContext _workContext = WorkContext.Empty;
        private readonly IMessageBrokerAdministration _admin = Repository.Administration;
        private readonly IMessageBrokerManagement _management = Repository.Management;

        public ManagementTests()
        {
            ResetDatabase()
                .GetAwaiter()
                .GetResult();
        }

        private async Task ResetDatabase()
        {
            var context = _workContext.WithTag(_tag);

            await _admin.ResetDatabase(context);
            await _admin.SetHistorySizeConfiguration(context, 1000);
        }

        [Fact]
        public async Task SetQueueSuccessTests()
        {
            var context = _workContext.WithTag(_tag);

            const string queueName = "test-queue";
            await _management.SetQueue(context, queueName, 10, 11, 12);

            InternalQueueManagementV1 result = await _management.GetQueue(context, queueName);
            result.Should().NotBeNull();
            result.QueueName.Should().Be(queueName);
            result.CurrentSizeLimit.Should().Be(10);
            result.CurrentRetryLimit.Should().Be(11);
            result.LockValidForSec.Should().Be(12);
            result.Disable.Should().BeFalse();
            result.QueueLength.Should().Be(0);

            await _management.DeleteQueue(context, queueName);

            result = await _management.GetQueue(context, queueName);
            result.Should().BeNull();
        }

        [Fact]
        public async Task SetDuplicateQueueFailedTests()
        {
            IEnumerable<InternalQueueManagementV1> rows;
            var context = _workContext.WithTag(_tag);

            rows = await _management.GetQueueList(context);
            rows.Should().NotBeNull();
            rows.Count().Should().Be(0);

            const string queueName = "test-queue";
            await _management.SetQueue(context, queueName, 10, 11, 12);

            InternalQueueManagementV1 result = await _management.GetQueue(context, queueName);
            result.Should().NotBeNull();
            result.QueueName.Should().Be(queueName);

            rows = await _management.GetQueueList(context);
            rows.Should().NotBeNull();
            rows.Count().Should().Be(1);

            await _management.SetQueue(context, queueName, 10, 11, 12);

            rows = await _management.GetQueueList(context);
            rows.Should().NotBeNull();
            rows.Count().Should().Be(1);

            await _management.DeleteQueue(context, queueName);

            rows = await _management.GetQueueList(context);
            rows.Should().NotBeNull();
            rows.Count().Should().Be(0);

            result = await _management.GetQueue(context, queueName);
            result.Should().BeNull();
        }

        [Fact]
        public async Task DisableQueueSuccessTest()
        {
            InternalQueueManagementV1 result;
            IEnumerable<InternalQueueManagementV1> list;
            var context = _workContext.WithTag(_tag);

            list = await _management.GetQueueList(context, false);
            list.Should().NotBeNull();
            list.Count().Should().Be(0);

            list = await _management.GetQueueList(context, true);
            list.Should().NotBeNull();
            list.Count().Should().Be(0);

            const string queueName = "test-queue";
            await _management.SetQueue(context, queueName, 10, 11, 12);

            list = await _management.GetQueueList(context, false);
            list.Should().NotBeNull();
            list.Count().Should().Be(1);

            result = await _management.GetQueue(context, queueName);
            result.Should().NotBeNull();
            result.QueueName.Should().Be(queueName);

            await _management.DisableQueue(context, queueName);

            result = await _management.GetQueue(context, queueName);
            result.Should().BeNull();

            list = await _management.GetQueueList(context, true);
            list.Should().NotBeNull();
            list.Count().Should().Be(1);
            result = list.First();
            result.QueueName.Should().Be(queueName);
            result.Disable.Should().BeTrue();

            await _management.EnableQueue(context, queueName);

            result = await _management.GetQueue(context, queueName);
            result.Should().NotBeNull();
            result.QueueName.Should().Be(queueName);

            list = await _management.GetQueueList(context, false);
            list.Should().NotBeNull();
            list.Count().Should().Be(1);
        }

        [Fact]
        public async Task MultipleQueueSuccessTest()
        {
            var context = _workContext.WithTag(_tag);

            IEnumerable<string> queueNames = new string[] { "test-queue1", "test-queue2", "test-queue3" };

            foreach (var item in queueNames)
            {
                await _management.SetQueue(context, item, 20, 21, 22);
            }

            IEnumerable<InternalQueueManagementV1> rows = await _management.GetQueueList(context);
            rows.Should().NotBeNull();
            rows.Count().Should().Be(queueNames.Count());

            rows
                .OrderBy(x => x.QueueName)
                .Zip(queueNames.OrderBy(x => x), (f, s) => new Tuple<string, string>(f.QueueName, s))
                .All(x => x.Item1 == x.Item2)
                .Should().BeTrue();

            foreach (var item in queueNames)
            {
                await _management.DeleteQueue(context, item);
            }

            rows = await _management.GetQueueList(context);
            rows.Should().NotBeNull();
            rows.Count().Should().Be(0);
        }

        [Fact]
        public async Task MultipleDisableQueueSuccessTest()
        {
            var context = _workContext.WithTag(_tag);

            IEnumerable<string> queueNames = new string[] { "test-queue1", "test-queue2", "test-queue3" };

            foreach (var item in queueNames)
            {
                await _management.SetQueue(context, item, 20, 21, 22);
            }

            IEnumerable<InternalQueueManagementV1> rows = await _management.GetQueueList(context);
            rows.Should().NotBeNull();
            rows.Count().Should().Be(queueNames.Count());

            rows
                .OrderBy(x => x.QueueName)
                .Zip(queueNames.OrderBy(x => x), (f, s) => new Tuple<string, string>(f.QueueName, s))
                .All(x => x.Item1 == x.Item2)
                .Should().BeTrue();

            foreach (var item in queueNames)
            {
                await _management.DisableQueue(context, item);
            }

            rows = await _management.GetQueueList(context);
            rows.Should().NotBeNull();
            rows.Count().Should().Be(0);

            rows = await _management.GetQueueList(context, disable: true);
            rows.Should().NotBeNull();
            rows.Count().Should().Be(3);

            foreach (var item in queueNames)
            {
                await _management.EnableQueue(context, item);
            }

            rows = await _management.GetQueueList(context);
            rows.Should().NotBeNull();
            rows.Count().Should().Be(queueNames.Count());

            rows
                .OrderBy(x => x.QueueName)
                .Zip(queueNames.OrderBy(x => x), (f, s) => new Tuple<string, string>(f.QueueName, s))
                .All(x => x.Item1 == x.Item2)
                .Should().BeTrue();
        }

        [Fact]
        public async Task GetQueueStatusTest()
        {
            var context = _workContext.WithTag(_tag);

            const string queueName = "test-queue";
            await _management.SetQueue(context, queueName, 10, 11, 12);

            IEnumerable<InternalQueueStatusV1> result = await _management.GetQueueStatus(context);
            result.Should().NotBeNull();
            result.Count().Should().Be(1);
            result.First().QueueName.Should().Be(queueName);

            const string queueName2 = "test-queue-2";
            await _management.SetQueue(context, queueName2, 10, 11, 12);

            result = await _management.GetQueueStatus(context);
            result.Should().NotBeNull();
            result.Count().Should().Be(2);
            result.Count(x => x.QueueName == queueName || x.QueueName == queueName2).Should().Be(2);
        }
    }
}

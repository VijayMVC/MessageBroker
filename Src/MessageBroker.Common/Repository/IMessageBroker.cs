// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Toolbox;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public interface IMessageBroker
    {
        Task<long> EnqueueMessage(IWorkContext context, InternalEnqueueMessageV1 message);

        Task<InternalMessageV1> DequeueMessageAndDelete(IWorkContext context, string queueName);

        Task<InternalMessageV1> DequeueMessageWithLock(IWorkContext context, string queueName, int agentId);

        Task SettleMessage(IWorkContext context, long messageId, int agentId, SettleType settleType, string errorMessage = null);

        Task<IEnumerable<InternalScheduleV1>> GetMessageSchedules(IWorkContext context, string queueName);

        Task DeleteMessageSchedule(IWorkContext context, long scheduleId);

        Task<int> GetAgentId(IWorkContext context, string agentName);

        Task<IEnumerable<InternalMessageV1>> ListActiveMessages(IWorkContext context, string queueName);
    }
}

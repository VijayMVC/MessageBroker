// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Toolbox;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public interface IMessageManager
    {
        Task<long> EnqueueMessage(IWorkContext context, InternalEnqueueMessageV1 message);

        Task<InternalMessageV1> DequeueMessageAndDelete(IWorkContext context, string queueName, TimeSpan? waitFor = null);

        Task<InternalMessageV1> DequeueMessageWithLock(IWorkContext context, string queueName, int agentId, TimeSpan? waitFor = null);

        Task SettleMessage(IWorkContext context, long messageId, int agentId, SettleType settleType, string errorMessage = null);

        Task<IEnumerable<InternalScheduleV1>> GetMessageSchedules(IWorkContext context, string queueName);

        Task DeleteMessageSchedule(IWorkContext context, long scheduleId);

        Task<int> GetAgentId(IWorkContext context, string agentName);

        Task<IEnumerable<InternalMessageV1>> ListActiveMessage(IWorkContext context, string queueName);
    }
}

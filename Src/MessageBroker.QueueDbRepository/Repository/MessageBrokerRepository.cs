// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Sql;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace MessageBroker.QueueDbRepository
{
    public class MessageBrokerRepository : IMessageBroker
    {
        private readonly IMessageBrokerConfiguration _configuration;

        public MessageBrokerRepository(IMessageBrokerConfiguration configuration)
        {
            Verify.IsNotNull(nameof(configuration), configuration);

            _configuration = configuration;
        }

        public Task DeleteMessageSchedule(IWorkContext context, long scheduleId)
        {
            return new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[Delete-Schedule]")
                .AddParameter(nameof(scheduleId), scheduleId)
                .ExecuteNonQuery(context);
        }

        public async Task<InternalMessageV1> DequeueMessageAndDelete(IWorkContext context, string queueName)
        {
            try
            {
                IEnumerable<MessageRow> rows = await new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[Dequeue-Message]")
                .AddParameter(nameof(queueName), queueName)
                .Execute(context, MessageRow.Read);

                return rows.Select(x => x.ConvertTo()).FirstOrDefault();
            }
            catch (SqlException ex)
            {
                throw new QueueNotFound(nameof(DequeueMessageWithLock), context, ex);
            }
        }

        public async Task<InternalMessageV1> DequeueMessageWithLock(IWorkContext context, string queueName, int agentId)
        {
            try
            {
                IEnumerable<MessageRow> rows = await new SqlExec(_configuration.SqlConfiguration)
                    .SetCommand("[App].[Dequeue-MessageWithLock]")
                    .AddParameter(nameof(queueName), queueName)
                    .AddParameter(nameof(agentId), agentId)
                    .Execute(context, MessageRow.Read);

                return rows.Select(x => x.ConvertTo()).FirstOrDefault();
            }
            catch (SqlException ex)
            {
                throw new QueueNotFound(nameof(DequeueMessageWithLock), context, ex);
            }
        }

        public async Task<long> EnqueueMessage(IWorkContext context, InternalEnqueueMessageV1 message)
        {
            Verify.IsNotNull(nameof(message), message);

            IEnumerable<ReturnIdRow> rows;

            try
            {
                if (message.ScheduleDate != null)
                {
                    rows = await new SqlExec(_configuration.SqlConfiguration)
                        .SetCommand("[App].[Create-Schedule]")
                        .AddParameter(nameof(message.QueueName), message.QueueName)
                        .AddParameter(nameof(message.AgentId), message.AgentId)
                        .AddParameter(nameof(message.ClientMessageId), message.ClientMessageId)
                        .AddParameter(nameof(message.Cv), message.Cv)
                        .AddParameter(nameof(message.Payload), message.Payload)
                        .AddParameter(nameof(message.ScheduleDate), (DateTime)message.ScheduleDate)
                        .Execute(context, ReturnIdRow.Read);

                    return rows.First().Id;
                }

                rows = await new SqlExec(_configuration.SqlConfiguration)
                    .SetCommand("[App].[Enqueue-Message]")
                    .AddParameter(nameof(message.QueueName), message.QueueName)
                    .AddParameter(nameof(message.AgentId), message.AgentId)
                    .AddParameter(nameof(message.ClientMessageId), message.ClientMessageId)
                    .AddParameter(nameof(message.Cv), message.Cv)
                    .AddParameter(nameof(message.Payload), message.Payload)
                    .Execute(context, ReturnIdRow.Read);

                return rows.First().Id;
            }
            catch (SqlException ex)
            {
                throw new QueueFullException(message.ScheduleDate == null ? "Enqueue" : "Schedule", context, ex);
            }
        }

        public async Task<IEnumerable<InternalScheduleV1>> GetMessageSchedules(IWorkContext context, string queueName)
        {
            IEnumerable<ScheduleRow> rows = await new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[List-Schedules]")
                .AddParameter(nameof(queueName), queueName)
                .Execute(context, ScheduleRow.Read);

            return rows.Select(x => x.ConvertTo());
        }

        public async Task<int> GetAgentId(IWorkContext context, string agentName)
        {
            Verify.IsNotEmpty(nameof(agentName), agentName);

            IEnumerable<AgentRow> rows = await new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[Set-Agent]")
                .AddParameter(nameof(agentName), agentName)
                .Execute(context, AgentRow.Read);

            return rows.Select(x => x.ConvertTo())
                .Select(x => x.AgentId)
                .First();
        }

        public Task SettleMessage(IWorkContext context, long messageId, int agentId, SettleType settleType, string errorMessage = null)
        {
            return new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[Settle-Message]")
                .AddParameter(nameof(messageId), messageId)
                .AddParameter(nameof(agentId), agentId)
                .AddParameter(nameof(settleType), settleType.ToString())
                .AddParameter(nameof(errorMessage), errorMessage)
                .ExecuteNonQuery(context);
        }

        public async Task<IEnumerable<InternalMessageV1>> ListActiveMessages(IWorkContext context, string queueName)
        {
            IEnumerable<MessageRow> rows = await new SqlExec(_configuration.SqlConfiguration)
                .SetCommand("[App].[List-ActiveMessages]")
                .AddParameter(nameof(queueName), queueName)
                .Execute(context, MessageRow.Read);

            return rows.Select(x => x.ConvertTo());
        }
    }
}

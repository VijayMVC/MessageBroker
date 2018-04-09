using MessageBroker.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.QueueDbRepository
{
    public static class RowExtensions
    {
        public static InternalAgentV1 ConvertTo(this AgentRow self)
        {
            return new InternalAgentV1
            {
                AgentId = self.AgentId,
                AgentName = self.AgentName,
                _createdDate = self._createdDate,
            };
        }

        public static AgentRow ConvertTo(this InternalAgentV1 self)
        {
            return new AgentRow
            {
                AgentId = self.AgentId,
                AgentName = self.AgentName,
                _createdDate = self._createdDate,
            };
        }

        public static InternalMessageV1 ConvertTo(this MessageRow self)
        {
            return new InternalMessageV1
            {
                MessageId = self.MessageId,
                QueueId = self.QueueId,
                ClientMessageId = self.ClientMessageId,
                Cv = self.Cv,
                Payload = self.Payload,
                LockedDate = self.LockedDate,
                LockedByAgentId = self.LockedByAgentId,
                RetryCount = self.RetryCount,
                CreatedByAgentId = self.CreatedByAgentId,
                _createdDate = self._createdDate,
            };
        }

        public static MessageRow ConvertTo(this InternalMessageV1 self)
        {
            return new MessageRow
            {
                MessageId = self.MessageId,
                QueueId = self.QueueId,
                ClientMessageId = self.ClientMessageId,
                Cv = self.Cv,
                Payload = self.Payload,
                LockedDate = self.LockedDate,
                LockedByAgentId = self.LockedByAgentId,
                RetryCount = self.RetryCount,
                CreatedByAgentId = self.CreatedByAgentId,
                _createdDate = self._createdDate,
            };
        }

        public static InternalQueueManagementV1 ConvertTo(this QueueManagementRow self)
        {
            return new InternalQueueManagementV1
            {
                QueueId = self.QueueId,
                QueueName = self.QueueName,
                CurrentSizeLimit = self.CurrentSizeLimit,
                CurrentRetryLimit = self.CurrentRetryLimit,
                LockValidForSec = self.LockValidForSec,
                Disable = self.Disable,
                QueueLength = self.QueueLength,
                ScheduleQueueLength = self.ScheduleQueueLength,
            };
        }

        public static QueueManagementRow ConvertTo(this InternalQueueManagementV1 self)
        {
            return new QueueManagementRow
            {
                QueueId = self.QueueId,
                QueueName = self.QueueName,
                CurrentSizeLimit = self.CurrentSizeLimit,
                CurrentRetryLimit = self.CurrentRetryLimit,
                LockValidForSec = self.LockValidForSec,
                Disable = self.Disable,
                QueueLength = self.QueueLength,
                ScheduleQueueLength = self.ScheduleQueueLength,
            };
        }

        public static InternalScheduleV1 ConvertTo(this ScheduleRow self)
        {
            return new InternalScheduleV1
            {
                ScheduleId = self.ScheduleId,
                QueueId = self.QueueId,
                ClientMessageId = self.ClientMessageId,
                Cv = self.Cv,
                Payload = self.Payload,
                ScheduleDate = self.ScheduleDate,
                CreatedByAgentId = self.CreatedByAgentId,
                _createdDate = self._createdDate,
            };
        }

        public static ScheduleRow ConvertTo(this InternalScheduleV1 self)
        {
            return new ScheduleRow
            {
                ScheduleId = self.ScheduleId,
                QueueId = self.QueueId,
                ClientMessageId = self.ClientMessageId,
                Cv = self.Cv,
                Payload = self.Payload,
                ScheduleDate = self.ScheduleDate,
                CreatedByAgentId = self.CreatedByAgentId,
                _createdDate = self._createdDate,
            };
        }

        public static InternalHistoryV1 ConvertTo(this HistoryRow self)
        {
            return new InternalHistoryV1
            {
                HistoryId = self.HistoryId,
                MessageId = self.MessageId,
                ActivityType = self.ActivityType,
                QueueName = self.QueueName,
                Cv = self.Cv,
                ClientMessageId = self.ClientMessageId,
                Payload = self.Payload,
                SettleByAgent = self.SettleByAgent,
                ErrorMesage = self.ErrorMessage,
                RetryCount = self.RetryCount,
                _createdDate = self._createdDate,
            };
        }

        public static HistoryRow ConvertTo(this InternalHistoryV1 self)
        {
            return new HistoryRow
            {
                HistoryId = self.HistoryId,
                MessageId = self.MessageId,
                ActivityType = self.ActivityType,
                QueueName = self.QueueName,
                Cv = self.Cv,
                ClientMessageId = self.ClientMessageId,
                Payload = self.Payload,
                SettleByAgent = self.SettleByAgent,
                ErrorMessage = self.ErrorMesage,
                RetryCount = self.RetryCount,
                _createdDate = self._createdDate,
            };
        }

        public static InternalQueueStatusV1 ConvertTo(this QueueStatusRow self)
        {
            return new InternalQueueStatusV1
            {
                QueueName = self.QueueName,
                QueueCount = self.QueueCount,
                CurrentSizeLimit = self.CurrentSizeLimit,
                QueueSize = self.QueueSize,
            };
        }
    }
}

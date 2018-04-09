using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public static class ConvertExtensions
    {
        public static QueueDetailContractV1 ConvertTo(this InternalQueueManagementV1 self)
        {
            if (self == null)
            {
                return null;
            }

            return new QueueDetailContractV1
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

        public static AgentContractV1 ConvertTo(this InternalAgentV1 self)
        {
            if (self == null)
            {
                return null;
            }

            return new AgentContractV1
            {
                AgentId = self.AgentId,
                AgentName = self.AgentName,
                _createdDate = self._createdDate,
            };
        }

        public static HistoryContractV1 ConvertTo(this InternalHistoryV1 self)
        {
            if (self == null)
            {
                return null;
            }

            return new HistoryContractV1
            {
                HistoryId = self.HistoryId,
                MessageId = self.MessageId,
                ActivityType = self.ActivityType,
                QueueName = self.QueueName,
                Cv = self.Cv,
                ClientMessageId = self.ClientMessageId,
                Payload = self.Payload,
                SettleByAgent = self.SettleByAgent,
                ErrorMesage = self.ErrorMesage,
                RetryCount = self.RetryCount,
                _createdDate = self._createdDate,

            };
        }

        public static InternalEnqueueMessageV1 ConvertTo(this EnqueueMessageContractV1 self)
        {
            if (self == null)
            {
                return null;
            }

            return new InternalEnqueueMessageV1
            {
                QueueName = self.QueueName,
                AgentId = self.AgentId,
                ClientMessageId = self.ClientMessageId,
                Cv = self.Cv,
                Payload = self.Payload,
                ScheduleDate = self.ScheduleDate,
            };
        }

        public static MessageContractV1 ConvertTo(this InternalMessageV1 self)
        {
            if (self == null)
            {
                return null;
            }

            return new MessageContractV1
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

        public static ScheduleContractV1 ConvertTo(this InternalScheduleV1 self)
        {
            if (self == null)
            {
                return null;
            }

            return new ScheduleContractV1
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

        public static QueueStatusContractV1 ConvertTo(this InternalQueueStatusV1 self)
        {
            if (self == null)
            {
                return null;
            }

            return new QueueStatusContractV1
            {
                QueueName = self.QueueName,
                QueueCount = self.QueueCount,
                CurrentSizeLimit = self.CurrentSizeLimit,
                QueueSize = self.QueueSize,
            };
        }
    }
}

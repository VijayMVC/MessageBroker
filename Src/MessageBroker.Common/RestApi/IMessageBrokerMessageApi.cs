using Khooversoft.Net;
using Khooversoft.Toolbox;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    public interface IMessageBrokerMessageApi
    {
        Task<RestResponse<EnqueuedContractV1>> EnqueueMessage(IWorkContext context, EnqueueMessageContractV1 contract);

        Task<RestResponse<MessageContractV1>> DequeueMessageAndDelete(IWorkContext context, string queueName, TimeSpan? waitFor = null, CancellationToken? token = null);

        Task<RestResponse<MessageContractV1>> DequeueMessageWithLock(IWorkContext context, string queueName, AgentContractV1 agentContract, TimeSpan? waitFor = null, CancellationToken? token = null);

        Task<RestResponse> SettleMessage(IWorkContext context, SettleMessageContractV1 contract);

        Task<RestResponse<RestPageResultV1<MessageContractV1>>> GetActiveMessages(IWorkContext context, string queueName);

        Task<RestResponse<RestPageResultV1<ScheduleContractV1>>> GetMessageSchedules(IWorkContext context, string queueName);

        Task<RestResponse> DeleteMessageSchedule(IWorkContext context, string queueName, long scheduleId);

        Task<RestResponse<AgentContractV1>> GetAgentId(IWorkContext context, string queueName, string agentName);
    }
}

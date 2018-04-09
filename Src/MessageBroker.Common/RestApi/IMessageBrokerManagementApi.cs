using Khooversoft.Net;
using Khooversoft.Toolbox;
using System.Net;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    public interface IMessageBrokerManagementApi
    {
        Task<RestResponse> SetQueue(IWorkContext context, SetQueueContractV1 contract);

        Task<RestResponse> ClearQueue(IWorkContext context, string queueName, bool copyToHistory = false);

        Task<RestResponse> DeleteQueue(IWorkContext context, string queueName);

        Task<RestResponse> DisableQueue(IWorkContext context, string queueName);

        Task<RestResponse> EnableQueue(IWorkContext context, string queueName);

        Task<RestResponse<QueueDetailContractV1>> GetQueue(IWorkContext context, string queueName, HttpStatusCode[] acceptedCodes = null);

        Task<RestResponse<RestPageResultV1<QueueDetailContractV1>>> GetQueueList(IWorkContext context, bool disable = false);

        Task<RestResponse<RestPageResultV1<QueueStatusContractV1>>> GetQueueStatus(IWorkContext context);
    }
}

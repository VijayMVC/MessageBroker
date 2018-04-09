using Khooversoft.Net;
using Khooversoft.Toolbox;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    public interface IMessageBrokerHistoryApi
    {
        Task<RestResponse<HistoryContractV1>> GetHistory(IWorkContext context, long messageId);
    }
}

using Khooversoft.Net;
using Khooversoft.Toolbox;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    public interface IMessageBrokerAdministrationApi
    {
        Task<RestResponse> ResetDatabase(IWorkContext context);

        Task<RestResponse> SetHistorySizeConfiguration(IWorkContext context, int size);

        Task<RestResponse<HistoryDetailContractV1>> GetHistorySizeConfiguration(IWorkContext context);
    }
}

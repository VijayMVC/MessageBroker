using Khooversoft.Net;
using Khooversoft.Toolbox;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    public interface IMessageBrokerAgentApi
    {
        Task<RestResponse<RestPageResultV1<AgentContractV1>>> GetAgents(IWorkContext context);
    }
}

using Khooversoft.Toolbox;
using System.Threading.Tasks;

namespace MessageBroker.Common
{
    public interface IMessageBrokerAdministration
    {
        Task ResetDatabase(IWorkContext context);

        Task SetHistorySizeConfiguration(IWorkContext context, int queueSize);

        Task<int?> GetHistorySizeConfiguration(IWorkContext context);
    }
}

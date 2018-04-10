using Khooversoft.Net;
using Khooversoft.Toolbox;
using System;
using System.Threading.Tasks;

namespace MessageBroker.Common.RestApi
{
    public class MessageBrokerAdministrationApi : ClientBase, IMessageBrokerAdministrationApi
    {
        private static readonly Tag _tag = new Tag(nameof(MessageBrokerAdministrationApi));

        public MessageBrokerAdministrationApi(Uri baseUri, IRestClientConfiguration restClientConfiguration)
            : base(baseUri, restClientConfiguration)
        {
        }

        /// <summary>
        /// Used for testing, clear database
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>OK</returns>
        public async Task<RestResponse> ResetDatabase(IWorkContext context)
        {
            Verify.IsNotNull(nameof(context), context);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath("clear-all")
                .PostAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context);
        }

        /// <summary>
        /// Set history ring buffer size
        /// </summary>
        /// <param name="context">context</param>
        /// <param name="size">new size</param>
        /// <returns>response</returns>
        public async Task<RestResponse> SetHistorySizeConfiguration(IWorkContext context, int size)
        {
            Verify.IsNotNull(nameof(context), context);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath("history")
                .AddQuery(nameof(size), size.ToString())
                .PutAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context);
        }

        /// <summary>
        /// Get history size configuration
        /// </summary>
        /// <param name="context">context</param>
        /// <returns>history details</returns>
        public async Task<RestResponse<HistoryDetailContractV1>> GetHistorySizeConfiguration(IWorkContext context)
        {
            Verify.IsNotNull(nameof(context), context);
            context = context.WithTag(_tag);

            return await CreateClient()
                .AddPath("history")
                .GetAsync(context)
                .ToRestResponseAsync(context)
                .EnsureSuccessStatusCodeAsync(context)
                .GetContentAsync<HistoryDetailContractV1>(context);
        }

        protected override RestClient CreateClient()
        {
            return base.CreateClient()
                .AddPath("v1/administration");
        }
    }
}

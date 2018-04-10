using Khooversoft.AspMvc;
using Khooversoft.Net;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MessageBrokerApi.Controllers.V1
{
    /// <summary>
    /// Controller for history
    /// </summary>
    [Route("V1/[controller]")]
    public class HistoryController : Controller
    {
        private readonly Tag _tag = new Tag(nameof(ManagementController));

        private readonly IMessageBrokerManagement _management;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageBrokerManagement">management service</param>
        public HistoryController(IMessageBrokerManagement messageBrokerManagement)
        {
            _management = messageBrokerManagement;
        }

        /// <summary>
        /// Get history detail for message
        /// </summary>
        /// <param name="messageId">message id</param>
        /// <returns>History details</returns>
        [Produces(typeof(HistoryContractV1))]
        [HttpGet("{messageId}")]
        public async Task<IActionResult> GetHistory(long messageId)
        {
            RequestContext requestContext = HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            InternalHistoryV1 result = await _management.GetHistory(context, messageId);

            return new StandardActionResult(context)
                .SetContent(result.ConvertTo());
        }
    }
}

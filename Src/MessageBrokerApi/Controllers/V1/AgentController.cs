using Khooversoft.AspMvc;
using Khooversoft.Net;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageBrokerApi.Controllers.V1
{
    /// <summary>
    /// Controller for managing Agents
    /// </summary>
    [Route("V1/[controller]")]
    public class AgentController : Controller
    {
        private readonly Tag _tag = new Tag(nameof(ManagementController));

        private readonly IMessageBrokerManagement _management;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageBrokerManagement">agent service</param>
        public AgentController(IMessageBrokerManagement messageBrokerManagement)
        {
            _management = messageBrokerManagement;
        }

        /// <summary>
        /// Get a list of all agents on the system
        /// </summary>
        /// <returns>Rest page result for list of Agents</returns>
        [Produces(typeof(RestPageResultV1<AgentContractV1>))]
        [HttpGet("")]
        public async Task<IActionResult> GetAgents()
        {
            RequestContext requestContext = HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            IEnumerable<InternalAgentV1> result = await _management.GetAgents(context);

            var pageResult = new RestPageResultV1<AgentContractV1>
            {
                Items = new List<AgentContractV1>(result.Select(x => x.ConvertTo())),
            };

            return new StandardActionResult(context)
                .SetContent(pageResult);
        }
    }
}


using Khooversoft.AspMvc;
using Khooversoft.Net;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MessageBrokerApi.Controllers.V1
{
    /// <summary>
    /// Management for queues
    /// </summary>
    [Route("V1/[controller]")]
    public class ManagementController : Controller
    {
        private readonly Tag _tag = new Tag(nameof(ManagementController));

        private readonly IMessageBrokerManagement _management;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageBrokerManagement">message broker management services</param>
        public ManagementController(IMessageBrokerManagement messageBrokerManagement)
        {
            _management = messageBrokerManagement;
        }

        /// <summary>
        /// Ping the health of the system
        /// </summary>
        /// <returns>OK</returns>
        [Produces(typeof(string))]
        [HttpGet("health-check")]
        public async Task<IActionResult> HealthCheck()
        {
            RequestContext requestContext = this.HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            bool repositoryHealthCheck = await _management.HealthCheck(context);

            return new StandardActionResult(context)
                .SetContent(new HealthCheckContractV1 { Status = "ok", RepositoryHealthCheck = repositoryHealthCheck ? "ok" : "failed" });
        }

        /// <summary>
        /// Creates or updates a queue
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <param name="contract">contract</param>
        /// <returns>result</returns>
        [HttpPut("{queueName}")]
        public async Task<IActionResult> SetQueue(string queueName, [FromBody] SetQueueContractV1 contract)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);
            Verify.IsNotNull(nameof(contract), contract);

            RequestContext requestContext = HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            contract.SetDefaults();
            await _management.SetQueue(context, queueName, (int)contract.CurrentSizeLimit, (int)contract.CurrentRetryLimit, (int)contract.LockValidForSec);

            return new StandardActionResult(context);
        }

        /// <summary>
        /// Clear a queue
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <param name="copyToHistory">copy to history</param>
        /// <returns>result</returns>
        [HttpPost("{queueName}/clear-queue")]
        public async Task<IActionResult> ClearQueue(string queueName, [FromQuery] bool copyToHistory = false)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            RequestContext requestContext = this.HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            await _management.ClearQueue(context, queueName, copyToHistory);

            return new StandardActionResult(context);
        }

        /// <summary>
        /// Delete a queue
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <returns>result</returns>
        [HttpDelete("{queueName}")]
        public async Task<IActionResult> DeleteQueue(string queueName)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            RequestContext requestContext = this.HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            await _management.DeleteQueue(context, queueName);

            return new StandardActionResult(context);
        }

        /// <summary>
        /// Delete a queue
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <returns>result</returns>
        [HttpPost("{queueName}/disable-queue")]
        public async Task<IActionResult> DisableQueue(string queueName)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            RequestContext requestContext = this.HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            await _management.DisableQueue(context, queueName);

            return new StandardActionResult(context);
        }
        /// <summary>
        /// Enable a queue
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <returns>result</returns>
        [HttpPost("{queueName}/enable-queue")]
        public async Task<IActionResult> EnableQueue(string queueName)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            RequestContext requestContext = this.HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            await _management.EnableQueue(context, queueName);

            return new StandardActionResult(context);
        }

        /// <summary>
        /// Get queue details
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <returns>result</returns>
        [Produces(typeof(QueueDetailContractV1))]
        [HttpGet("{queueName}")]
        public async Task<IActionResult> GetQueue(string queueName)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            RequestContext requestContext = this.HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            InternalQueueManagementV1 result = await _management.GetQueue(context, queueName);
            if (result == null)
            {
                return new StandardActionResult(context, HttpStatusCode.NotFound);
            }

            return new StandardActionResult(context)
                .SetContent(result.ConvertTo());
        }

        /// <summary>
        /// Get list of queues
        /// </summary>
        /// <param name="disable"></param>
        /// <returns>page result</returns>
        [Produces(typeof(RestPageResultV1<QueueDetailContractV1>))]
        [HttpGet("")]
        public async Task<IActionResult> GetQueueList(bool disable)
        {
            RequestContext requestContext = this.HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            IEnumerable<InternalQueueManagementV1> result = await _management.GetQueueList(context, disable);
            Verify.IsNotNull(nameof(result), result);

            var pageResult = new RestPageResultV1<QueueDetailContractV1>
            {
                Items = new List<QueueDetailContractV1>(result.Select(x => x.ConvertTo())),
            };

            return new StandardActionResult(context)
                .SetContent(pageResult);
        }

        /// <summary>
        /// Get a list of all queues status and their status
        /// </summary>
        /// <returns>page result</returns>
        [Produces(typeof(RestPageResultV1<QueueStatusContractV1>))]
        [HttpGet("status")]
        public async Task<IActionResult> GetQueueStatus()
        {
            RequestContext requestContext = this.HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            IEnumerable<InternalQueueStatusV1> result = await _management.GetQueueStatus(context);
            Verify.IsNotNull(nameof(result), result);

            var pageResult = new RestPageResultV1<QueueStatusContractV1>
            {
                Items = new List<QueueStatusContractV1>(result.Select(x => x.ConvertTo())),
            };

            return new StandardActionResult(context)
                .SetContent(pageResult);
        }
    }
}

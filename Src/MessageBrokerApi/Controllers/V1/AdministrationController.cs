// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.AspMvc;
using Khooversoft.Net;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace MessageBrokerApi.Controllers.V1
{
    /// <summary>
    /// Administration controller - used to manage queue storage
    /// </summary>
    [Route("V1/[controller]")]
    public class AdministrationController : Controller
    {
        private readonly Tag _tag = new Tag(nameof(ManagementController));

        private readonly IMessageBrokerAdministration _administration;

        /// <summary>
        /// Create new controller
        /// </summary>
        /// <param name="messageBrokerAdministration">administration service</param>
        public AdministrationController(IMessageBrokerAdministration messageBrokerAdministration)
        {
            _administration = messageBrokerAdministration;
        }

#if DEBUG
        /// <summary>
        /// Clear database
        /// </summary>
        /// <returns>OK</returns>
        [HttpPost("clear-all")]
        public async Task<IActionResult> ClearDatabase()
        {
            RequestContext requestContext = this.HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            // Clear out the database (used for testing)
            await _administration.ResetDatabase(context);

            return new StandardActionResult(context);
        }
#endif

        /// <summary>
        /// Set history table size
        /// </summary>
        /// <param name="size">size of history ring buffer</param>
        /// <returns>result</returns>
        [HttpPut("history")]
        public async Task<IActionResult> SetHistorySizeConfiguration(int size)
        {
            RequestContext requestContext = this.HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            await _administration.SetHistorySizeConfiguration(context, size);

            return new StandardActionResult(context);
        }

        /// <summary>
        /// Get the size of the history table
        /// </summary>
        /// <returns>response</returns>
        [Produces(typeof(HistoryDetailContractV1))]
        [HttpGet("history")]
        public async Task<IActionResult> GetHistorySizeConfiguration()
        {
            RequestContext requestContext = this.HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            int? size = await _administration.GetHistorySizeConfiguration(context);
            if (size == null)
            {
                return new StandardActionResult(context, HttpStatusCode.NotFound);
            }

            var response = new HistoryDetailContractV1
            {
                HistorySize = (int)size,
            };

            return new StandardActionResult(context)
                .SetContent(response);
        }
    }
}

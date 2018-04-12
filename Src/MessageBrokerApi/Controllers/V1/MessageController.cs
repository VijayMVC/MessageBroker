// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.AspMvc;
using Khooversoft.Net;
using Khooversoft.Toolbox;
using MessageBroker.Common;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MessageBrokerApi.Controllers.V1
{
    /// <summary>
    /// Controller for messages
    /// </summary>
    [Route("V1/[controller]")]
    public class MessageController : Controller
    {
        private readonly Tag _tag = new Tag(nameof(ManagementController));

        private readonly IMessageManager _message;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageBroker">message service</param>
        public MessageController(IMessageManager messageBroker)
        {
            _message = messageBroker;
        }

        /// <summary>
        /// Enqueue a message into a queue
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <param name="contract">message contract</param>
        /// <returns>response (message id)</returns>
        [Produces(typeof(EnqueuedContractV1))]
        [HttpPost("{queueName}/message/head")]
        public async Task<IActionResult> EnqueueMessage(string queueName, [FromBody] EnqueueMessageContractV1 contract)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);
            Verify.IsNotNull(nameof(contract), contract);

            RequestContext requestContext = HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            long messageId;
            try
            {
                messageId = await _message.EnqueueMessage(context, contract.ConvertTo());
            }
            catch (QueueFullException ex)
            {
                return new StandardActionResult(context, HttpStatusCode.Conflict)
                    .SetContent($"Queue is full {ex.Message}");
            }

            var response = new EnqueuedContractV1
            {
                MessageId = messageId,
            };

            return new StandardActionResult(context)
                .SetContent(response);
        }

        /// <summary>
        /// Get a message from the queue and delete it (move to history)
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <param name="waitMs">Number of ms to wait for a message(0 or null = no wait)</param>
        /// <returns>message</returns>
        [Produces(typeof(MessageContractV1))]
        [HttpDelete("{queueName}/message/head")]
        public async Task<IActionResult> DequeueMessageAndDelete(string queueName, [FromQuery] int? waitMs)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            RequestContext requestContext = HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            TimeSpan? waitFor = (waitMs ?? 0) != 0 ? waitFor = TimeSpan.FromMilliseconds((int)waitMs) : null;

            try
            {
                InternalMessageV1 result = await _message.DequeueMessageAndDelete(context, queueName, waitFor);
                if (result == null)
                {
                    return new StandardActionResult(context, HttpStatusCode.NoContent);
                }

                return new StandardActionResult(context)
                    .SetContent(result.ConvertTo());
            }
            catch (QueueNotFound ex)
            {
                return new StandardActionResult(context, HttpStatusCode.NotFound)
                    .SetContent($"Queue not found {ex.Message}");
            }
        }


        /// <summary>
        /// Get a message from the queue and lock it for the agent
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <param name="agentId">agent id</param>
        /// <param name="waitMs">Number of ms to wait for a message(0 or null = no wait)</param>
        /// <returns>message</returns>
        [Produces(typeof(MessageContractV1))]
        [HttpGet("{queueName}/message/head")]
        public async Task<IActionResult> DequeueMessageWithLock(string queueName, [FromQuery] int agentId, [FromQuery] int? waitMs)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            RequestContext requestContext = HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            TimeSpan? waitFor = (waitMs ?? 0) != 0 ? waitFor = TimeSpan.FromMilliseconds((int)waitMs) : null;

            try
            {
                InternalMessageV1 result = await _message.DequeueMessageWithLock(context, queueName, agentId, waitFor);
                if (result == null)
                {
                    return new StandardActionResult(context, HttpStatusCode.NoContent);
                }

                return new StandardActionResult(context)
                    .SetContent(result.ConvertTo());
            }
            catch (QueueNotFound ex)
            {
                return new StandardActionResult(context, HttpStatusCode.NotFound)
                    .SetContent($"Queue not found {ex.Message}");
            }
        }

        /// <summary>
        /// Settle a message that was locked by an agent
        /// </summary>
        /// <param name="queueName">queueName</param>
        /// <param name="messageId">messageId</param>
        /// <param name="contract">contract</param>
        /// <returns>result</returns>
        [HttpPost("{queueName}/message/{messageId}")]
        public async Task<IActionResult> SettleMessage(string queueName, long messageId, [FromBody] SettleMessageContractV1 contract)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);
            Verify.IsNotNull(nameof(contract), contract);
            Verify.Assert(messageId == contract.MessageId, $"{nameof(contract.MessageId)} does not match resource URI");

            RequestContext requestContext = HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            await _message.SettleMessage(context, messageId, contract.AgentId, contract.SettleType, contract.ErrorMessage);

            return new StandardActionResult(context);
        }

        /// <summary>
        /// Get a list of active messages
        /// </summary>
        /// <param name="queueName">queueName</param>
        /// <returns>page results</returns>
        [Produces(typeof(RestPageResultV1<MessageContractV1>))]
        [HttpGet("{queueName}/message")]
        public async Task<IActionResult> GetActiveMessages(string queueName)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            RequestContext requestContext = HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            IEnumerable<InternalMessageV1> result = await _message.ListActiveMessage(context, queueName);

            var pageResult = new RestPageResultV1<MessageContractV1>
            {
                Items = new List<MessageContractV1>(result.Select(x => x.ConvertTo())),
            };

            return new StandardActionResult(context)
                .SetContent(pageResult);
        }

        /// <summary>
        /// Get a list of messages that have been scheduled for a queue.
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <returns>all scheduled messages</returns>
        [Produces(typeof(RestPageResultV1<ScheduleContractV1>))]
        [HttpGet("{queueName}/schedule")]
        public async Task<IActionResult> GetMessageSchedules(string queueName)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            RequestContext requestContext = HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            IEnumerable<InternalScheduleV1> result = await _message.GetMessageSchedules(context, queueName);
            if (result == null || result.Count() == 0)
            {
                return new StandardActionResult(context, HttpStatusCode.NoContent);
            }

            var pageResult = new RestPageResultV1<ScheduleContractV1>
            {
                Items = new List<ScheduleContractV1>(result.Select(x => x.ConvertTo())),
            };

            return new StandardActionResult(context)
                .SetContent(pageResult);
        }

        /// <summary>
        /// Delete a scheduled message from the schedule system.
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <param name="scheduleId">schedule id</param>
        /// <returns>http status</returns>
        [HttpDelete("{queueName}/schedule/{scheduleId}")]
        public async Task<IActionResult> DeleteMessageSchedule(string queueName, long scheduleId)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);

            RequestContext requestContext = HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            await _message.DeleteMessageSchedule(context, scheduleId);

            return new StandardActionResult(context);
        }

        /// <summary>
        /// Get an agent's id and details
        /// </summary>
        /// <param name="queueName">queue name</param>
        /// <param name="agentName">agent name</param>
        /// <returns>agent details</returns>
        [Produces(typeof(AgentContractV1))]
        [HttpGet("{queueName}/agent/{agentName}")]
        public async Task<IActionResult> GetAgentId(string queueName, string agentName)
        {
            Verify.IsNotEmpty(nameof(queueName), queueName);
            Verify.IsNotEmpty(nameof(agentName), agentName);

            RequestContext requestContext = HttpContext.GetRequestContext();
            var context = requestContext.Context.WithTag(_tag);

            int agentId = await _message.GetAgentId(context, agentName);

            var result = new AgentContractV1
            {
                AgentId = agentId,
                AgentName = agentName
            };

            return new StandardActionResult(context)
                .SetContent(result);
        }
    }
}

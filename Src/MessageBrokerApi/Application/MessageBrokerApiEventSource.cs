﻿using Khooversoft.AspMvc;
using Khooversoft.Net;
using Khooversoft.Toolbox;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;

namespace MessageBrokerApi
{
    [EventSource(Name = "Khooversoft.MessageBroker.Api")]
    internal class MessageBrokerApiEventSource : EventSource, IWebEventLog
    {
        public static MessageBrokerApiEventSource Log { get; } = new MessageBrokerApiEventSource();

        private enum EventIds
        {
            ActivityStart = 1,
            ActivityStop,
            Info,
            Error,
            Warning,
            Verbose,
            HttpRequestStart,
            HttpRequestStop,
            HttpError
        };

        private readonly string _machineName;
        private readonly string _processName;

        public MessageBrokerApiEventSource()
            : base(EventSourceSettings.ThrowOnEventWriteErrors)
        {
            _machineName = Environment.MachineName;
            _processName = Process.GetCurrentProcess().ProcessName;
        }

        public class Keywords
        {
            public const EventKeywords Diagnostic = (EventKeywords)0x1;
            public const EventKeywords Perf = (EventKeywords)0x2;
        }

        public class Tasks
        {
            public const EventTask DBQuery = (EventTask)1;
        }

        // ========================================================================================
        // ========================================================================================

        [NonEvent]
        public void ActivityStart(IWorkContext context, string message = null)
        {
            Verify.IsNotNull(nameof(context), context);

            ActivityStart(_machineName, _processName, context.Cv, context.Tag, message);
        }

        [Event((int)EventIds.ActivityStart, Level = EventLevel.Verbose, Keywords = Keywords.Perf)]
        private void ActivityStart(string machineName, string processName, string cv, string tag, string message)
        {
            if (IsEnabled())
            {
                WriteEvent((int)EventIds.ActivityStart, machineName, processName, cv, tag, message);
            }
        }

        // ========================================================================================
        // ========================================================================================

        [NonEvent]
        public void ActivityStop(IWorkContext context, string message = null, long durationMs = 0)
        {
            Verify.IsNotNull(nameof(context), context);

            ActivityStop(_machineName, _processName, context.Cv, context.Tag, message, durationMs);
        }

        [Event((int)EventIds.ActivityStop, Level = EventLevel.Verbose, Keywords = Keywords.Perf)]
        private void ActivityStop(string machineName, string processName, string cv, string tag, string message, long durationMs)
        {
            if (IsEnabled())
            {
                WriteEvent((int)EventIds.ActivityStop, machineName, processName, cv, tag, message, durationMs);
            }
        }

        // ========================================================================================
        // ========================================================================================

        [NonEvent]
        public void Info(IWorkContext context, string message)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotEmpty(nameof(message), message);

            Info(_machineName, _processName, context.Cv, context.Tag, message);
        }

        [Event((int)EventIds.Info, Level = EventLevel.Informational, Keywords = Keywords.Diagnostic)]
        private void Info(string machineName, string processName, string cv, string tag, string message)
        {
            if (IsEnabled())
            {
                WriteEvent((int)EventIds.Info, machineName, processName, cv, tag, message);
            }
        }

        // ========================================================================================
        // ========================================================================================

        [NonEvent]
        public void Error(IWorkContext context, string message, Exception exception = null)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotEmpty(nameof(message), message);

            Error(_machineName, _processName, context.Cv, context.Tag, message, exception?.ToString());
        }

        [Event((int)EventIds.Error, Level = EventLevel.Error, Keywords = Keywords.Diagnostic)]
        private void Error(string machineName, string processName, string cv, string tag, string message, string exception)
        {
            if (IsEnabled())
            {
                WriteEvent((int)EventIds.Error, machineName, processName, cv, tag, message, exception);
            }
        }

        // ========================================================================================
        // ========================================================================================

        [NonEvent]
        public void Warning(IWorkContext context, string message, Exception exception = null)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotEmpty(nameof(message), message);

            Warning(_machineName, _processName, context.Cv, context.Tag, message, exception?.ToString());
        }

        [Event((int)EventIds.Warning, Level = EventLevel.Warning, Keywords = Keywords.Diagnostic)]
        private void Warning(string machineName, string processName, string cv, string tag, string message, string exception)
        {
            if (IsEnabled())
            {
                WriteEvent((int)EventIds.Warning, machineName, processName, cv, tag, message, exception);
            }
        }

        // ========================================================================================
        // ========================================================================================

        [NonEvent]
        public void Verbose(IWorkContext context, string message, Exception exception = null)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotEmpty(nameof(message), message);

            Verbose(_machineName, _processName, context.Cv, context.Tag, message, exception?.ToString());
        }

        [Event((int)EventIds.Verbose, Level = EventLevel.Verbose, Keywords = Keywords.Diagnostic)]
        private void Verbose(string machineName, string processName, string cv, string tag, string message, string exception)
        {
            if (IsEnabled())
            {
                WriteEvent((int)EventIds.Verbose, machineName, processName, cv, tag, message, exception);
            }
        }

        // ========================================================================================
        // ========================================================================================

        [NonEvent]
        public void HttpRequestStart(IWorkContext context, RequestContext requestContext)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotNull(nameof(requestContext), requestContext);

            HttpRequestStart(_machineName, _processName, context.Cv, context.Tag, requestContext.HttpMethod, requestContext?.Uri?.ToString());
        }

        [Event((int)EventIds.HttpRequestStart, Level = EventLevel.Verbose, Keywords = Keywords.Diagnostic)]
        private void HttpRequestStart(string machineName, string processName, string cv, string tag, string method, string url)
        {
            if (IsEnabled())
            {
                WriteEvent((int)EventIds.HttpRequestStart, machineName, processName, cv, tag, method, url);
            }
        }

        // ========================================================================================
        // ========================================================================================

        [NonEvent]
        public void HttpRequestStop(IWorkContext context, RequestContext requestContext, long durationMs)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotNull(nameof(requestContext), requestContext);

            HttpRequestStop(_machineName, _processName, context.Cv, context.Tag, requestContext.HttpMethod, requestContext.Uri.ToString(), durationMs);
        }

        [Event((int)EventIds.HttpRequestStop, Level = EventLevel.Verbose, Keywords = Keywords.Diagnostic)]
        private void HttpRequestStop(string machineName, string processName, string cv, string tag, string method, string url, long durationMs)
        {
            if (IsEnabled())
            {
                WriteEvent((int)EventIds.HttpRequestStop, machineName, processName, cv, tag, method, url, durationMs);
            }
        }

        // ========================================================================================
        // ========================================================================================

        [NonEvent]
        public void HttpError(IWorkContext context, HttpRequest request, HttpResponse response, string message)
        {
            Verify.IsNotNull(nameof(context), context);
            Verify.IsNotNull(nameof(response), response);

            HttpError(_machineName, _processName, context.Cv, context.Tag, request.Method, request.GetEncodedUrl(), (int)response.StatusCode, message);
        }

        [Event((int)EventIds.HttpError, Level = EventLevel.Error, Keywords = Keywords.Diagnostic)]
        private void HttpError(string machineName, string processName, string cv, string tag, string method, string url, int statusCode, string message)
        {
            if (IsEnabled())
            {
                WriteEvent((int)EventIds.HttpError, machineName, processName, cv, tag, method, url, statusCode, message);
            }
        }
    }
}

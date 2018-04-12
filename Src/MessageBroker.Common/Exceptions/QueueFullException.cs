// Copyright (c) KhooverSoft. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Khooversoft.Toolbox;
using System;
using System.Runtime.Serialization;

namespace MessageBroker.Common
{
    [Serializable]
    public class QueueFullException : WorkException
    {
        protected QueueFullException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public QueueFullException(string message, IWorkContext workContext)
            : base(message, workContext)
        {
        }

        public QueueFullException(string message, IWorkContext workContext, Exception inner)
            : base(message, workContext, inner)
        {
        }
    }
}

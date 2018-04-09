using Khooversoft.Toolbox;
using System;
using System.Runtime.Serialization;

namespace MessageBroker.Common
{
    [Serializable]
    public class QueueNotFound : WorkException
    {
        protected QueueNotFound(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public QueueNotFound(string message, IWorkContext workContext)
            : base(message, workContext)
        {
        }

        public QueueNotFound(string message, IWorkContext workContext, Exception inner)
            : base(message, workContext, inner)
        {
        }
    }
}

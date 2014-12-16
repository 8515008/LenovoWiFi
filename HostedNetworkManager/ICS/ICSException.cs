using System;
using System.Runtime.Serialization;

namespace HostedNetworkManager.ICS
{
    public class ICSException : ApplicationException
    {
        public ICSException()
        {
        }

        public ICSException(string message)
            : base(message)
        {
        }

        public ICSException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected ICSException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}

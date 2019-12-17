using System;
using System.Runtime.Serialization;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    [Serializable]
    public class VerificationException : Exception
    {
        public VerificationException()
        {
        }

        public VerificationException(string message) : base(message)
        {
        }

        public VerificationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected VerificationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

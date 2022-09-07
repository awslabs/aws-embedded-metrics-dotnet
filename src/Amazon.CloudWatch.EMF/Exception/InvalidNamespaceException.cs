using System;

namespace Amazon.CloudWatch.EMF
{
    public class InvalidNamespaceException : Exception
    {
        public InvalidNamespaceException()
        {
        }

        public InvalidNamespaceException(string message)
            : base(message)
        {
        }

        public InvalidNamespaceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
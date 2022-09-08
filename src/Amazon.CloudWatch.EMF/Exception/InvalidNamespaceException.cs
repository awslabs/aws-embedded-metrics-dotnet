using System;

namespace Amazon.CloudWatch.EMF
{
    public class InvalidNamespaceException : Exception
    {
        public InvalidNamespaceException(string message)
            : base(message)
        {
        }
    }
}
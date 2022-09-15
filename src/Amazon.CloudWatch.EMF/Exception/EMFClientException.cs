using System;

namespace Amazon.CloudWatch.EMF
{
    public class EMFClientException : Exception
    {
        public EMFClientException()
        {
        }

        public EMFClientException(string message)
            : base(message)
        {
        }

        public EMFClientException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

using System;

namespace Amazon.CloudWatch.EMF
{
    public class InvalidMetricException : Exception
    {
        public InvalidMetricException()
        {
        }

        public InvalidMetricException(string message)
            : base(message)
        {
        }

        public InvalidMetricException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
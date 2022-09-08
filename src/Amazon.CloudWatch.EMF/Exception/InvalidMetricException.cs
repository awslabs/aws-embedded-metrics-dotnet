using System;

namespace Amazon.CloudWatch.EMF
{
    public class InvalidMetricException : Exception
    {
        public InvalidMetricException(string message)
            : base(message)
        {
        }
    }
}
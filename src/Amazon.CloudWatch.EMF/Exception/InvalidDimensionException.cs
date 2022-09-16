using System;

namespace Amazon.CloudWatch.EMF
{
    public class InvalidDimensionException : Exception
    {
        public InvalidDimensionException(string message)
            : base(message)
        {
        }
    }
}

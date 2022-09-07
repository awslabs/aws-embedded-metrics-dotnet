using System;

namespace Amazon.CloudWatch.EMF
{
    public class InvalidDimensionException : Exception
    {
        public InvalidDimensionException()
        {
        }

        public InvalidDimensionException(string message)
            : base(message)
        {
        }

        public InvalidDimensionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
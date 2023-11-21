using System;

namespace Amazon.CloudWatch.EMF
{
    public class InvalidTimestampException : Exception
    {
        public InvalidTimestampException(string message)
          : base(message)
        {
        }
    }
}

using System;

namespace Amazon.CloudWatch.EMF
{
    public class DimensionsExceededException : Exception
    {
        public DimensionsExceededException()
            : base("Maximum number of dimensions allowed are " + Constants.MAX_DIMENSIONS +
                   ". Account for default dimensions if not using SetDimensions.")
        {
        }

        public DimensionsExceededException(string message)
            : base(message)
        {
        }

        public DimensionsExceededException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
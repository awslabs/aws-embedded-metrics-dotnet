using System;

namespace Amazon.CloudWatch.EMF
{
    public class DimensionSetExceededException : Exception
    {
        public DimensionSetExceededException()
            : base("Maximum number of dimensions per dimension set allowed are " + Constants.MAX_DIMENSION_SET_SIZE +
                   ". Account for default dimensions if not using SetDimensions.")
        {
        }

        public DimensionSetExceededException(string message)
            : base(message)
        {
        }

        public DimensionSetExceededException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
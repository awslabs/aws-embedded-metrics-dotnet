using System;

namespace Amazon.CloudWatch.EMF
{
    public class DimensionSetExceededException : Exception
    {
        public DimensionSetExceededException()
            : base("Maximum number of dimensions per dimension set allowed are " + Constants.MaxDimensionSetSize +
                   ". Account for default dimensions if not using SetDimensions.")
        {
        }
    }
}

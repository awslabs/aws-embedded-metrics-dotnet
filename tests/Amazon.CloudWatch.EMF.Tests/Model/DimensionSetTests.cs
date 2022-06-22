using Amazon.CloudWatch.EMF.Model;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Model
{
    public class DimensionSetTests
    {
        public void AddDimension_LimitExceededError()
        {
            Assert.Throws<DimensionsExceededException>(() =>
            {
                const int dimensionsToBeAdded = 33;
                var ds = new DimensionSet();

                for (var i = 0; i < dimensionsToBeAdded; i++)
                {
                    ds.AddDimension("Dimension" + 1, "value" + 1);
                }
            });
        }

    }
}
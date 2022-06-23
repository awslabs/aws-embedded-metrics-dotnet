using Amazon.CloudWatch.EMF.Model;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Model
{
    public class DimensionSetTests
    {
        private DimensionSet Get_DimensionSet(int dimensionSetSize)
        {
            var dimensionSet = new DimensionSet();
            for (var i = 0; i < dimensionSetSize; i++)
            {
                dimensionSet.AddDimension("Dimension" + 1, "value" + 1);
            }

            return dimensionSet;
        }

        [Fact]
        public void AddDimension_30_Dimensions()
        {
            const int dimensionSetSize = 30;
            var dimensionSet = Get_DimensionSet(dimensionSetSize);

            Assert.Equal(dimensionSetSize, dimensionSet.DimensionKeys.Count);
        }

        [Fact]
        public void AddDimension_Limit_Exceeded_Error()
        {
            Assert.Throws<DimensionSetExceededException>(() =>
            {
                const int dimensionSetSize = 33;
                Get_DimensionSet(dimensionSetSize);
            });
        }

        [Fact]
        public void AddRange_Limit_Exceeded_Error() {
            Assert.Throws<DimensionSetExceededException>(() =>
            {
                const int dimensionSetSize = 28;
                const int otherDimensionSetSize = 5;
                var dimensionSet = Get_DimensionSet(dimensionSetSize);
                var otherDimensionSet = Get_DimensionSet(otherDimensionSetSize);

                dimensionSet.AddRange(otherDimensionSet);
            });
        }

    }
}
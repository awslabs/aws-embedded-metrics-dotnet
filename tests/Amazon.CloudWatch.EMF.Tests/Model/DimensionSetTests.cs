using Amazon.CloudWatch.EMF.Model;
using Amazon.CloudWatch.EMF.Utils;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Model
{
    public class DimensionSetTests
    {
        private static DimensionSet Get_DimensionSet(int dimensionSetSize, string key = "key")
        {
            var dimensionSet = new DimensionSet();
            for (var i = 0; i < dimensionSetSize; i++)
            {
                dimensionSet.AddDimension(key + i, "value" + i);
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

        [Theory]
        [InlineData(null, "value")]
        [InlineData(" ", "value")]
        [InlineData("ďïɱ", "value")]
        [InlineData("dim", null)]
        [InlineData("dim", " ")]
        [InlineData("dim", "ⱱẵĺ")]
        [InlineData(":dim", "val")]
        public void AddDimension_WithInvalidValues_ThrowsInvalidDimensionException(string key, string value)
        {
            Assert.Throws<InvalidDimensionException>(() =>
            {
                var dimensionSet = new DimensionSet();
                dimensionSet.AddDimension(key, value);
            });
        }

        [Fact]
        public void AddDimension_WithNameTooLong_ThrowsInvalidDimensionException()
        {
            Assert.Throws<InvalidDimensionException>(() =>
            {
                var dimensionSet = new DimensionSet();
                dimensionSet.AddDimension(new string('a', Constants.MAX_DIMENSION_NAME_LENGTH + 1), "value");
            });
        }

        [Fact]
        public void AddDimension_WithValueTooLong_ThrowsInvalidDimensionException()
        {
            Assert.Throws<InvalidDimensionException>(() =>
            {
                var dimensionSet = new DimensionSet();
                dimensionSet.AddDimension("name", new string('a', Constants.MAX_DIMENSION_VALUE_LENGTH + 1));
            });
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
        public void AddRange_Limit_Exceeded_Error()
        {
            Assert.Throws<DimensionSetExceededException>(() =>
            {
                const int dimensionSetSize = 28;
                const int otherDimensionSetSize = 5;
                var dimensionSet = Get_DimensionSet(dimensionSetSize);
                var otherDimensionSet = Get_DimensionSet(otherDimensionSetSize, key: "otherKey");

                dimensionSet.AddRange(otherDimensionSet);
            });
        }

    }
}
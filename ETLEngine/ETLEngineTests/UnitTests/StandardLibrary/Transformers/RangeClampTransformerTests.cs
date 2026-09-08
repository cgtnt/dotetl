using ETLEngine.StandardLibrary.Transformers;

namespace ETLEngineTests.UnitTests.StandardLibrary.Transformers
{
    public class RangeClampTransformerTests
    {
        [Theory]
        [InlineData(1, 5, 3)]
        [InlineData(1, 5, 1)]
        [InlineData(1, 5, 5)]
        public void AlredyWithinRange(int min, int max, int value)
        {
            RangeClampTransformer<int> transformer = new(min, max);
            Assert.Equal(value, transformer.Transform(value));
        }

        [Theory]
        [InlineData(1, 5, 0)]
        public void ClampMin(int min, int max, int value)
        {
            RangeClampTransformer<int> transformer = new(min, max);
            Assert.Equal(min, transformer.Transform(value));
        }

        [Theory]
        [InlineData(1, 5, 7)]
        public void ClampMax(int min, int max, int value)
        {
            RangeClampTransformer<int> transformer = new(min, max);
            Assert.Equal(max, transformer.Transform(value));
        }
    }
}

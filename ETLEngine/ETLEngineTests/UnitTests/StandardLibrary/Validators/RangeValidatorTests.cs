using ETLEngine.StandardLibrary.Validators;

namespace ETLEngineTests.UnitTests.StandardLibrary.Validators
{
    public class RangeValidatorTests
    {
        [Theory]
        [InlineData(1, 5, 3)]
        [InlineData(-2,-1,-2)]
        [InlineData(-2, -1, -1)]
        [InlineData(-1, -1, -1)]
        public void ValidRangeValidData(int min, int max, int data)
        {
            RangeValidator<int> validator = new(min, max);
            Assert.True(validator.Validate(data));
        }

        [Theory]
        [InlineData(1, 5, 6)]
        [InlineData(-1, -1, -2)]
        [InlineData(-1, -1, 0)]
        public void ValidRangeInvalidData(int min, int max, int data)
        {
            RangeValidator<int> validator = new(min, max);
            Assert.False(validator.Validate(data));
        }

        [Theory]
        [InlineData(6, 5)]
        public void InvalidRange(int min, int max)
        {
            Assert.Throws<ApplicationException>(() => new RangeValidator<int>(min, max));
        }
    }
}

using ETLEngine.StandardLibrary.Validators;

namespace ETLEngineTests.UnitTests.StandardLibrary.Validators
{
    public class AllowedValuesValidatorTests
    {
        [Theory]
        [InlineData(6, 5, 6, 2)]
        [InlineData(6, 5, 6, 6, 2)]
        public void IsValidValue(int data, params int[] values)
        {
            AllowedValuesValidator<int> validator = new(values);
            Assert.True(validator.Validate(data));
        }

        [Theory]
        [InlineData(6, 5, 3, 2)]
        [InlineData(6)]
        public void IsNotValidValue(int data, params int[] values)
        {
            AllowedValuesValidator<int> validator = new(values);
            Assert.False(validator.Validate(data));
        }
    }
}

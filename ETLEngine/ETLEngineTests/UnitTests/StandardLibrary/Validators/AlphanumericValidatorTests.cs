using ETLEngine.StandardLibrary.Validators;

namespace ETLEngineTests.UnitTests.StandardLibrary.Validators
{
    public class AlpanumericValidatorTests
    {
        AlphanumericValidator validator = new();

        [Theory]
        [InlineData("ahasga")]
        [InlineData("")]
        [InlineData("as8t59")]
        public void IsAlphaNumeric(string data)
        {
            Assert.True(validator.Validate(data));
        }

        [Theory]
        [InlineData("_")]
        [InlineData("$3kao5")]
        [InlineData("-")]
        public void IsNotAlphaNumeric(string data)
        {
            Assert.False(validator.Validate(data));
        }
    }
}

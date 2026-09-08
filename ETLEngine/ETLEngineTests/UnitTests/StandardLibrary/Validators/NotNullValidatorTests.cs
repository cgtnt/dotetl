using ETLEngine.StandardLibrary.Validators;

namespace ETLEngineTests.UnitTests.StandardLibrary.Validators
{
    public class NotNullValidatorTests
    {
        NotNullValidator<string> validator = new();

        [Fact]
        public void IsNull()
        {
            string? data = null;
            Assert.False(validator.Validate(data));
        }

        [Fact]
        public void IsNotNull()
        {
            string? data = "not null";
            Assert.True(validator.Validate(data));
        }
    }
}

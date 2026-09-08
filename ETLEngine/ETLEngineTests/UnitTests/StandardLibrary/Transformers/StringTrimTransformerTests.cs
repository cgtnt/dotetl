using ETLEngine.StandardLibrary.Transformers;

namespace ETLEngineTests.UnitTests.StandardLibrary.Transformers
{
    public class StringTrimTransformerTests
    {
        StringTrimTransformer transformer = new();

        [Theory]
        [InlineData("something")]
        [InlineData("")]
        public void AlredyTrimmed(string value)
        {
            Assert.Equal(value, transformer.Transform(value));
        }

        [Theory]
        [InlineData("something  ", "something")]
        [InlineData("  something ", "something")]
        [InlineData(" something ", "something")]
        [InlineData(" something  word ", "something  word")]
        [InlineData(" something \n", "something")]
        [InlineData("  ", "")]
        [InlineData(" \n", "")]
        public void ToBeTrimmed(string value, string expected)
        {
            Assert.Equal(expected, transformer.Transform(value));
        }
    }
}

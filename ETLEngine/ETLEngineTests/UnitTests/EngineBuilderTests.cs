using ETLEngine;

namespace ETLEngineTests.UnitTests
{
    public class EngineBuilderTests
    {
        [Fact]
        public void EngineBuilderNoTransformer() {
            EngineBuilder<string, int> builder = new();
            Assert.Throws<ApplicationException>(() => builder.Build());
        }
    }
}

namespace ETLEngineTests
{
    public class StreamEqualityCheckerTests
    {
        [Fact]
        public void EmptyStreams()
        {
            MemoryStream output = new();
            MemoryStream expectedOutput = new();

            Assert.True(StreamEqualityChecker.Equal(expectedOutput, output));
        }

        [Fact]
        public void EqualStreams()
        {
            MemoryStream output = new();
            StreamWriter outputWriter = new(output);
            outputWriter.Write("sagasagsgagasgashbasgasfa");
            outputWriter.Flush();

            MemoryStream expectedOutput = new();
            StreamWriter expectedWriter = new(expectedOutput);
            expectedWriter.Write("sagasagsgagasgashbasgasfa");
            expectedWriter.Flush();

            Assert.True(StreamEqualityChecker.Equal(expectedOutput, output));
        }

        [Fact]
        public void NotEqualStreams()
        {
            MemoryStream output = new();
            StreamWriter outputWriter = new(output);
            outputWriter.Write("sagasagsgaasgashbasgasfa");
            outputWriter.Flush();

            MemoryStream expectedOutput = new();
            StreamWriter expectedWriter = new(expectedOutput);
            expectedWriter.Write("sagasagsgagasgashbasgasfa");
            expectedWriter.Flush();

            Assert.False(StreamEqualityChecker.Equal(expectedOutput, output));
        }
    }
}
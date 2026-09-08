namespace ETLEngineTests.UnitTests.StandardLibrary.LoadingDestinations
{
    using ETLEngine.Interfaces;
    using ETLEngine.StandardLibrary.LoadingDestinations;

    public class StreamWriterLoadingDestinationTests
    {
        private class MockRecordWriter(
            string failureCondition    
        ) : IStreamWriterRecordWriter<string>
        {
            public void WriteRecord(string record, StreamWriter writer)
            {
                if (record == failureCondition)
                    throw new ApplicationException();

                writer.Write(record);
            }
        }

        [Fact]
        public void TestNoFail() {
            string failureCondition = "FAIL";
            MockRecordWriter recordWriter = new(failureCondition);

            MemoryStream output = new();
            StreamWriter outputWriter = new(output);

            StreamWriterLoadingDestination<string> destination = new(outputWriter, recordWriter);
            destination.Load(["Ana", "Mana", "Cana", "Bana"]);
            outputWriter.Flush();

            MemoryStream expectedOutput = new();
            StreamWriter expectedOutputWriter = new(expectedOutput);
            expectedOutputWriter.Write("AnaManaCanaBana");
            expectedOutputWriter.Flush();

            Assert.True(StreamEqualityChecker.Equal(output, expectedOutput));
        }

        [Fact]
        public void TestFail()
        {
            string failureCondition = "FAIL";
            MockRecordWriter recordWriter = new(failureCondition);

            MemoryStream output = new();
            StreamWriter outputWriter = new(output);

            StreamWriterLoadingDestination<string> destination = new(outputWriter, recordWriter);
            Assert.Throws<ApplicationException>(() => destination.Load(["Ana", "Mana", "Cana", failureCondition, "Bana"]));
        }
    }
}

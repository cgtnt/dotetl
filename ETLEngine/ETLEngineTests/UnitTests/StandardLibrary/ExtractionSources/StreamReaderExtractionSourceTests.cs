using ETLEngine.StandardLibrary.ExtractionSources;

namespace ETLEngineTests.UnitTests.StandardLibrary.ExtractionSources
{
    using ETLEngine.Interfaces;

    public class StreamReaderExtractionSourceTests
    {
        private class MockRecordReader(
            IEnumerable<string> records
        ) : IStreamReaderRecordReader
        {
            public IEnumerable<string> ReadRecords(StreamReader reader) => records;
        }
        
        private class MockParser(
            string failureCondition    
        ) : IParser<PersonTDataExample>
        {
            public PersonTDataExample Parse(string input)
            {
                if (input == failureCondition)
                    throw new ApplicationException();
                return new PersonTDataExample();
            }
        }

        [Fact]
        public void Test() {
            string failureCondition = "FAIL";
            MockRecordReader recordReader = new(["Ana", "Bana", "Cana", failureCondition, "Dana"]);
            MockParser parser = new(failureCondition);

            MemoryStream input = new();
            StreamReader inputReader = new(input);

            StreamReaderExtractionSource<PersonTDataExample> extractor = new(inputReader, recordReader, parser);
            var result = extractor.Extract().ToArray();

            int success = result.Where((res) => res.IsSuccess).Count();
            int expectedSuccess = 4;

            Assert.Equal(expectedSuccess, success);
        }
    }
}

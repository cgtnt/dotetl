using ETLEngine.StandardLibrary.RecordWriters;
using ETLEngine.Interfaces;

namespace ETLEngineTests.UnitTests.StandardLibrary.RecordWriters
{
    file class WriteTSerializer<TData> : IStreamWriterSerializer<TData>
    {
        public void Serialize(TData person, StreamWriter writer)
        {
            writer.Write('T');
        }
    }

    public class DelimitedRecordWriterTests
    {
        [Theory]
        [InlineData(",", "T,")]
        [InlineData("|aa|", "T|aa|")]
        [InlineData("|aa|\n", "T|aa|\n")]
        public void Write(
            string separator,
            string expected
        ) {
            MemoryStream output = new();
            StreamWriter outputWriter = new(output);

            MemoryStream expectedOutput = new();
            StreamWriter expectedWriter = new(expectedOutput);
            expectedWriter.Write(expected);
            expectedWriter.Flush();

            PersonTDataExample person = new();
            WriteTSerializer<PersonTDataExample> serializer = new();

            DelimitedRecordWriter<PersonTDataExample> recordWriter = new(
                separator,
                serializer
            );

            recordWriter.WriteRecord(person, outputWriter);
            outputWriter.Flush();

            Assert.True(StreamEqualityChecker.Equal(expectedOutput, output));
        }
    }
}

using ETLEngine.StandardLibrary.Serializers;

namespace ETLEngineTests.UnitTests.StandardLibrary.Serializers
{
    public class JsonSerializerTests
    {
        [Theory]
        [InlineData("""{"FullName":"Ana","Age":32}""", "Ana", 32)]
        public void SerializeRow(
            string expected,
            string name,
            int age
        ) {
            MemoryStream output = new();
            StreamWriter outputWriter = new(output);

            MemoryStream expectedOutput = new();
            StreamWriter expectedWriter = new(expectedOutput);
            expectedWriter.Write(expected);
            expectedWriter.Flush();

            JsonStreamWriterSerializer<PersonTDataExample> serialzier = new();

            PersonTDataExample person = new();
            person.FullName = name;
            person.Age = age;

            serialzier.Serialize(person, outputWriter);
            outputWriter.Flush();

            Assert.True(StreamEqualityChecker.Equal(expectedOutput, output));
        }
    }
}

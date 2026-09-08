using ETLEngine.StandardLibrary.Serializers;

namespace ETLEngineTests.UnitTests.StandardLibrary.Serializers
{
    using System.Reflection;

    public class CsvSerializerTests
    {
        [Theory]
        [InlineData(',', "head1,head2,head3", "head1", "head2", "head3")]
        [InlineData(' ', "head1 head2 head3", "head1", "head2", "head3")]
        [InlineData('\n', "head1\nhead2\nhead3", "head1", "head2", "head3")]
        [InlineData('.', "")]
        public void SerializeHeader(
            char separator,
            string expected,
            params string[] header
        ) {
            MemoryStream output = new();
            StreamWriter outputWriter = new(output);

            MemoryStream expectedOutput = new();
            StreamWriter expectedWriter = new(expectedOutput);
            expectedWriter.Write(expected);
            expectedWriter.Flush();

            CsvHeaderStreamWriterSerializer serialzier = new(separator);
            serialzier.Serialize(header, outputWriter);
            outputWriter.Flush();

            Assert.True(StreamEqualityChecker.Equal(expectedOutput, output));
        }

        [Theory]
        [InlineData(',', "Ana,32", "Ana", 32)]
        [InlineData('|', "Ana|32", "Ana", 32)]
        [InlineData('|', "|32", "", 32)]
        public void SerializeRow(
            char separator,
            string expected,
            string name,
            int age
        )
        {
            MemoryStream output = new();
            StreamWriter outputWriter = new(output);

            MemoryStream expectedOutput = new();
            StreamWriter expectedWriter = new(expectedOutput);
            expectedWriter.Write(expected);
            expectedWriter.Flush();

            PropertyInfo[] propertyInfos = { 
                typeof(PersonTDataExample).GetProperty("FullName"),
                typeof(PersonTDataExample).GetProperty("Age")
            };

            CsvStreamWriterSerializer<PersonTDataExample> serialzier = new(
                separator,
                propertyInfos
            );

            PersonTDataExample person = new();
            person.FullName = name;
            person.Age = age;

            serialzier.Serialize(person, outputWriter);
            outputWriter.Flush();

            Assert.True(StreamEqualityChecker.Equal(expectedOutput, output));
        }
    }
}

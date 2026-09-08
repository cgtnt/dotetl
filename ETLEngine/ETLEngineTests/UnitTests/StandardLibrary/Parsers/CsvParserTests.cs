using ETLEngine.StandardLibrary.Parsers;

namespace ETLEngineTests.UnitTests.StandardLibrary.Parsers
{
    using System.Reflection;

    public class CsvParserTests
    {
        [Theory]
        [InlineData(',', "Ana,32", "Ana", 32)]
        [InlineData('|', "Ana|32", "Ana", 32)]
        public void ParseRow(
            char separator,
            string input,
            string expectedName,
            int expectedAge
        ) {
            Dictionary<PropertyInfo, int> keymap = new Dictionary<PropertyInfo, int> {
                { typeof(PersonTDataExample).GetProperty("FullName"), 0 },
                { typeof(PersonTDataExample).GetProperty("Age"), 1 }
            };

            CsvParser<PersonTDataExample> parser = new(
                separator,
                keymap
            );

            var person = parser.Parse(input);

            Assert.Equal(expectedName, person.FullName);
            Assert.Equal(expectedAge, person.Age);
        }
    }
}

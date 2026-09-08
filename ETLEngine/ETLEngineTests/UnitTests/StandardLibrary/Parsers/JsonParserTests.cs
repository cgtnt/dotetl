using ETLEngine.StandardLibrary.Parsers;

namespace ETLEngineTests.UnitTests.StandardLibrary.Parsers
{
    using System.Reflection;

    public class JsonParserTests
    {
        [Theory]
        [InlineData("""{"FullName":"Ana","Age":32}""", "Ana", 32)]
        [InlineData("""{"name":"Ana","Age":32}""", "Ana", 32)]
        [InlineData("""{"FullName":"Ana","years_old":32}""", "Ana", 32)]
        public void ParseRow(
            string input,
            string expectedName,
            int expectedAge
        ) {
            string[] NameAliases = { "FullName", "name", "fullname" };
            string[] AgeAliases = { "Age", "age", "years_old" };

            Dictionary<PropertyInfo, string[]> keymap = new Dictionary<PropertyInfo, string[]> {
                { typeof(PersonTDataExample).GetProperty("FullName"), NameAliases },
                { typeof(PersonTDataExample).GetProperty("Age"), AgeAliases }
            };

            JsonParser<PersonTDataExample> parser = new(
                keymap
            );

            var person = parser.Parse(input);

            Assert.Equal(expectedName, person.FullName);
            Assert.Equal(expectedAge, person.Age);
        }
    }
}

namespace ETLEngine.StandardLibrary.Parsers
{
    using ETLEngine.Interfaces;
    using System.Reflection;
    using System.Text.Json;

    /// <summary>
    /// Parases a JSON string into a <seealso cref="TData"/>.
    /// </summary>
    /// <typeparam name="TData">Type of object to parse</typeparam>
    /// <param name="propertiesKeyMap">Mapping from <seealso cref="PropertyInfo"/>'s of <seealso cref="TData"/> to potential aliases of said property in the JSON string</param>
    public class JsonParser<TData>(
        Dictionary<PropertyInfo, string[]> propertiesKeyMap
    ) : IParser<TData> where TData : new()
    {
        /// <summary>
        /// Parses a JSON string into a <seealso cref="TData"/>.
        /// </summary>
        /// <param name="jsonStr">String to parse</param>
        /// <returns>Parsed data</returns>
        public TData Parse(string jsonStr)
        {
            using JsonDocument document = JsonDocument.Parse(jsonStr);
            JsonElement root = document.RootElement;

            TData output = new();

            foreach (var (propertyInfo, keys) in propertiesKeyMap)
            {
                foreach (var key in keys)
                    if (root.TryGetProperty(key, out JsonElement jsonValue))
                    {
                        var value = jsonValue.Deserialize(propertyInfo.PropertyType);
                        propertyInfo.SetValue(output, value);
                        break;
                    }
            }

            return output;
        }
    }
}

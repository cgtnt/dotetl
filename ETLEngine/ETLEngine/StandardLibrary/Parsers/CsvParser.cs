namespace ETLEngine.StandardLibrary.Parsers
{
    using ETLEngine.Interfaces;
    using System.Reflection;

    /// <summary>
    /// Parses a CSV header.
    /// </summary>
    /// <param name="separator">CSV Field separator</param>
    /// <param name="propertiesKeyMap">Mapping of each <seealso cref="PropertyInfo"/> of a desired type to a list of aliases, ie. header names which the parser should look for.</param>
    public class CsvHeaderParser(
        char separator,
        Dictionary<PropertyInfo, string[]> propertiesKeyMap
    ) {
        /// <summary>
        /// Parses a CSV header into a mapping from <seealso cref="PropertyInfo"/>'s of a desired type to a column index in the CSV file.
        /// </summary>
        /// <param name="header">Header to parse</param>
        /// <returns>Mapping of properties to column indices</returns>
        /// <exception cref="ApplicationException">Thrown if for a given <seealso cref="PropertyInfo"/>, no header field with any of the listed aliases was found.</exception>
        public Dictionary<PropertyInfo, int> Parse(string header)
        {
            var fields = header.Split(separator);
            Dictionary<PropertyInfo, int> output = new();

            foreach (var (propertyInfo, possibleNames) in propertiesKeyMap)
            {
                bool found = false;

                foreach (var (idx, field) in fields.Index())
                    if (possibleNames.Contains(field))
                    {
                        found = true;
                        output.Add(propertyInfo, idx);
                        break;
                    }

                if (!found)
                    throw new ApplicationException($"Cannot find any of the provided keys [{string.Join(',', possibleNames)}] for property {propertyInfo.Name} in CSV header {header}");
            }

            return output;
        }
    }

    /// <summary>
    /// Parses CSV data rows into <seealso cref="TData"/>.
    /// </summary>
    /// <typeparam name="TData">Type of objects to parse</typeparam>
    /// <param name="separator">CSV Field separator</param>
    /// <param name="columnIdxMap">Mapping from <seealso cref="PropertyInfo"/>'s of <seealso cref="TData"/> to column indices in the CSV file</param>
    public class CsvParser<TData>(
        char separator,
        Dictionary<PropertyInfo, int> columnIdxMap
    ) : IParser<TData> where TData : new()
    {
        /// <summary>
        /// Parses a string to a <seealso cref="TData"/> object.
        /// </summary>
        /// <param name="str">String to parse</param>
        /// <returns>Parsed object</returns>
        public TData Parse(string str)
        {
            var fields = str.Split(separator);
            TData output = new();

            foreach (var (propertyInfo, columnIdx) in columnIdxMap)
                propertyInfo.SetValue(output, Convert.ChangeType(fields[columnIdx], propertyInfo.PropertyType));

            return output;
        }
    }
}

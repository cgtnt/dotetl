using ETLEngine.Interfaces;
using System.Reflection;

namespace ETLEngine.StandardLibrary.Serializers
{
    /// <summary>
    /// Serializes a CSV header to a <seealso cref="StreamWriter"/>.
    /// </summary>
    /// <param name="separator">CSV field separator</param>
    public class CsvHeaderStreamWriterSerializer(
       char separator
    ) {
        /// <summary>
        /// Serializes a CSV header to a <seealso cref="StreamWriter"/>.
        /// <param name="labels">Header field labels</param>
        /// <param name="writer">Output writer</param>
        public void Serialize(string[] labels, StreamWriter writer)
        {
            for (int labelIdx = 0; labelIdx < labels.Length; ++labelIdx)
            {
                writer.Write(labels[labelIdx]);

                if (labelIdx < labels.Length - 1)
                    writer.Write(separator);
            }
        }
    }

    /// <summary>
    /// Serializes a <seealso cref="TData"/> object to a <seealso cref="StreamWriter"/>.
    /// </summary>
    /// <typeparam name="TData">Type of object to serialize</typeparam>
    /// <param name="fieldSeparator">CSV field separator</param>
    /// <param name="propertyInfos">List of <seealso cref="TData"/>'s <seealso cref="PropertyInfo"/>'s in order of which they should be serialized</param>
    public class CsvStreamWriterSerializer<TData>(
        char fieldSeparator,
        PropertyInfo[] propertyInfos
    ) : IStreamWriterSerializer<TData> {
        /// <summary>
        /// Serializes a <seealso cref="TData"/> object to a <seealso cref="StreamWriter"/>.
        /// </summary>
        /// <param name="data">Data object to serialize</param>
        /// <param name="writer">Output writer</param>
        public void Serialize(TData data, StreamWriter writer)
        {
            for (int propertyIdx = 0; propertyIdx < propertyInfos.Length; ++propertyIdx)
            {
                var propertyInfo = propertyInfos[propertyIdx];
                writer.Write(
                    propertyInfo.GetValue(data)
                );

                if (propertyIdx < propertyInfos.Length - 1)
                    writer.Write(fieldSeparator);
            }
        }
    }
}

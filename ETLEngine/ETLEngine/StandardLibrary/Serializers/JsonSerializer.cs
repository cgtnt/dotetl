using ETLEngine.Interfaces;

namespace ETLEngine.StandardLibrary.Serializers
{
    /// <summary>
    /// Serializes a <seealso cref="TData"/> object to a <seealso cref="StreamWriter"/>.
    /// </summary>
    /// <typeparam name="TData">Data object to serialize</typeparam>
    public class JsonStreamWriterSerializer<TData> : IStreamWriterSerializer<TData>
    {
        /// <summary>
        /// Serializes a <seealso cref="TData"/> object to a <seealso cref="StreamWriter"/>.
        /// </summary>
        /// <param name="data">Data object to serialize</param>
        /// <param name="writer">Output writer</param>
        public void Serialize(TData data, StreamWriter writer)
        {
            string jsonText = System.Text.Json.JsonSerializer.Serialize(data);
            writer.Write(jsonText);
        }
    }
}

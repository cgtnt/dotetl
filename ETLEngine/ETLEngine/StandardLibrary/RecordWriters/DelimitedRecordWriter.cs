using ETLEngine.Interfaces;

namespace ETLEngine.StandardLibrary.RecordWriters
{
    /// <summary>
    /// Writes records separated by a delimiter.
    /// </summary>
    /// <typeparam name="TData">Type of record to write</typeparam>
    /// <param name="delimiter">Record delimiter</param>
    /// <param name="serializer">Record serializer</param>
    public class DelimitedRecordWriter<TData>(
        string delimiter,
        IStreamWriterSerializer<TData> serializer
    ) : IStreamWriterRecordWriter<TData>
    {
        /// <summary>
        /// Writes a record and a delimiter.
        /// </summary>
        /// <param name="record">Record to write</param>
        /// <param name="writer">Output writer</param>
        public void WriteRecord(TData record, StreamWriter writer)
        {
            serializer.Serialize(record, writer);
            writer.Write(delimiter);
        }
    }
}

namespace ETLEngine.Interfaces
{
    /// <summary>
    /// Writes a <seealso cref="TData"/> into a <seealso cref="StreamWriter"/>.
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public interface IStreamWriterRecordWriter<TData>
    {
        void WriteRecord(TData record, StreamWriter writer);
    }
}

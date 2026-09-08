namespace ETLEngine.Interfaces
{
    /// <summary>
    /// Reads a <seealso cref="StreamReader"/> into an <seealso cref="IEnumerable{string}"/> of records to be parsed.
    /// </summary>
    public interface IStreamReaderRecordReader
    {
        IEnumerable<string> ReadRecords(StreamReader streamReader);
    }
}

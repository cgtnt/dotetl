using ETLEngine.Interfaces;

namespace ETLEngine.StandardLibrary.RecordReaders
{
    /// <summary>
    /// Reads records separated by newline
    /// </summary>
    public class NewlineRecordReader : IStreamReaderRecordReader
    {
        /// <summary>
        /// Reads records separated by newline
        /// </summary>
        /// <param name="reader">Input reader</param>
        /// <returns>Records</returns>
        public IEnumerable<string> ReadRecords(StreamReader reader)
        {
            while (reader.ReadLine() is string line)
                yield return line;
        }
    }
}

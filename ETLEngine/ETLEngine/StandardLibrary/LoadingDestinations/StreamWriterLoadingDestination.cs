namespace ETLEngine.StandardLibrary.LoadingDestinations
{
    using System.Text;
    using ETLEngine.StandardLibrary.Serializers;
    using ETLEngine.StandardLibrary.RecordWriters;
    using ETLEngine.Interfaces;

    /// <summary>
    /// Loads <seealso cref="TData"/> objects to a <seealso cref="StreamWriter"/>.
    /// </summary>
    /// <typeparam name="TData">Type of objects to load</typeparam>
    /// <param name="output">Output writer</param>
    /// <param name="recordWriter">Writes records to the <paramref name="output"/></param>
    public class StreamWriterLoadingDestination<TData>(
        StreamWriter output,
        IStreamWriterRecordWriter<TData> recordWriter
    ) : ILoadingDestination<TData>
    {
        public StreamWriterLoadingDestination(StreamWriter output)
            : this(
                  output,
                  new DelimitedRecordWriter<TData>(
                    Environment.NewLine,
                    new JsonStreamWriterSerializer<TData>()
                  )
            )
        { }

        public void Load(IEnumerable<TData> data)
        {
            StringBuilder exceptionSb = new();

            foreach (var item in data)
                try
                {
                    recordWriter.WriteRecord(item, output);
                }
                catch (Exception ex)
                {
                    exceptionSb.AppendLine($"[SERIALIZATION] {ex.Message}");
                }

            if (exceptionSb.Length > 0)
                throw new ApplicationException(exceptionSb.ToString());
        }
    }
}

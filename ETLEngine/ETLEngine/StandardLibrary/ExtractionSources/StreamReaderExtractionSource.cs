 namespace ETLEngine.StandardLibrary.ExtractionSources
{
    using ETLEngine.Interfaces;
    using ETLEngine.StandardLibrary.Parsers;
    using ETLEngine.StandardLibrary.RecordReaders;

    /// <summary>
    /// Extractions <seealso cref="TData"/> objects from a <seealso cref="StreamReader"/>.
    /// </summary>
    /// <typeparam name="TData">Type of objects to be extracted from the data source</typeparam>
    /// <param name="input">Input reader</param>
    /// <param name="recordReader">Record reader used to split the input from the <paramref name="input"/></param>
    /// <param name="parser">Parsed used to parse the records given by the <paramref name="recordReader"/></param>
    public class StreamReaderExtractionSource<TData>(
        StreamReader input,
        IStreamReaderRecordReader recordReader,
        IParser<TData> parser
    ) : IExtractionSource<TData> where TData : new()
    {
        public StreamReaderExtractionSource(StreamReader input, IParser<TData> parser)
            : this(input, new NewlineRecordReader(), parser) { }

        public StreamReaderExtractionSource(StreamReader input) : this(
            input,
            new JsonParser<TData>(
                new ReflectionTypePropertyMappingProvider<TData, string[]>(
                    new PropertyAttributeInputKeysStrategy()
                ).Discover()
            )
        )
        { }

        public IEnumerable<InstanceProcessingResult<TData>> Extract()
        {
            var records = recordReader.ReadRecords(input);
            foreach (var record in records)
            {
                InstanceProcessingResult<TData> result;

                try
                {
                    var parsed = parser.Parse(record);
                    result = new InstanceProcessingResult<TData>(parsed);
                }
                catch (Exception ex)
                {
                    result = new InstanceProcessingResult<TData>(
                        new Exception($"[PARSING] {ex.Message}")
                    );
                }

                yield return result;
            }
        }
    }
}

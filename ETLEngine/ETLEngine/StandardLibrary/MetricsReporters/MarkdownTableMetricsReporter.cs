namespace ETLEngine.StandardLibrary.MetricsReporters
{
    using ETLEngine.Interfaces;
    using System.Text;
   
    /// <summary>
    /// Serializes a <seealso cref="EngineMetrics"/> to markdown tables.
    /// </summary>
    /// <param name="writer">Writer to which the table should be printed</param>
    public class MarkdownTableMetricsReporter(
        StreamWriter writer
    ) : IMetricsReporter {
        private readonly string[] timingHeader = { "Start time", "End time", "Duration" };
        private readonly string[] statsHeader = { 
            "Extraction exceptions",
            "Passed validation",
            "Failed validation",
            "Transformed",
            "Transformation exceptions",
            "Loaded",
            "Loading exceptions"
        };

        private const string alignmentOptions = ":---";
       
        private string GenerateRow(params string[] entries)
        {
            StringBuilder sb = new("|");
            foreach (var entry in entries)
                sb.Append($" {entry} |");
            return sb.ToString();
        }

        private void WriteTable(string header, string formatting, params string[] rows)
        {
            writer.WriteLine(header);
            writer.WriteLine(formatting);

            foreach (var row in rows)
                writer.WriteLine(row);
        }

        public void Report(EngineMetrics metrics)
        {
            WriteTable(
                GenerateRow(timingHeader),
                GenerateRow(Enumerable.Repeat(alignmentOptions, timingHeader.Length).ToArray()),
                GenerateRow(
                    metrics.StartTime.ToString(),
                    metrics.EndTime.ToString(),
                    metrics.Duration.ToString()
                )
            );

            writer.WriteLine();

            WriteTable(
                GenerateRow(statsHeader),
                GenerateRow(Enumerable.Repeat(alignmentOptions, statsHeader.Length).ToArray()),
                GenerateRow(
                    metrics.ExtractionExceptions.ToString(),
                    metrics.PassedValidation.ToString(),
                    metrics.FailedValidation.ToString(),
                    metrics.Transformed.ToString(),
                    metrics.TransformationExceptions.ToString(),
                    metrics.Loaded.ToString(),
                    metrics.LoadingExceptions.ToString()
                )
            );
        }
    }
}

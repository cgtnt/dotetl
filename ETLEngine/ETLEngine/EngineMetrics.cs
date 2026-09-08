namespace ETLEngine
{
    /// <summary>
    /// Metrics of a single <seealso cref="EngineBuilder{TIn, TOut}"/> pipeline run.
    /// </summary>
    /// <param name="StartTime">Start time of the pipeline run.</param>
    /// <param name="EndTime">End time of the pipeline run.</param>
    /// <param name="Duration">Duration of the pipeline run.</param>
    /// <param name="ExtractionExceptions">Number of exceptions throws while exctracting data.</param>
    /// <param name="PassedValidation">Number of objects that passed validation and were enqueued for transformation.</param>
    /// <param name="FailedValidation">Number of objects that failed validation and were discarded.</param>
    /// <param name="Transformed">Number of objects succesfully transformed.</param>
    /// <param name="TransformationExceptions">Number of exceptions thrown while applying transformations.</param>
    /// <param name="Loaded">Number of objects successfuly loaded to the destination.</param>
    /// <param name="LoadingExceptions">Number of batch-level exceptions thrown while loading batches to destination.</param>
    public record struct EngineMetrics
    (
        DateTime StartTime,
        DateTime EndTime,

        TimeSpan Duration,

        long ExtractionExceptions,

        long PassedValidation,
        long FailedValidation,

        long Transformed,
        long TransformationExceptions,

        long Loaded,
        long LoadingExceptions
    ) { };
}

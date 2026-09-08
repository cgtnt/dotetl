# Engine architecture
The engine consists of two worker groups and two additional workers (where each worker is a thread):

| Group | Purpose |
|:---|:-- |
| Extraction group | Repeatedly extracts `TDataIn` from a `IExtractionSource<TDataIn>`, then filters which `TData` is enqueued to the next stage by applying the provided `IValidator<TData>`. |
| Transformation group | Dequeues `TDataIn` from the previous queue, applies the transformation and enqueues the resulting `TDataOut` for loading. |
| Loading worker | Dequeues `TDataOut` from the previous queue, and calls `ILoadingDestination<TDataOut>`. |
| Exception loading worker | Dequeus any data-processing exceptions in the exception queue, and loads it to a separate `ILoadingDestination<Exception>`. |

Each is managed by a ThreadWorkerCoordinator, and the different stages ie. groups are connected by a 
`ThreadSafeBoundedQueue`, using a **producer-consumer** pattern.

The queue is implemented using condition variables. Note: once the producer finished producing elements, it is necessary to call `CompleteInput` on the queue, so the consumers don't keep waiting indefinitely for new input. Items are enqueued into the queues using a `Batch` system to reduce locking overhead.

The engine uses `ErrorQueueEnqueuerExceptionHandler` for row-wise data processing exceptions, eventually loading them to the user-provided `ILoadingDestination<Exception>`. For uncaught exceptions which occur in the `IWorker`'s `Work()` routines, `StreamSerializerExceptionHandler` is used, printing them to `Console.Out`.  

### Metrics 
The engine records execution metrics for each stage of the ETL process, specifically each `ThreadWorkerCoordinator` aggregates individual `WorkerMetrics` objects from each worker it manages, then returns this object (this object is for internal use). The engine then creates an `EngineMetrics` objects to be reported to the user.

# Standard library
The framework provides a standard library of implementations of different components for the users. 

### Interfaces
Interfaces of the individual components are in the `ETLEngine.Interfaces` namespace.

### Sepration of concerns
Implementations of `IExtractionSource` and `ILoaderDestination` deliberately separate serialization of a single `TData` object from formatting of the underlying stream, ie. separation of `IParser` and `ISerizalier` from `IStreamReaderRecordReader` and `IStreamWriterRecordWriter`. 

### Parser implementations
The standard library parsers are implemented using reflection, discovering `PropertyInfo` objects once and caching them for parsing of each row.
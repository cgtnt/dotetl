using ETLEngine.Interfaces;
using System.Diagnostics;

namespace ETLEngine
{
    internal record class EngineConfiguration<Tin, TOut>(
        IValidator<Tin> validator,
        ITransformer<Tin, TOut> transformer,
        int transformationBatchSize,
        int transformationMaxBytes,
        int serializationBatchSize,
        int serializationMaxBytes,
        int exceptionMaxBytes,
        int maxExtractionWorkers,
        int maxTransformationWorkers
    );

    
    internal class EngineExecutionContext<TIn, TOut>
    {
        private readonly EngineConfiguration<TIn, TOut> config_;

        private ThreadSafeBoundedQueue<Batch<TIn>> transformationQueue_;
        private ThreadSafeBoundedQueue<Batch<TOut>> serializationQueue_;
        private ThreadSafeBoundedQueue<Exception> dataProcessingExceptionsQueue_;
        private ThreadSafeBoundedQueue<IExtractionSource<TIn>> extractorQueue_;

        private int dataSourceCount_;

        private Stopwatch stopwatch_ = new();
        private DateTime startTime_;
        private DateTime endTime_;
        private TimeSpan executionTime_;

        private ILoadingDestination<TOut> outputLoader_;
        private ILoadingDestination<Exception> dataProcessingExceptionsLoader_;

        private ErrorQueueEnqueuerExceptionHandler dataProcessingExceptionHandler_;
        private StreamSerializerExceptionHandler uncaughtExceptionHandler_;

        private ThreadWorkerCoordinator? extractionWorkersGroup_;
        private ThreadWorkerCoordinator? transformationWorkersGroup_;
        private ThreadWorkerCoordinator? loadingWorker_;
        private ThreadWorkerCoordinator? dataProcessingExceptionLoadingWorker_;

        private enum ContextState {
            WorkersNotBuilt,
            WorkersNotStarted,
            WorkersRunning,
            WorkersComplete
        }
        private ContextState state_ = ContextState.WorkersNotBuilt;

        public EngineExecutionContext(
            EngineConfiguration<TIn, TOut> config,
            ICollection<IExtractionSource<TIn>> dataExtractors,
            ILoadingDestination<TOut> outputLoader,
            ILoadingDestination<Exception> dataProcessingExceptionsLoader
        ) {
            config_ = config;
            outputLoader_ = outputLoader;
            dataProcessingExceptionsLoader_ = dataProcessingExceptionsLoader;

            transformationQueue_ = new(config_.transformationMaxBytes);
            serializationQueue_ = new(config_.serializationMaxBytes);
            dataProcessingExceptionsQueue_ = new(config_.exceptionMaxBytes);

            dataSourceCount_ = dataExtractors.Count;
            extractorQueue_ = new(dataExtractors.Count);
            foreach (var extractor in dataExtractors)
                extractorQueue_.Enqueue(extractor);
            extractorQueue_.CompleteInput();

            dataProcessingExceptionHandler_ = new(dataProcessingExceptionsQueue_);
            uncaughtExceptionHandler_ = new(Console.Out);
        }

        private void BuildExtractionWorkerGroup()
        {
            List<BatchExtractionWorker<TIn>> extractionWorkers = new();
            for (int i = 0; i < Math.Min(config_.maxExtractionWorkers, dataSourceCount_); ++i)
                extractionWorkers.Add(new BatchExtractionWorker<TIn>(
                        config_.validator,
                        extractorQueue_,
                        transformationQueue_,
                        config_.transformationBatchSize,
                        dataProcessingExceptionHandler_
                    )
                );

            extractionWorkersGroup_ = new(extractionWorkers, uncaughtExceptionHandler_);
        }

        private void BuildTransformationWorkerGroup() {
            List<BatchTransformationWorker<TIn, TOut>> transformationWorkers = new();
            for (int i = 0; i < config_.maxTransformationWorkers; ++i)
                transformationWorkers.Add(new BatchTransformationWorker<TIn, TOut>(
                    transformationQueue_,
                    config_.transformer,
                    serializationQueue_,
                    config_.serializationBatchSize,
                    dataProcessingExceptionHandler_
                )
            );

            transformationWorkersGroup_ = new(transformationWorkers, uncaughtExceptionHandler_);
        }

        private void BuildLoadingWorker()
        {
            loadingWorker_ = new(
                new BatchLoadingWorker<TOut>(
                    serializationQueue_,
                    outputLoader_,
                    dataProcessingExceptionHandler_
                ),
                uncaughtExceptionHandler_
            );
        }

        private void BuildDataProcessingExceptionSerializerWorker()
        {
            dataProcessingExceptionLoadingWorker_ = new(
                new LoadingWorker<Exception>(
                    dataProcessingExceptionsQueue_,
                    dataProcessingExceptionsLoader_
                ),
                uncaughtExceptionHandler_
            );
        }

        public void BuildWorkerGroups()
        {
            if (state_ != ContextState.WorkersNotBuilt)
                throw new ApplicationException($"{nameof(BuildWorkerGroups)} can only be called if the workers have not been built yet");

            BuildExtractionWorkerGroup();
            BuildTransformationWorkerGroup();
            BuildLoadingWorker();
            BuildDataProcessingExceptionSerializerWorker();

            state_ = ContextState.WorkersNotStarted;
        }

        public void StartWorkerGroups()
        {
            if (state_ != ContextState.WorkersNotStarted)
                throw new ApplicationException($"{nameof(StartWorkerGroups)} can only be called if the worker groups have been built and there is no ongoing processing. Call {nameof(BuildWorkerGroups)} before this method.");

            startTime_ = DateTime.UtcNow;
            stopwatch_.Start();
            
            extractionWorkersGroup_.Start();
            transformationWorkersGroup_.Start();
            loadingWorker_.Start();
            dataProcessingExceptionLoadingWorker_.Start();

            state_ = ContextState.WorkersRunning;
        }

        public void WaitForCompletion()
        {
            if (state_ != ContextState.WorkersRunning)
                throw new ApplicationException($"{nameof(WaitForCompletion)} can only be called if processing is ongoing. Call {nameof(StartWorkerGroups)} before this method.");

            extractionWorkersGroup_.Join();
            transformationQueue_.CompleteInput();

            transformationWorkersGroup_.Join();
            serializationQueue_.CompleteInput();

            loadingWorker_.Join();
            dataProcessingExceptionsQueue_.CompleteInput();

            dataProcessingExceptionLoadingWorker_.Join();

            endTime_ = DateTime.UtcNow;
            stopwatch_.Stop();
            executionTime_ = stopwatch_.Elapsed;
            stopwatch_.Reset();

            state_ = ContextState.WorkersComplete;
        }

        public EngineMetrics CalculateMetrics()
        {
            if (state_ != ContextState.WorkersComplete)
                throw new ApplicationException($"{nameof(CalculateMetrics)} can only be called once the processing is done. Call {nameof(WaitForCompletion)} before this method.");

            var extractionMetrics = extractionWorkersGroup_.Metrics;
            var transformationMetrics = transformationWorkersGroup_.Metrics;
            var loadingMetrics = loadingWorker_.Metrics;
            
            EngineMetrics metrics = new(
                startTime_,
                endTime_,
                executionTime_,
                
                extractionMetrics.Exceptions,

                extractionMetrics.UnitsSuccess,
                extractionMetrics.UnitsFail,

                transformationMetrics.UnitsSuccess,
                transformationMetrics.Exceptions,

                loadingMetrics.UnitsSuccess,
                loadingMetrics.Exceptions
            );

            state_ = ContextState.WorkersNotBuilt;

            return metrics;
        }
    }
    
    /// <summary>
    /// An Extract-Transform-Load (ETL) multithreaded data processing engine. 
    /// See <seealso cref="EngineBuilder{TIn, TOut}"/> or <seealso cref="EngineBuilder{TData}"/> to construct.
    /// </summary>
    /// <typeparam name="TIn">Type of input objects extracted by the engine.</typeparam>
    /// <typeparam name="TOut">Type of output objects loaded by the engine.</typeparam>
    public class Engine<TIn, TOut>
    {
        private readonly EngineConfiguration<TIn, TOut> config_;
        internal Engine(EngineConfiguration<TIn, TOut> config) => config_ = config;

        /// <summary>
        /// Runs the configured ETL pipeline and waits for completion.
        /// </summary>
        /// <param name="dataExtractors">Extractor objects representing data sourcs from which the engine extracts objects.</param>
        /// <param name="outputLoader">Loader object representing the destination to which the engine loads objects after the ET pipeline.</param>
        /// <param name="dataProcessingExceptionsLoader">Loader object representing the destination to which the engine should load data-processing related exceptions.</param>
        /// <returns>Metrics of the ETL pipeline execution.</returns>
        public EngineMetrics Process(
            ICollection<IExtractionSource<TIn>> dataExtractors,
            ILoadingDestination<TOut> outputLoader,
            ILoadingDestination<Exception> dataProcessingExceptionsLoader
        ) {
            EngineExecutionContext<TIn, TOut> context = new(
                config_,
                dataExtractors,
                outputLoader,
                dataProcessingExceptionsLoader
            );

            context.BuildWorkerGroups();

            context.StartWorkerGroups();

            context.WaitForCompletion();

            return context.CalculateMetrics();
        }
    }

    /// <summary>
    /// A builder for <seealso cref="Engine{TData, TData}"/>.
    /// </summary>
    /// <typeparam name="TData">Type of objects extracted and loaded by the engine.</typeparam>
    public class EngineBuilder<TData> : EngineBuilder<TData, TData>
    {
        public EngineBuilder() : base()
        {
            transformer_ = new IdentityTransformer<TData>();
        }
    }

    /// <summary>
    /// A builder for <seealso cref="Engine{TIn, TOut}"/>.
    /// </summary>
    /// <typeparam name="TIn">Type of objects extracted by the engine.</typeparam>
    /// <typeparam name="TOut">Type of objects loaded by the engine.</typeparam>
    public class EngineBuilder<TIn, TOut>
    {
        private const int DefaultTransformationBatchSize = 8192;
        private const int DefaultTransformationQueueMaxBatches = 64;
        private const int DefaultSerializationBatchSize = 8192;
        private const int DefaultSerializationQueueMaxBatches = 64;
        private const int DefaultExceptionsQueueMaxItems = 64;

        protected IValidator<TIn> validator_ = new StandardLibrary.Validators.AlwaysValidValidator<TIn>();
        protected ITransformer<TIn, TOut>? transformer_;

        private int transformationBatchSize = DefaultTransformationBatchSize;
        private int transformationQueueMaxBatches = DefaultTransformationQueueMaxBatches;
        private int serializationBatchSize = DefaultSerializationBatchSize;
        private int serializationQueueMaxBatches = DefaultSerializationQueueMaxBatches;
        private int exceptionsQueueMaxItems = DefaultExceptionsQueueMaxItems;

        private int maxExtractionWorkers = 4;
        private int maxTransformationWorkers = Math.Max(1, Environment.ProcessorCount - 1);

        /// <summary>
        /// Sets the validation pipeline which the engine will use to filter extracted objects.
        /// </summary>
        /// <param name="validationPipeline">Provided validation pipeline</param>
        /// <returns>Reference to this <seealso cref="EngineBuilder{TIn, TOut}"/></returns>
        public EngineBuilder<TIn, TOut> WithValidator(IValidator<TIn> validationPipeline)
        {
            validator_ = validationPipeline;
            return this;
        }

        /// <summary>
        /// Sets the transformation pipeline which the engine will apply to each object that passed validation.
        /// </summary>
        /// <param name="transformationPipeline">Provided transformation pipeline</param>
        /// <returns>Reference to this <seealso cref="EngineBuilder{TIn, TOut}"/></returns>
        public EngineBuilder<TIn, TOut> WithTransformer(ITransformer<TIn, TOut> transformationPipeline)
        {
            transformer_ = transformationPipeline;
            return this;
        }

        /// <summary>
        /// Sets the size of batches the engine's transformation worker threads will use. 
        /// </summary>
        /// <param name="batchSize">Number of objects in batch</param>
        /// <returns>Reference to this <seealso cref="EngineBuilder{TIn, TOut}"/></returns>
        public EngineBuilder<TIn, TOut> WithTransformationBatchSize(int batchSize)
        {
            transformationBatchSize = batchSize;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of batches the engine's internal transformation queue will hold at a time. This limits the memory footprint of the engine.
        /// </summary>
        /// <param name="maxBatches">Batch queue capacity</param>
        /// <returns>Reference to this <seealso cref="EngineBuilder{TIn, TOut}"/></returns>
        public EngineBuilder<TIn, TOut> WithTransformationQueueMaxBatches(int maxBatches)
        {
            transformationQueueMaxBatches = maxBatches;
            return this;
        }

        /// <summary>
        /// Sets the size of batches the engine's loading worker threads will use. 
        /// </summary>
        /// <param name="maxBatches">Batch queue capacity</param>
        /// <returns>Reference to this <seealso cref="EngineBuilder{TIn, TOut}"/></returns>
        public EngineBuilder<TIn, TOut> WithLoadingBatchSize(int batchSize)
        {
            serializationBatchSize = batchSize;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of batches the engine's internal loading queue will hold at a time. This limits the memory footprint of the engine.
        /// </summary>
        /// <param name="maxBatches">Batch queue capacity</param>
        /// <returns>Reference to this <seealso cref="EngineBuilder{TIn, TOut}"/></returns>
        public EngineBuilder<TIn, TOut> WithLoadingQueueMaxBatches(int maxBatches)
        {
            serializationQueueMaxBatches = maxBatches;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of exception the engine's internal data-processing exception queue will hold at a time. This limits the memory footprint of the engine.
        /// </summary>
        /// <param name="maxItems">Queue capacity</param>
        /// <returns>Reference to this <seealso cref="EngineBuilder{TIn, TOut}"/></returns>
        public EngineBuilder<TIn, TOut> WithExceptionsQueueMaxItems(int maxItems)
        {
            exceptionsQueueMaxItems = maxItems;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of extraction worker threads the engine will use.
        /// </summary>
        /// <param name="maxWorkers">Maximum number of workers</param>
        /// <returns>Reference to this <seealso cref="EngineBuilder{TIn, TOut}"/></returns>
        public EngineBuilder<TIn, TOut> WithMaxExtractionWorkers(int maxWorkers)
        {
            maxExtractionWorkers = maxWorkers;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of transformation worker threads the engine will use.
        /// </summary>
        /// <param name="maxWorkers">Maximum number of workers</param>
        /// <returns>Reference to this <seealso cref="EngineBuilder{TIn, TOut}"/></returns>
        public EngineBuilder<TIn, TOut> WithMaxTransformationWorkers(int maxWorkers)
        {
            maxTransformationWorkers = maxWorkers;
            return this;
        }

        /// <summary>
        /// Validates the set builder configuration and build the engine.
        /// </summary>
        /// <returns>The ETL engine</returns>
        /// <exception cref="ApplicationException">Thrown if no <seealso cref="ITransformer{TIn, TOut}"/> was set using <seealso cref="WithTransformer(ITransformer{TIn, TOut})"/></exception>
        public Engine<TIn, TOut> Build()
        {
            if (transformer_ is null)
                throw new ApplicationException($"A transformation pipeline from {nameof(TIn)} to {nameof(TOut)} must be set using the {nameof(WithTransformer)} method");

            return new Engine<TIn, TOut>(
                new EngineConfiguration<TIn, TOut>(
                    validator_,
                    transformer_,
                    transformationBatchSize,
                    transformationQueueMaxBatches,
                    serializationBatchSize,
                    serializationQueueMaxBatches,
                    exceptionsQueueMaxItems,
                    maxExtractionWorkers,
                    maxTransformationWorkers
                )
            );
        }
    }
}

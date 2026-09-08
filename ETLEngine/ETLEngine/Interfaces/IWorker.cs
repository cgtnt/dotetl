namespace ETLEngine.Interfaces
{
    internal interface IWorker
    {
        /// <summary>
        /// Starts the worker.
        /// </summary>
        void Work();

        /// <summary>
        /// Metrics produced by a <seealso cref="Work"/> run.
        /// </summary>
        WorkerMetrics? Metrics { get; }
    }

    internal class BatchExtractionWorker<TIn>(
        IValidator<TIn> validator,
        ThreadSafeBoundedQueue<IExtractionSource<TIn>> inputStreams,
        ThreadSafeBoundedQueue<Batch<TIn>> queue,
        int batchCapacity,
        IExceptionHandler exceptionHandler
    ) : IWorker
    {
        private WorkerMetrics metrics_;
        public WorkerMetrics? Metrics { get => metrics_; }

        public void Work()
        {
            BatchEnqueuer<TIn> batchEnqueuer = new(queue, batchCapacity);

            while (inputStreams.TryDequeue(out IExtractionSource<TIn> extractor))
            {
                foreach (InstanceProcessingResult<TIn> result in extractor.Extract())
                try
                {
                    if (result.IsSuccess)
                    {
                        if (validator.Validate(result.Data!))
                        {
                            batchEnqueuer.EnqueueElement(result.Data!);
                            metrics_.UnitsSuccess++;
                        } else
                        {
                            metrics_.UnitsFail++;
                        }
                    } else
                    {
                        exceptionHandler.Handle(result.Exception!);
                        metrics_.Exceptions++;
                    }
                } 
                catch (Exception ex) 
                {
                    exceptionHandler.Handle(new Exception($"[VALIDATION] {ex.Message}"));
                }
            }

            batchEnqueuer.Dispose();
        }
    }

    internal class BatchTransformationWorker<TIn, TOut>(
        ThreadSafeBoundedQueue<Batch<TIn>> inQueue,
        ITransformer<TIn, TOut> transformator,
        ThreadSafeBoundedQueue<Batch<TOut>> outQueue,
        int outBatchCapacity,
        IExceptionHandler exceptionHandler
    ) : IWorker
    {
        private WorkerMetrics metrics_;
        public WorkerMetrics? Metrics { get => metrics_; }

        public void Work()
        {
            BatchEnqueuer<TOut> outBatchEnqueuer = new(outQueue, outBatchCapacity);

            while (inQueue.TryDequeue(out Batch<TIn> inBatch))
            {
                foreach (TIn inVal in inBatch)
                try
                {
                    outBatchEnqueuer.EnqueueElement(transformator.Transform(inVal));
                    metrics_.UnitsSuccess++;
                } catch (Exception ex)
                {
                    metrics_.Exceptions++;
                    exceptionHandler.Handle(new Exception($"[TRANSFORMATION] {ex.Message}"));
                }
            }

            outBatchEnqueuer.Dispose();
        }
    }

    internal class BatchLoadingWorker<TOut>(
        ThreadSafeBoundedQueue<Batch<TOut>> inQueue,
        ILoadingDestination<TOut> outputLoader,
        IExceptionHandler exceptionHandler
    ) : IWorker
    {
        private WorkerMetrics metrics_;
        public WorkerMetrics? Metrics { get => metrics_; }

        public void Work()
        {
            while(inQueue.TryDequeue(out Batch<TOut> inBatch))
            {
                try
                {
                    outputLoader.Load(inBatch);
                    metrics_.UnitsSuccess += inBatch.Size;
                } catch (Exception ex) 
                {
                    exceptionHandler.Handle(new Exception($"[[LOADING BATCH]] \n {ex.Message}"));
                    metrics_.Exceptions++;
                }
            }
        }
    }

    internal class LoadingWorker<TOut>(
        ThreadSafeBoundedQueue<TOut> inQueue,
        ILoadingDestination<TOut> outputLoader
    ) : IWorker
    {
        public WorkerMetrics? Metrics { get => null; }

        public void Work()
        {
            while (inQueue.TryDequeue(out TOut data))
            {
                outputLoader.Load([data]); //FIXME: this is ugly
            }
        }
    }
}

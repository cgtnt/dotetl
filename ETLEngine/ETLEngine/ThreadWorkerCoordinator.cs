namespace ETLEngine
{
    using ETLEngine.Interfaces;
    using System.Threading;

    internal class ThreadWorkerCoordinator(
        IEnumerable<IWorker> workers,
        IExceptionHandler threadExceptionHandler
    ) {
        private List<Thread> threads = new();

        public ThreadWorkerCoordinator(IWorker worker, IExceptionHandler threadExceptionHandler) 
            : this([worker], threadExceptionHandler) { }

        /// <summary>
        /// Aggregated <seealso cref="WorkerMetrics"/> from all <seealso cref="IWorker"s/>.
        /// </summary>
        public WorkerMetrics Metrics { get; private set; } = new();

        /// <summary>
        /// Creates a thread for each given <seealso cref="IWorker"/> and starts each thread. In case a thread throws an exception, the given <seealso cref="IExceptionHandler"/> is called. The <seealso cref="ThreadWorkerCoordinator"/> does not rethrow the exception or terminate.
        /// </summary>
        public void Start()
        {
            foreach (IWorker worker in workers)
                threads.Add(new Thread(
                    () => {
                        try
                        {
                            worker.Work(); 
                        } catch (Exception ex)
                        {
                            threadExceptionHandler.Handle(new ApplicationException($"[FATAL] Thread worker shutting down: {ex.Message}"));
                        }
                    }
                ));

            foreach (Thread thread in threads)
                thread.Start();
        }

        /// <summary>
        /// Calls Join on each thread, once all threads have joined, aggregates the <seealso cref="WorkerMetrics"/> from each and sets <seealso cref="Metrics"/>.
        /// </summary>
        public void Join()
        {
            List<WorkerMetrics> metrics = new();

            foreach (Thread thread in threads)
                thread.Join();

            foreach (IWorker worker in workers)
            if (worker.Metrics is WorkerMetrics metric)
                metrics.Add(metric);

            if (metrics.Count != 0)
                Metrics = metrics.Aggregate(
                    (WorkerMetrics left, WorkerMetrics right) => left + right
                );
        }
    }
}

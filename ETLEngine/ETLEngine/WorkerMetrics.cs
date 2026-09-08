namespace ETLEngine
{
    /// <summary>
    /// Metrics of a single <seealso cref="IWorker"/>.Work() call.
    /// </summary>
    internal struct WorkerMetrics
    {
        /// <summary>
        /// Number of work units that succeeded.
        /// </summary>
        public long UnitsSuccess;

        /// <summary>
        /// Number of work units that failed.
        /// </summary>
        public long UnitsFail;

        /// <summary>
        /// Number of work units that threw an exception.
        /// </summary>
        public long Exceptions;

        public static WorkerMetrics operator+(WorkerMetrics left, WorkerMetrics right)
        {
            WorkerMetrics result = new();
            result.UnitsSuccess = left.UnitsSuccess + right.UnitsSuccess;
            result.UnitsFail = left.UnitsFail + right.UnitsFail;
            result.Exceptions = left.Exceptions + right.Exceptions;
            return result;
        }
    }
}

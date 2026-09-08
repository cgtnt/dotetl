using System.Collections;

namespace ETLEngine
{
    /// <summary>
    /// A type representing the outcome of a processing operation. Either holds <see cref="TData"/> or an Exception
    /// </summary>
    /// <typeparam name="TData">Type of object the result will hold if processing was successful</typeparam>
    public struct InstanceProcessingResult<TData>
    {
        /// <summary>
        /// Processing result if processing was succesful.
        /// </summary>
        public TData? Data { get; }

        /// <summary>
        /// Processing result if processing failed.
        /// </summary>
        public Exception? Exception { get;  } = null;

        /// <summary>
        /// Checks whether the processing was succesful or not.
        /// </summary>
        public bool IsSuccess => Exception == null;

        /// <summary>
        /// Constructor to be used if the processing operation was succesful.
        /// </summary>
        /// <param name="data">Result</param>
        public InstanceProcessingResult(TData data) { Data = data; }

        /// <summary>
        /// Constructor to be used if the processing operation was unsuccesful.
        /// </summary>
        /// <param name="ex">Processing exception</param>
        public InstanceProcessingResult(Exception ex) { Exception = ex; }
    }

    internal struct Batch<TData>(int capacity) : IEnumerable<TData>
    {
        private readonly TData[] data_ = new TData[capacity];

        /// <summary>
        /// Current number of items in the batch.
        /// </summary>
        public int Size { get; private set; } = 0;
        
        /// <summary>
        /// Capacity of the batch.
        /// </summary>
        public int Capacity { get => capacity; }

        public TData this[int index]
        {
            get => data_[index];
            set => data_[index] = value;
        }

        /// <summary>
        /// Adds an object to the batch.
        /// </summary>
        /// <param name="value">Object to add</param>
        /// <exception cref="ApplicationException">Thrown if batch is full</exception>
        public void Add(TData value)
        {
            if (Size == Capacity)
                throw new ApplicationException("Batch is full");

            data_[Size++] = value;
        }

        public IEnumerator<TData> GetEnumerator()
        {
            for (int i = 0; i < Size; ++i)
                yield return data_[i];
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    internal class BatchEnqueuer<TData>(ThreadSafeBoundedQueue<Batch<TData>> queue, int batchCapacity) : IDisposable
    {
        private Batch<TData> currentBatch_ = new(batchCapacity);
        private bool disposed_ = false;

        /// <summary>
        /// Adds an element to the current batch. If the current batch is full, enqueues the current batch to the queue and adds the element to the next batch.
        /// </summary>
        /// <param name="element">Element to add</param>
        /// <exception cref="ObjectDisposedException">Thrown if <seealso cref="BatchEnqueuer{TData}"/> was disposed.</exception>
        public void EnqueueElement(TData element)
        {
            if (disposed_)
                throw new ObjectDisposedException(nameof(BatchEnqueuer<TData>));

            if (currentBatch_.Size == batchCapacity)
            {
                queue.Enqueue(currentBatch_);
                currentBatch_ = new Batch<TData>(batchCapacity);
            }

            currentBatch_.Add(element);
        }

        /// <summary>
        /// Disposes of the enqueuer and enqueues the remaining batch elements to the queue.
        /// </summary>
        public void Dispose()
        {
            if (disposed_)
                return;

            if (currentBatch_.Size > 0)
                queue.Enqueue(currentBatch_);

            disposed_ = true;
        }
    }
}

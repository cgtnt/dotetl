namespace ETLEngine
{
    /// <summary>
    /// A thread-safe implementation of a capacity-bounded queue.
    /// </summary>
    /// <typeparam name="TData">Type of items in the queue</typeparam>
    /// <param name="capacity">Capacity of the queue</param>
    internal class ThreadSafeBoundedQueue<TData>(int capacity)
    {
        private readonly object sync_ = new object();
        private readonly Queue<TData> queue_ = new();
        private bool completed_ = false;

        /// <summary>
        /// Enqueues an element into the queue. If the queue is full, blocks the current thread until signaled by a <seealso cref="TryDequeue(out TData)"/> operation.
        /// </summary>
        /// <param name="data">Element to enqueue</param>
        /// <exception cref="ApplicationException">Thrown if the queue was alredy marked as Completed</exception>
        public void Enqueue(TData data)
        {
            lock (sync_)
            {
                if (completed_)
                    throw new ApplicationException($"Tried to enqueue new {nameof(TData)} into {nameof(ThreadSafeBoundedQueue<TData>)} which was marked as completed");

                while (queue_.Count >= capacity)
                    Monitor.Wait(sync_);

                queue_.Enqueue(data);
            
                Monitor.Pulse(sync_);
            }
        }

        /// <summary>
        /// Marks the queue as completed. After this, calling <seealso cref="Enqueue(TData)"/> will throw an exception. Calling <seealso cref="TryDequeue(out TData)"/> is still permitted and will dequeue any items still in the queue.
        /// </summary>
        public void CompleteInput()
        {
            lock (sync_)
            {
                completed_ = true;
                Monitor.PulseAll(sync_);
            }
        }

        /// <summary>
        /// Tries to dequeue an element from the queue. If there are no items in the queue, blocks the calling thread until <seealso cref="Enqueue(TData)"/> signals that an item has been enqueued.
        /// </summary>
        /// <param name="dequeued">Output of the dequeued element</param>
        /// <returns>False if the queue is empty and was marked as completed, otherwise true</returns>
        public bool TryDequeue(out TData dequeued)
        {
            lock (sync_)
            {
                while (queue_.Count == 0 && !completed_)
                    Monitor.Wait(sync_);

                if (queue_.Count == 0 && completed_)
                {
                    dequeued = default(TData);
                    return false;
                }

                TData data = queue_.Dequeue();
                Monitor.PulseAll(sync_);
                
                dequeued = data;
                return true;
            }
        }

    }
}

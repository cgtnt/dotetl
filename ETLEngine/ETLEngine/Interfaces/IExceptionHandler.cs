namespace ETLEngine.Interfaces
{
    internal interface IExceptionHandler
    {
        void Handle(Exception ex);
    }

    /// <summary>
    /// Enqueues an exception to the given <seealso cref="ThreadSafeBoundedQueue{Exception}"/>. 
    /// </summary>
    /// <param name="exceptions">Exceptions queue</param>
    internal class ErrorQueueEnqueuerExceptionHandler(
        ThreadSafeBoundedQueue<Exception> exceptions
    ) : IExceptionHandler
    {
        public void Handle(Exception ex) => exceptions.Enqueue(ex);
    }

    /// <summary>
    /// Writes an exception to the given TextWriter.
    /// </summary>
    /// <param name="output"></param>
    internal class StreamSerializerExceptionHandler(
        TextWriter output
    ) : IExceptionHandler
    {
        public void Handle(Exception ex) => output.WriteLine(ex);
    }
}
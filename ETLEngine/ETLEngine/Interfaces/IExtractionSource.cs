namespace ETLEngine.Interfaces
{
    /// <summary>
    /// Represents a data source to be used by the <seealso cref="Engine{TIn, TOut}"/> to extract objects.
    /// </summary>
    /// <typeparam name="TData">Type of objects to be extracted</typeparam>
    public interface IExtractionSource<TData>
    {
        IEnumerable<InstanceProcessingResult<TData>> Extract();
    }
}
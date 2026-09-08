namespace ETLEngine.Interfaces
{
    /// <summary>
    /// Represents a loading destination to be used by the <seealso cref="Engine{TIn, TOut}"/> to load objects after transformation.
    /// </summary>
    /// <typeparam name="TData">Type of objects to be loaded</typeparam>
    public interface ILoadingDestination<TData>
    {
        void Load(IEnumerable<TData> data);
    }
}

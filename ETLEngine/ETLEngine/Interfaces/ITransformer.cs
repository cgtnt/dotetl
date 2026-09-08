namespace ETLEngine.Interfaces
{
    /// <summary>
    /// Applies a transformation to a <seealso cref="TIn"/> to obtain a <seealso cref="TOut"/>.
    /// </summary>
    /// <typeparam name="TIn">Type of object the transformer reads.</typeparam>
    /// <typeparam name="TOut">Type of object the transformer outputs.</typeparam>
    public interface ITransformer<in TIn, out TOut>
    {
        public TOut Transform(TIn input);
    }

    /// <summary>
    /// A transformer that does nothing.
    /// </summary>
    /// <typeparam name="T">Type of object to do nothing to.</typeparam>
    public class IdentityTransformer<T> : ITransformer<T, T>
    {
        public T Transform(T input) => input;
    }
}

namespace ETLEngine.Interfaces
{
    /// <summary>
    /// Serializes a <seealso cref="TData"/> to string.
    /// </summary>
    /// <typeparam name="TData">Type of object to be serialized</typeparam>
    public interface IStreamWriterSerializer<in TData>
    {
        void Serialize(TData data, StreamWriter writer);
    }
}